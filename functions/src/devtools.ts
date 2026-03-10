/**
 * Game-developer tooling Cloud Functions.
 *
 * These functions are **restricted to admin callers** (Firebase custom claims
 * `{ admin: true }`) and are intended exclusively for use during development,
 * QA, and playtesting. They must never be exposed to regular players.
 *
 * Available tools:
 *  - `devResetPlayer`    — wipe a player's progress back to Day-1 defaults
 *  - `devGrantCurrency`  — inject gems / coins for playtesting economy balance
 *  - `devSeedLeaderboard`— populate a leaderboard with synthetic data for UI testing
 *  - `devGetMetrics`     — return live aggregate stats (DAU, raid count, etc.)
 */

import * as admin from 'firebase-admin';
import {onCall, HttpsError} from 'firebase-functions/v2/https';

import {LeaderboardCategory, PlayerData} from './types';

const VALID_CATEGORIES: LeaderboardCategory[] = [
  'loops_completed',
  'bosses_defeated',
  'total_score',
];

/** Asserts the caller holds the `admin` custom claim. */
async function requireAdmin(uid: string): Promise<void> {
  const user = await admin.auth().getUser(uid);
  const claims = user.customClaims as Record<string, unknown> | undefined;
  if (!claims?.admin) {
    throw new HttpsError('permission-denied', 'Admin claim required');
  }
}

/**
 * Callable: `devResetPlayer`
 *
 * Resets a player's progression to Day-1 defaults while preserving their UID
 * and display name. Useful for reproducing FTUE (first-time user experience)
 * bugs during QA.
 *
 * Request  `{ targetUserId: string }`
 * Response `{ success: boolean; resetAt: string }`
 */
export const devResetPlayer = onCall(async (request) => {
  if (!request.auth) throw new HttpsError('unauthenticated', 'Must be authenticated');
  await requireAdmin(request.auth.uid);

  const {targetUserId} = request.data as {targetUserId: string};
  if (!targetUserId) throw new HttpsError('invalid-argument', 'targetUserId is required');

  const db = admin.firestore();
  const playerRef = db.collection('players').doc(targetUserId);
  const snap = await playerRef.get();
  if (!snap.exists) throw new HttpsError('not-found', 'Player not found');

  const existing = snap.data() as PlayerData;
  const resetAt = new Date().toISOString();

  await playerRef.update({
    level: 1,
    xp: 0,
    currencies: {gems: 100, coins: 500, energy: 100},
    stats: {
      loopsCompleted: 0,
      bossesDefeated: 0,
      mathGatesHit: 0,
      totalPlaytime: 0,
      deaths: 0,
    },
    baseLayout: [],
    inventory: [],
    rating: 1000,
    lastResetAt: resetAt,
    displayName: existing.displayName,
  });

  return {success: true, resetAt};
});

/**
 * Callable: `devGrantCurrency`
 *
 * Injects gems and/or coins into a player's wallet. Used to rapidly test
 * monetization flows and economy-balance scenarios.
 *
 * Request  `{ targetUserId: string; gems?: number; coins?: number }`
 * Response `{ success: boolean; newCurrencies: { gems: number; coins: number; energy: number } }`
 */
export const devGrantCurrency = onCall(async (request) => {
  if (!request.auth) throw new HttpsError('unauthenticated', 'Must be authenticated');
  await requireAdmin(request.auth.uid);

  const {targetUserId, gems = 0, coins = 0} = request.data as {
    targetUserId: string;
    gems?: number;
    coins?: number;
  };

  if (!targetUserId) throw new HttpsError('invalid-argument', 'targetUserId is required');
  if (gems < 0 || coins < 0) throw new HttpsError('invalid-argument', 'Currency amounts must be non-negative');

  const db = admin.firestore();
  const playerRef = db.collection('players').doc(targetUserId);

  const newCurrencies = await db.runTransaction(async (tx) => {
    const snap = await tx.get(playerRef);
    if (!snap.exists) throw new HttpsError('not-found', 'Player not found');
    const data = snap.data() as PlayerData;
    const updated = {
      gems: (data.currencies?.gems ?? 0) + gems,
      coins: (data.currencies?.coins ?? 0) + coins,
      energy: data.currencies?.energy ?? 100,
    };
    tx.update(playerRef, {'currencies.gems': updated.gems, 'currencies.coins': updated.coins});
    return updated;
  });

  return {success: true, newCurrencies};
});

/**
 * Callable: `devSeedLeaderboard`
 *
 * Populates a leaderboard with a configurable number of synthetic bot entries.
 * Handy for testing leaderboard UI with realistic ranking data before real
 * players exist.
 *
 * Request  `{ category: LeaderboardCategory; count?: number }`
 * Response `{ success: boolean; seeded: number }`
 */
export const devSeedLeaderboard = onCall(async (request) => {
  if (!request.auth) throw new HttpsError('unauthenticated', 'Must be authenticated');
  await requireAdmin(request.auth.uid);

  const {category, count = 20} = request.data as {
    category: LeaderboardCategory;
    count?: number;
  };

  if (!VALID_CATEGORIES.includes(category)) {
    throw new HttpsError('invalid-argument', `Invalid category: ${category}`);
  }

  const clampedCount = Math.min(Math.max(1, count), 100);
  const db = admin.firestore();
  const collRef = db.collection('leaderboards').doc(category).collection('entries');
  const batch = db.batch();
  const now = new Date().toISOString();

  for (let i = 0; i < clampedCount; i++) {
    const suffix = `${Date.now()}_${i.toString().padStart(3, '0')}`;
    const botId = `bot_${category}_${suffix}`;
    const score = Math.floor(Math.random() * 10_000) + i * 100;
    batch.set(collRef.doc(botId), {
      userId: botId,
      displayName: `TestBot_${i + 1}`,
      score,
      updatedAt: now,
    });
  }

  await batch.commit();
  return {success: true, seeded: clampedCount};
});

/**
 * Callable: `devGetMetrics`
 *
 * Returns aggregate live-game metrics useful during playtests: total players,
 * raids completed today, and leaderboard entry counts.
 *
 * Request  `{}`
 * Response `{ totalPlayers: number; raidsToday: number; leaderboardCounts: Record<string, number>; generatedAt: string }`
 */
export const devGetMetrics = onCall(async (request) => {
  if (!request.auth) throw new HttpsError('unauthenticated', 'Must be authenticated');
  await requireAdmin(request.auth.uid);

  const db = admin.firestore();
  const todayStart = new Date();
  todayStart.setHours(0, 0, 0, 0);
  const todayIso = todayStart.toISOString();

  const [totalPlayersSnap, raidsTodaySnap, ...lbSnaps] = await Promise.all([
    db.collection('players').count().get(),
    db.collection('raidHistory').where('completedAt', '>=', todayIso).count().get(),
    ...VALID_CATEGORIES.map((cat) =>
      db.collection('leaderboards').doc(cat).collection('entries').count().get(),
    ),
  ]);

  const leaderboardCounts: Record<string, number> = {};
  VALID_CATEGORIES.forEach((cat, idx) => {
    leaderboardCounts[cat] = lbSnaps[idx].data().count;
  });

  return {
    totalPlayers: totalPlayersSnap.data().count,
    raidsToday: raidsTodaySnap.data().count,
    leaderboardCounts,
    generatedAt: new Date().toISOString(),
  };
});
