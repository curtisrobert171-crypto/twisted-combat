import * as admin from 'firebase-admin';
import {onCall} from 'firebase-functions/v2/https';

admin.initializeApp();

// ── Health ────────────────────────────────────────────────────────────────────

export const healthCheck = onCall(async () => {
  return {
    service: 'Empire of Glass — Game Backend Functions',
    mode: process.env.AI_STUB_MODE ?? 'true',
    status: 'ok',
  };
});

// ── Authentication ────────────────────────────────────────────────────────────

export {deviceLogin, setDisplayName} from './auth';

// ── Player data (cloud save / load) ──────────────────────────────────────────

export {savePlayerData, loadPlayerData} from './player';

// ── Leaderboards ──────────────────────────────────────────────────────────────

export {submitScore, getLeaderboard} from './leaderboard';

// ── PvP Matchmaking ───────────────────────────────────────────────────────────

export {findRaidTarget} from './matchmaking';

// ── Raid results ──────────────────────────────────────────────────────────────

export {completeRaid} from './raid';

// ── Developer tools (admin-only) ─────────────────────────────────────────────

export {
  devResetPlayer,
  devGrantCurrency,
  devSeedLeaderboard,
  devGetMetrics,
} from './devtools';
