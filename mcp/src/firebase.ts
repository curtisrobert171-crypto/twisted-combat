/**
 * Shared Firestore instance for all MCP tool modules.
 *
 * Uses the `@google-cloud/firestore` SDK directly (lighter than firebase-admin).
 * Authentication is handled by Application Default Credentials (ADC) or a
 * service-account key pointed to by GOOGLE_APPLICATION_CREDENTIALS.
 */

import {Firestore} from '@google-cloud/firestore';

let instance: Firestore | null = null;

export function getFirestore(): Firestore {
  if (!instance) {
    const projectId = process.env.FIREBASE_PROJECT_ID;
    if (!projectId) {
      throw new Error(
        'FIREBASE_PROJECT_ID environment variable is required. ' +
          'Set it in your MCP client config (e.g. claude_desktop_config.json).',
      );
    }
    instance = new Firestore({projectId});
  }
  return instance;
}
