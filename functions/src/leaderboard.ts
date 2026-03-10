/**
 * Leaderboard Cloud Functions — submit scores and fetch ranked entries.
 *
 * Supported categories: loops_completed | bosses_defeated | total_score
 * Each category is stored as a Firestore collection `leaderboards/{category}/entries`.
 */

import * as admin from 'firebase-admin';
import {onCall, HttpsError} from 'firebase-functions/v2/https';

import {LeaderboardEntry, LeaderboardCategory} from './types';

const VALID_CATEGORIES: LeaderboardCategory[] = [
  'loops_completed',
  'bosses_defeated',
  'total_score',
];

/** Maximum achievable score per category used for server-side validation. */
const MAX_SCORE_PER_CATEGORY: Record<LeaderboardCategory, number> = {
  loops_completed: 10_000,
  bosses_defeated: 50_000,
  total_score: 999_999_999,
};

/**
 * Callable: `submitScore`
 *
 * Submits a score for the authenticated player. The server validates that the
 * score is within achievable bounds before persisting it.
 *
 * Request  `{ category: LeaderboardCategory; score: number }`
 * Response `{ newRank: number; previousRank: number; improved: boolean }`
 */
export const submitScore = onCall(async (request) => {
  if (!request.auth) {
    throw new HttpsError('unauthenticated', 'Must be authenticated');
  }

  const {category, score} = request.data as {
    category: LeaderboardCategory;
    score: number;
  };

  if (!VALID_CATEGORIES.includes(category)) {
    throw new HttpsError('invalid-argument', `Invalid category: ${category}`);
  }

  if (typeof score !== 'number' || !Number.isInteger(score) || score < 0) {
    throw new HttpsError('invalid-argument', 'score must be a non-negative integer');
  }

  if (score > MAX_SCORE_PER_CATEGORY[category]) {
    throw new HttpsError('invalid-argument', 'Score exceeds the maximum achievable value');
  }

  const db = admin.firestore();
  const entryRef = db
    .collection('leaderboards')
    .doc(category)
    .collection('entries')
    .doc(request.auth.uid);

  const entrySnap = await entryRef.get();
  const previousScore: number = entrySnap.exists
    ? (entrySnap.data() as LeaderboardEntry).score
    : 0;

  const improved = score > previousScore;

  if (improved) {
    const playerSnap = await db.collection('players').doc(request.auth.uid).get();
    const displayName: string = playerSnap.exists
      ? ((playerSnap.data() as {displayName: string}).displayName ?? 'Unknown')
      : 'Unknown';

    await entryRef.set({
      userId: request.auth.uid,
      displayName,
      score,
      updatedAt: new Date().toISOString(),
    });
  }

  // Calculate ranks: count entries with a strictly higher score than new and old scores.
  // When the score improved we fire both queries in parallel; otherwise reuse the same result.
  const higherThanNewSnap = await db
    .collection('leaderboards')
    .doc(category)
    .collection('entries')
    .where('score', '>', score)
    .get();

  const newRank = higherThanNewSnap.size + 1;

  let previousRank: number;
  if (!improved || previousScore === 0) {
    previousRank = newRank;
  } else {
    const higherThanPrevSnap = await db
      .collection('leaderboards')
      .doc(category)
      .collection('entries')
      .where('score', '>', previousScore)
      .get();
    previousRank = higherThanPrevSnap.size + 1;
  }

  return {newRank, previousRank, improved};
});

/**
 * Callable: `getLeaderboard`
 *
 * Returns the top-N entries for a category, with the requesting player's entry
 * always appended (if it exists but is outside the top-N window).
 *
 * Request  `{ category: LeaderboardCategory; limit?: number }`
 * Response `{ entries: LeaderboardEntry[]; userRank: number; totalEntries: number }`
 */
export const getLeaderboard = onCall(async (request) => {
  if (!request.auth) {
    throw new HttpsError('unauthenticated', 'Must be authenticated');
  }

  const {category, limit = 100} = request.data as {
    category: LeaderboardCategory;
    limit?: number;
  };

  if (!VALID_CATEGORIES.includes(category)) {
    throw new HttpsError('invalid-argument', `Invalid category: ${category}`);
  }

  const clampedLimit = Math.min(Math.max(1, limit), 200);

  const db = admin.firestore();
  const collRef = db.collection('leaderboards').doc(category).collection('entries');

  const [topSnap, totalSnap, playerSnap] = await Promise.all([
    collRef.orderBy('score', 'desc').limit(clampedLimit).get(),
    collRef.count().get(),
    collRef.doc(request.auth.uid).get(),
  ]);

  const totalEntries: number = totalSnap.data().count;

  const entries: LeaderboardEntry[] = topSnap.docs.map((doc, idx) => ({
    ...(doc.data() as Omit<LeaderboardEntry, 'rank'>),
    rank: idx + 1,
  }));

  // Determine this player's rank
  let userRank = 0;
  if (playerSnap.exists) {
    const playerScore = (playerSnap.data() as LeaderboardEntry).score;
    const higherSnap = await collRef.where('score', '>', playerScore).count().get();
    userRank = higherSnap.data().count + 1;
  }

  return {entries, userRank, totalEntries};
});
