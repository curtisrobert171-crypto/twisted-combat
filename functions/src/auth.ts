/**
 * Player authentication Cloud Functions.
 *
 * Supports device-based (anonymous) login and display-name registration.
 * A Firestore document under /players/{userId} is created on first login.
 */

import * as admin from 'firebase-admin';
import {onCall, HttpsError} from 'firebase-functions/v2/https';

import {PlayerData} from './types';

const DEFAULT_PLAYER = (): Omit<PlayerData, 'userId' | 'createdAt' | 'lastLogin' | 'displayName'> => ({
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
  settings: {musicVolume: 0.8, sfxVolume: 1.0, analyticsEnabled: true},
});

/**
 * Callable: `deviceLogin`
 *
 * Creates or returns an existing player profile identified by device ID.
 *
 * Request  `{ deviceId: string; platform: string; version: string }`
 * Response `{ userId: string; sessionToken: string; playerData: PlayerData; isNewPlayer: boolean }`
 */
export const deviceLogin = onCall(async (request) => {
  const {deviceId, platform, version} = request.data as {
    deviceId: string;
    platform: string;
    version: string;
  };

  if (!deviceId || typeof deviceId !== 'string' || deviceId.trim() === '') {
    throw new HttpsError('invalid-argument', 'deviceId is required');
  }

  const db = admin.firestore();
  const now = new Date().toISOString();

  // Map device → userId for stable cross-login identity
  const deviceRef = db.collection('deviceMappings').doc(deviceId);
  const deviceSnap = await deviceRef.get();

  let userId: string;
  let isNewPlayer = false;

  if (deviceSnap.exists) {
    userId = (deviceSnap.data() as {userId: string}).userId;
  } else {
    // Create a new Firebase Auth anonymous user to obtain a stable UID
    const userRecord = await admin.auth().createUser({});
    userId = userRecord.uid;
    await deviceRef.set({userId, createdAt: now});
    isNewPlayer = true;
  }

  const playerRef = db.collection('players').doc(userId);
  const playerSnap = await playerRef.get();

  let playerData: PlayerData;

  if (playerSnap.exists) {
    playerData = playerSnap.data() as PlayerData;
    await playerRef.update({lastLogin: now, 'meta.platform': platform, 'meta.version': version});
    playerData.lastLogin = now;
  } else {
    playerData = {
      userId,
      displayName: `Player_${userId.slice(0, 6)}`,
      createdAt: now,
      lastLogin: now,
      ...DEFAULT_PLAYER(),
    };
    await playerRef.set({...playerData, meta: {platform, version}});
  }

  // Mint a short-lived custom token the Unity client can exchange for a full ID token
  const sessionToken = await admin.auth().createCustomToken(userId);

  return {userId, sessionToken, playerData, isNewPlayer};
});

/**
 * Callable: `setDisplayName`
 *
 * Updates a player's display name (must be 3–20 alphanumeric chars or underscores).
 *
 * Request  `{ displayName: string }`
 * Response `{ success: boolean }`
 */
export const setDisplayName = onCall(async (request) => {
  if (!request.auth) {
    throw new HttpsError('unauthenticated', 'Must be authenticated');
  }

  const {displayName} = request.data as {displayName: string};
  const nameRegex = /^[a-zA-Z0-9_]{3,20}$/;

  if (!displayName || !nameRegex.test(displayName)) {
    throw new HttpsError(
      'invalid-argument',
      'displayName must be 3–20 alphanumeric characters or underscores',
    );
  }

  const db = admin.firestore();

  // Ensure the name is not already taken
  const taken = await db
    .collection('players')
    .where('displayName', '==', displayName)
    .limit(1)
    .get();

  if (!taken.empty && taken.docs[0].id !== request.auth.uid) {
    throw new HttpsError('already-exists', 'Display name is already taken');
  }

  await db.collection('players').doc(request.auth.uid).update({displayName});
  return {success: true};
});
