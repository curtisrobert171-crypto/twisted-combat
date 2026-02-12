import * as admin from 'firebase-admin';
import {onCall} from 'firebase-functions/v2/https';

admin.initializeApp();

export const healthCheck = onCall(async () => {
  return {
    service: 'AI Marketplace Seller Studio Functions',
    mode: process.env.AI_STUB_MODE ?? 'true',
    status: 'ok',
  };
});
