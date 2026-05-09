/**
 * MCP tools — Player data
 *
 * Exposed tools:
 *   get_player      — Fetch a single player's full profile by UID
 *   list_players    — List players ordered by level (most recent first)
 *   search_players  — Search players by display name prefix
 */

import {z} from 'zod';
import {McpServer} from '@modelcontextprotocol/sdk/server/mcp.js';

import {getFirestore} from '../firebase.js';

export function registerPlayerTools(server: McpServer): void {
  // ── get_player ────────────────────────────────────────────────────────────

  server.registerTool(
    'get_player',
    {
      description:
        'Fetch the full profile for a single Empire of Glass player by their UID. ' +
        'Returns level, XP, currencies (gems/coins/energy), stats, base layout, inventory, and settings.',
      inputSchema: z.object({
        userId: z.string().describe('Firebase UID of the player to look up'),
      }),
    },
    async ({userId}) => {
      const db = getFirestore();
      const snap = await db.collection('players').doc(userId).get();

      if (!snap.exists) {
        return {
          content: [{type: 'text', text: `No player found with UID "${userId}".`}],
          isError: true,
        };
      }

      return {
        content: [
          {
            type: 'text',
            text: JSON.stringify(snap.data(), null, 2),
          },
        ],
      };
    },
  );

  // ── list_players ──────────────────────────────────────────────────────────

  server.registerTool(
    'list_players',
    {
      description:
        'List Empire of Glass players ordered by level (descending). ' +
        'Useful for a quick overview of who is in the database during playtests.',
      inputSchema: z.object({
        limit: z
          .number()
          .int()
          .min(1)
          .max(100)
          .optional()
          .default(20)
          .describe('Maximum number of players to return (1–100, default 20)'),
      }),
    },
    async ({limit}) => {
      const db = getFirestore();
      const snap = await db
        .collection('players')
        .orderBy('level', 'desc')
        .limit(limit)
        .get();

      if (snap.empty) {
        return {content: [{type: 'text', text: 'No players in the database yet.'}]};
      }

      const rows = snap.docs.map((doc) => {
        const d = doc.data() as Record<string, unknown> & {
          displayName?: string;
          level?: number;
          xp?: number;
          currencies?: {gems?: number; coins?: number; energy?: number};
          stats?: {loopsCompleted?: number; bossesDefeated?: number};
        };
        return {
          userId: doc.id,
          displayName: d.displayName ?? '(unknown)',
          level: d.level ?? 0,
          xp: d.xp ?? 0,
          gems: d.currencies?.gems ?? 0,
          coins: d.currencies?.coins ?? 0,
          loopsCompleted: d.stats?.loopsCompleted ?? 0,
          bossesDefeated: d.stats?.bossesDefeated ?? 0,
        };
      });

      return {
        content: [{type: 'text', text: JSON.stringify(rows, null, 2)}],
      };
    },
  );

  // ── search_players ────────────────────────────────────────────────────────

  server.registerTool(
    'search_players',
    {
      description:
        'Search players by display-name prefix. ' +
        'Case-sensitive prefix match against the `displayName` field in Firestore.',
      inputSchema: z.object({
        prefix: z.string().min(1).describe('Display-name prefix to search for (case-sensitive)'),
        limit: z
          .number()
          .int()
          .min(1)
          .max(50)
          .optional()
          .default(10)
          .describe('Max results (1–50, default 10)'),
      }),
    },
    async ({prefix, limit}) => {
      const db = getFirestore();
      // Firestore range query simulates a "starts with" search
      const snap = await db
        .collection('players')
        .where('displayName', '>=', prefix)
        .where('displayName', '<', prefix + '\uf8ff')
        .limit(limit)
        .get();

      if (snap.empty) {
        return {
          content: [{type: 'text', text: `No players found matching prefix "${prefix}".`}],
        };
      }

      const results = snap.docs.map((doc) => {
        const d = doc.data() as Record<string, unknown> & {
          displayName?: string;
          level?: number;
        };
        return {userId: doc.id, displayName: d.displayName, level: d.level ?? 0};
      });

      return {content: [{type: 'text', text: JSON.stringify(results, null, 2)}]};
    },
  );
}
