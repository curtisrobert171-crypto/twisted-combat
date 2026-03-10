/**
 * Raid result Cloud Functions — process and validate PvP raid outcomes.
 *
 * All outcomes are written to /raidHistory/{raidId} and the defender's
 * /players/{uid} document is updated accordingly. Currency transfers happen
 * inside a Firestore transaction so they are atomic.
 */

import * as admin from 'firebase-admin';
import {onCall, HttpsError} from 'firebase-functions/v2/https';

import {PlayerData} from './types';

/** Maximum loot percentage that can be stolen in a single successful raid. */
const MAX_LOOT_FRACTION = 0.15;

/** Maximum number of raids a player can launch per hour (anti-abuse). */
const MAX_RAIDS_PER_HOUR = 20;

/**
 * Callable: `completeRaid`
 *
 * Finalises a raid, transfers loot, updates defender shield, and logs the
 * outcome. The server validates loot amounts against the defender's actual
 * wallet to prevent client-side spoofing.
 *
 * Request  `{ targetUserId: string; result: 'victory' | 'defeat'; damageDealt: number; lootClaimed: { coins: number; gems: number } }`
 * Response `{ success: boolean; revengeAvailable: boolean; newRating: number }`
 */
export const completeRaid = onCall(async (request) => {
  if (!request.auth) {
    throw new HttpsError('unauthenticated', 'Must be authenticated');
  }

  const {targetUserId, result, damageDealt, lootClaimed} = request.data as {
    targetUserId: string;
    result: 'victory' | 'defeat';
    damageDealt: number;
    lootClaimed: {coins: number; gems: number};
  };

  if (!targetUserId || typeof targetUserId !== 'string') {
    throw new HttpsError('invalid-argument', 'targetUserId is required');
  }
  if (result !== 'victory' && result !== 'defeat') {
    throw new HttpsError('invalid-argument', 'result must be "victory" or "defeat"');
  }
  if (typeof damageDealt !== 'number' || damageDealt < 0 || damageDealt > 100) {
    throw new HttpsError('invalid-argument', 'damageDealt must be 0–100');
  }

  const db = admin.firestore();
  const attackerId = request.auth.uid;
  const now = new Date().toISOString();
  const oneHourAgo = new Date(Date.now() - 60 * 60 * 1000).toISOString();

  // Rate-limit: count raids launched in the last hour
  const recentRaids = await db
    .collection('raidHistory')
    .where('attackerId', '==', attackerId)
    .where('completedAt', '>=', oneHourAgo)
    .count()
    .get();

  if (recentRaids.data().count >= MAX_RAIDS_PER_HOUR) {
    throw new HttpsError('resource-exhausted', 'Raid limit reached — try again later');
  }

  const raidId = db.collection('raidHistory').doc().id;

  let newRating = 1000;

  await db.runTransaction(async (tx) => {
    const attackerRef = db.collection('players').doc(attackerId);
    const defenderRef = db.collection('players').doc(targetUserId);

    const [attackerSnap, defenderSnap] = await Promise.all([
      tx.get(attackerRef),
      tx.get(defenderRef),
    ]);

    if (!attackerSnap.exists) throw new HttpsError('not-found', 'Attacker not found');
    if (!defenderSnap.exists) throw new HttpsError('not-found', 'Defender not found');

    const attacker = attackerSnap.data() as PlayerData;
    const defender = defenderSnap.data() as PlayerData;

    // Server-side loot validation: clamp to actual wallet × MAX_LOOT_FRACTION
    const maxCoins = Math.floor(defender.currencies.coins * MAX_LOOT_FRACTION);
    const maxGems = Math.floor(defender.currencies.gems * MAX_LOOT_FRACTION);
    const actualCoins = result === 'victory' ? Math.min(lootClaimed.coins, maxCoins) : 0;
    const actualGems = result === 'victory' ? Math.min(lootClaimed.gems, maxGems) : 0;

    // ELO-style rating update — computed and applied atomically inside the transaction
    const currentRating = (attacker as PlayerData & {rating?: number}).rating ?? 1000;
    newRating = Math.max(0, currentRating + (result === 'victory' ? 25 : -10));

    if (result === 'victory') {
      tx.update(attackerRef, {
        'currencies.coins': attacker.currencies.coins + actualCoins,
        'currencies.gems': attacker.currencies.gems + actualGems,
        'stats.raidsWon': admin.firestore.FieldValue.increment(1),
        rating: newRating,
      });
      tx.update(defenderRef, {
        'currencies.coins': Math.max(0, defender.currencies.coins - actualCoins),
        'currencies.gems': Math.max(0, defender.currencies.gems - actualGems),
        lastRaidedAt: now,
      });
    } else {
      tx.update(attackerRef, {rating: newRating});
      tx.update(defenderRef, {lastRaidedAt: now});
    }

    const raidRef = db.collection('raidHistory').doc(raidId);
    tx.set(raidRef, {
      raidId,
      attackerId,
      targetUserId,
      result,
      damageDealt,
      lootTransferred: {coins: actualCoins, gems: actualGems},
      completedAt: now,
    });
  });

  // A revenge flag is available when this player has previously been raided by the target
  const revengeSnap = await db
    .collection('raidHistory')
    .where('attackerId', '==', targetUserId)
    .where('targetUserId', '==', attackerId)
    .limit(1)
    .get();

  const revengeAvailable = !revengeSnap.empty;

  return {success: true, revengeAvailable, newRating};
});
