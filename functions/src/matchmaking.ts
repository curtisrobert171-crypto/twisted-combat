/**
 * PvP matchmaking Cloud Functions — find a raid target for the authenticated player.
 *
 * Targets are selected from the /players collection using a level-proximity
 * window so fights stay competitive. Players who have been raided recently
 * (within SHIELD_DURATION_MS) are excluded.
 */

import * as admin from 'firebase-admin';
import {onCall, HttpsError} from 'firebase-functions/v2/https';

import {PlayerData, RaidTarget} from './types';

/** Players who attacked recently cannot be chosen as targets. */
const SHIELD_DURATION_MS = 10 * 60 * 1000; // 10 minutes

/** How many candidates to fetch and then randomly pick one from. */
const CANDIDATE_POOL = 10;

/** Level window (±N) used when searching for suitable opponents. */
const LEVEL_WINDOW = 3;

/**
 * Callable: `findRaidTarget`
 *
 * Finds a suitable PvP target for the current player. The target's base layout
 * and potential loot are returned so the client can render the pre-raid preview.
 *
 * Request  `{}`
 * Response `RaidTarget`
 */
export const findRaidTarget = onCall(async (request) => {
  if (!request.auth) {
    throw new HttpsError('unauthenticated', 'Must be authenticated');
  }

  const db = admin.firestore();
  const attackerId = request.auth.uid;

  // Fetch attacker's level
  const attackerSnap = await db.collection('players').doc(attackerId).get();
  if (!attackerSnap.exists) {
    throw new HttpsError('not-found', 'Attacker player data not found');
  }

  const attacker = attackerSnap.data() as PlayerData;
  const minLevel = Math.max(1, attacker.level - LEVEL_WINDOW);
  const maxLevel = attacker.level + LEVEL_WINDOW;

  // Fetch candidates within the level range (excluding the attacker)
  const candidatesSnap = await db
    .collection('players')
    .where('level', '>=', minLevel)
    .where('level', '<=', maxLevel)
    .limit(CANDIDATE_POOL + 5) // over-fetch to account for filtered-out players
    .get();

  const now = Date.now();

  const eligible = candidatesSnap.docs.filter((doc) => {
    if (doc.id === attackerId) return false;
    const data = doc.data() as PlayerData & {lastRaidedAt?: string};
    if (!data.lastRaidedAt) return true;
    return now - new Date(data.lastRaidedAt).getTime() > SHIELD_DURATION_MS;
  });

  if (eligible.length === 0) {
    throw new HttpsError('not-found', 'No suitable opponent found — try again shortly');
  }

  // Pick a random candidate from the eligible pool
  const target = eligible[Math.floor(Math.random() * eligible.length)].data() as PlayerData;

  const lootBase = target.currencies.coins;
  const potentialCoins = Math.floor(lootBase * 0.1);
  const potentialGems = Math.floor(target.currencies.gems * 0.05);

  // Rough defence power: sum of building levels in the base layout
  const defensePower = (target.baseLayout ?? []).reduce(
    (sum, cell) => sum + (cell.level ?? 0),
    0,
  );

  const result: RaidTarget = {
    targetUserId: target.userId,
    displayName: target.displayName,
    baseLevel: target.level,
    baseLayout: target.baseLayout ?? [],
    potentialLoot: {coins: potentialCoins, gems: potentialGems},
    defensePower,
  };

  return result;
});
