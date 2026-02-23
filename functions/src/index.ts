import * as admin from 'firebase-admin';
import {onCall, HttpsError} from 'firebase-functions/v2/https';

admin.initializeApp();

export const healthCheck = onCall(async () => {
  return {
    service: 'AI Marketplace Seller Studio Functions',
    mode: process.env.AI_STUB_MODE ?? 'true',
    status: 'ok',
  };
});

/**
 * Exchanges a Facebook OAuth authorization code for a user access token.
 *
 * The Facebook App Secret is used exclusively server-side here so it is
 * never exposed to the client.
 *
 * Expected request.data shape:
 *   { code: string, redirectUri: string }
 *
 * Returns:
 *   { accessToken: string }
 */
export const exchangeFacebookToken = onCall(async (request) => {
  const appId = process.env.FACEBOOK_APP_ID;
  const appSecret = process.env.FACEBOOK_APP_SECRET;

  if (!appId || !appSecret) {
    throw new HttpsError(
      'failed-precondition',
      'Facebook credentials are not configured on the server.'
    );
  }

  const data = request.data as {code?: unknown; redirectUri?: unknown};
  const code = data.code;
  const redirectUri = data.redirectUri;

  if (typeof code !== 'string' || !code) {
    throw new HttpsError('invalid-argument', 'code must be a non-empty string.');
  }
  if (typeof redirectUri !== 'string' || !redirectUri) {
    throw new HttpsError('invalid-argument', 'redirectUri must be a non-empty string.');
  }

  const url = new URL('https://graph.facebook.com/v19.0/oauth/access_token');
  url.searchParams.set('client_id', appId);
  url.searchParams.set('client_secret', appSecret);
  url.searchParams.set('redirect_uri', redirectUri);
  url.searchParams.set('code', code);

  const response = await fetch(url.toString());
  const json = (await response.json()) as {access_token?: string; error?: {message: string}};

  if (!response.ok || json.error) {
    throw new HttpsError('internal', json.error?.message ?? 'Facebook token exchange failed.');
  }

  if (!json.access_token) {
    throw new HttpsError('internal', 'Facebook API did not return an access token.');
  }

  return {accessToken: json.access_token};
});
