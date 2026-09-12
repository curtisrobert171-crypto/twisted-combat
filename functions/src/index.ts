import * as admin from 'firebase-admin';
import {HttpsError, onCall} from 'firebase-functions/v2/https';
import {defineSecret} from 'firebase-functions/params';

admin.initializeApp();

const facebookAppId = defineSecret('FACEBOOK_APP_ID');
const facebookAppSecret = defineSecret('FACEBOOK_APP_SECRET');

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
/**
 * Exchanges a GitHub OAuth authorization code for an access token.
 *
 * The GitHub Client Secret is read exclusively from server-side environment
 * variables (GITHUB_CLIENT_SECRET) and is never returned to or readable by
 * the client.
 *
 * Expected request data: { code: string, redirectUri: string }
 * Response: { accessToken: string }
 */
export const exchangeGitHubToken = onCall(async (request) => {
  const clientId = process.env.GITHUB_CLIENT_ID;
  const clientSecret = process.env.GITHUB_CLIENT_SECRET;

  if (!clientId || !clientSecret) {
    throw new HttpsError(
      'failed-precondition',
      'GitHub OAuth is not configured on this server.'
    );
  }

  const data = request.data as unknown;
  if (!data || typeof data !== 'object') {
    throw new HttpsError('invalid-argument', 'Request data must be an object.');
  }
  const {code, redirectUri} = data as Record<string, unknown>;

  if (!code || typeof code !== 'string') {
    throw new HttpsError('invalid-argument', 'Missing or invalid OAuth code.');
  }
  if (!redirectUri || typeof redirectUri !== 'string') {
    throw new HttpsError('invalid-argument', 'Missing or invalid redirectUri.');
  }

  const params = new URLSearchParams({
    client_id: clientId,
    client_secret: clientSecret,
    code,
    redirect_uri: redirectUri,
  });

  let response: Response;
  try {
    response = await fetch(
      `https://github.com/login/oauth/access_token?${params.toString()}`,
      {
        method: 'POST',
        headers: {Accept: 'application/json'},
      }
    );
  } catch (err) {
    throw new HttpsError('unavailable', 'Failed to reach GitHub OAuth endpoint.');
  }

  if (!response.ok) {
    throw new HttpsError(
      'internal',
      `GitHub OAuth endpoint returned HTTP ${response.status}.`
    );
  }

  let body: unknown;
  try {
    body = await response.json();
  } catch {
    throw new HttpsError('internal', 'Failed to parse GitHub OAuth response.');
  }

  if (!body || typeof body !== 'object') {
    throw new HttpsError('internal', 'Unexpected GitHub OAuth response format.');
  }

  const {access_token, error, error_description} = body as Record<string, unknown>;

  if (error || !access_token) {
    throw new HttpsError(
      'unauthenticated',
      typeof error_description === 'string'
        ? error_description
        : typeof error === 'string'
          ? error
          : 'GitHub token exchange failed.'
    );
  }

  if (typeof access_token !== 'string') {
    throw new HttpsError('internal', 'GitHub OAuth returned an invalid access token.');
  }
  return {accessToken: access_token};
});

/**
 * Exchanges a Facebook OAuth authorization code for an access token.
 *
 * The Facebook App ID and App Secret are read from Firebase Functions secrets
 * and are never returned to the client.
 *
 * Expected request data: { code: string, redirectUri: string }
 * Response: { accessToken: string }
 */
export const exchangeFacebookToken = onCall(
  {secrets: [facebookAppId, facebookAppSecret]},
  async (request) => {
    const appId = facebookAppId.value();
    const appSecret = facebookAppSecret.value();

    if (!appId || !appSecret) {
      throw new HttpsError(
        'failed-precondition',
        'Facebook OAuth is not configured on this server.'
      );
    }

    const data = request.data as unknown;
    if (!data || typeof data !== 'object') {
      throw new HttpsError('invalid-argument', 'Request data must be an object.');
    }

    const {code, redirectUri} = data as Record<string, unknown>;
    if (!code || typeof code !== 'string') {
      throw new HttpsError('invalid-argument', 'Missing or invalid OAuth code.');
    }
    if (!redirectUri || typeof redirectUri !== 'string') {
      throw new HttpsError('invalid-argument', 'Missing or invalid redirectUri.');
    }

    const params = new URLSearchParams({
      client_id: appId,
      client_secret: appSecret,
      code,
      redirect_uri: redirectUri,
    });

    let response: Response;
    try {
      response = await fetch(
        `https://graph.facebook.com/v19.0/oauth/access_token?${params.toString()}`,
        {
          method: 'GET',
          headers: {Accept: 'application/json'},
        }
      );
    } catch {
      throw new HttpsError('unavailable', 'Failed to reach Facebook OAuth endpoint.');
    }

    let body: unknown;
    try {
      body = await response.json();
    } catch {
      throw new HttpsError('internal', 'Failed to parse Facebook OAuth response.');
    }

    if (!body || typeof body !== 'object') {
      throw new HttpsError('internal', 'Unexpected Facebook OAuth response format.');
    }

    const {access_token, error} = body as Record<string, unknown>;
    const errorObject = error && typeof error === 'object' ? (error as Record<string, unknown>) : null;
    const errorMessage =
      errorObject && typeof errorObject.message === 'string'
        ? errorObject.message
        : 'Facebook token exchange failed.';

    if (!response.ok || errorObject || typeof access_token !== 'string') {
      throw new HttpsError(
        response.status >= 400 && response.status < 500 ? 'unauthenticated' : 'internal',
        errorMessage
      );
    }

    return {accessToken: access_token};
  }
);
