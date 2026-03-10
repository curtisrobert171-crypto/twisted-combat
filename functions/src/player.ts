/**
 * Player data Cloud Functions — cloud save and load with checksum validation.
 */

import * as admin from 'firebase-admin';
import * as crypto from 'crypto';
import {onCall, HttpsError} from 'firebase-functions/v2/https';

import {PlayerData} from './types';

/**
 * Derives a server-side HMAC-SHA256 checksum for a player-data JSON string.
 * The secret must match what the Unity client uses (set via the SAVE_HMAC_SECRET
 * environment variable).
 */
function computeChecksum(payload: string, userId: string): string {
  const secret = process.env.SAVE_HMAC_SECRET;
  if (!secret) {
    throw new HttpsError(
      'internal',
      'SAVE_HMAC_SECRET is not configured — contact the server administrator',
    );
  }
  return crypto
    .createHmac('sha256', secret)
    .update(`${userId}:${payload}`)
    .digest('hex');
}

/**
 * Callable: `savePlayerData`
 *
 * Persists a player's JSON snapshot to Firestore after validating the checksum.
 *
 * Request  `{ playerDataJson: string; checksum: string }`
 * Response `{ success: boolean; savedAt: string }`
 */
export const savePlayerData = onCall(async (request) => {
  if (!request.auth) {
    throw new HttpsError('unauthenticated', 'Must be authenticated');
  }

  const {playerDataJson, checksum} = request.data as {
    playerDataJson: string;
    checksum: string;
  };

  if (typeof playerDataJson !== 'string' || playerDataJson.trim() === '') {
    throw new HttpsError('invalid-argument', 'playerDataJson is required');
  }

  // Validate checksum to detect tampering
  const expected = computeChecksum(playerDataJson, request.auth.uid);
  if (checksum !== expected) {
    throw new HttpsError('permission-denied', 'Checksum mismatch — save rejected');
  }

  let parsed: PlayerData;
  try {
    parsed = JSON.parse(playerDataJson) as PlayerData;
  } catch {
    throw new HttpsError('invalid-argument', 'playerDataJson is not valid JSON');
  }

  // Prevent clients from spoofing a different userId
  if (parsed.userId !== request.auth.uid) {
    throw new HttpsError('permission-denied', 'userId mismatch');
  }

  const savedAt = new Date().toISOString();
  const db = admin.firestore();

  await db.collection('players').doc(request.auth.uid).set(
    {...parsed, lastSavedAt: savedAt},
    {merge: true},
  );

  return {success: true, savedAt};
});

/**
 * Callable: `loadPlayerData`
 *
 * Retrieves a player's latest Firestore snapshot.
 *
 * Request  `{}`
 * Response `{ playerData: PlayerData }`
 */
export const loadPlayerData = onCall(async (request) => {
  if (!request.auth) {
    throw new HttpsError('unauthenticated', 'Must be authenticated');
  }

  const db = admin.firestore();
  const snap = await db.collection('players').doc(request.auth.uid).get();

  if (!snap.exists) {
    throw new HttpsError('not-found', 'Player data not found');
  }

  return {playerData: snap.data() as PlayerData};
});
