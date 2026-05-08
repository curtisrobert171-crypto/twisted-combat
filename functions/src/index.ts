import * as admin from 'firebase-admin';
import {HttpsError, onCall} from 'firebase-functions/v2/https';

admin.initializeApp();

export const healthCheck = onCall(async () => {
  return {
    service: 'AI Marketplace Seller Studio Functions',
    mode: process.env.AI_STUB_MODE ?? 'true',
    status: 'ok',
  };
});

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

  const {code, redirectUri} = request.data as {
    code?: string;
    redirectUri?: string;
  };

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

  const body = (await response.json()) as {
    access_token?: string;
    error?: string;
    error_description?: string;
  };

  if (body.error || !body.access_token) {
    throw new HttpsError(
      'unauthenticated',
      body.error_description ?? body.error ?? 'GitHub token exchange failed.'
    );
  }

  return {accessToken: body.access_token};
});
