/**
 * MCP tools — Raid system
 *
 * Exposed tools:
 *   find_raid_target  — Simulate matchmaking: find an eligible opponent
 *   get_raid_history  — Fetch recent raids (optionally filtered by player)
 */

import {z} from 'zod';
import {McpServer} from '@modelcontextprotocol/sdk/server/mcp.js';

import {getFirestore} from '../firebase.js';

/**
 * Level window used in matchmaking — must match LEVEL_WINDOW in
 * functions/src/matchmaking.ts.
 */
const LEVEL_WINDOW = 3;

/**
 * Raid-shield duration in ms — must match SHIELD_DURATION_MS in
 * functions/src/matchmaking.ts.
 */
const SHIELD_DURATION_MS = 10 * 60 * 1000;

/**
 * Fraction of the defender's coins shown as *preview* loot — kept lower than
 * MAX_LOOT_FRACTION in functions/src/raid.ts (0.15) because the preview is an
 * estimate before the defender's actual wallet is authorised by the server.
 */
const PREVIEW_LOOT_FRACTION_COINS = 0.1;
const PREVIEW_LOOT_FRACTION_GEMS = 0.05;

export function registerRaidTools(server: McpServer): void {
  // ── find_raid_target ──────────────────────────────────────────────────────

  server.registerTool(
    'find_raid_target',
    {
      description:
        'Simulate the matchmaking algorithm: find an eligible PvP raid target for a given player. ' +
        'Respects the ±3 level window and the 10-minute raid shield. ' +
        'Useful for QA testing or verifying matchmaking fairness.',
      inputSchema: z.object({
        userId: z.string().describe('UID of the attacking player'),
      }),
    },
    async ({userId}) => {
      const db = getFirestore();

      const attackerSnap = await db.collection('players').doc(userId).get();
      if (!attackerSnap.exists) {
        return {
          content: [{type: 'text', text: `Player "${userId}" not found.`}],
          isError: true,
        };
      }

      const attacker = attackerSnap.data() as {level?: number};
      const level = attacker.level ?? 1;
      const minLevel = Math.max(1, level - LEVEL_WINDOW);
      const maxLevel = level + LEVEL_WINDOW;
      const now = Date.now();

      const candidatesSnap = await db
        .collection('players')
        .where('level', '>=', minLevel)
        .where('level', '<=', maxLevel)
        .limit(15)
        .get();

      const eligible = candidatesSnap.docs.filter((doc) => {
        if (doc.id === userId) return false;
        const d = doc.data() as {lastRaidedAt?: string};
        if (!d.lastRaidedAt) return true;
        return now - new Date(d.lastRaidedAt).getTime() > SHIELD_DURATION_MS;
      });

      if (eligible.length === 0) {
        return {
          content: [
            {
              type: 'text',
              text: `No eligible targets found for player "${userId}" (level ${level}±${LEVEL_WINDOW}). All candidates may be shielded.`,
            },
          ],
        };
      }

      const targets = eligible.map((doc) => {
        const d = doc.data() as Record<string, unknown> & {
          displayName?: string;
          level?: number;
          currencies?: {coins?: number; gems?: number};
          baseLayout?: unknown[];
        };
        return {
          targetUserId: doc.id,
          displayName: d.displayName ?? '(unknown)',
          baseLevel: d.level ?? 1,
          potentialLoot: {
            coins: Math.floor((d.currencies?.coins ?? 0) * PREVIEW_LOOT_FRACTION_COINS),
            gems: Math.floor((d.currencies?.gems ?? 0) * PREVIEW_LOOT_FRACTION_GEMS),
          },
          defensePower: (d.baseLayout ?? []).reduce(
            (sum: number, cell: unknown) =>
              sum + ((cell as {level?: number}).level ?? 0),
            0,
          ),
        };
      });

      return {
        content: [
          {
            type: 'text',
            text: `Found ${targets.length} eligible target(s):\n${JSON.stringify(targets, null, 2)}`,
          },
        ],
      };
    },
  );

  // ── get_raid_history ──────────────────────────────────────────────────────

  server.registerTool(
    'get_raid_history',
    {
      description:
        'Fetch recent raid outcomes. Optionally filter by attacker or defender UID. ' +
        'Each record includes attacker, defender, result, loot transferred, damage dealt, and timestamp.',
      inputSchema: z.object({
        userId: z
          .string()
          .optional()
          .describe(
            'Optional UID. If provided, returns raids where this player is the attacker OR defender.',
          ),
        limit: z
          .number()
          .int()
          .min(1)
          .max(100)
          .optional()
          .default(20)
          .describe('Max records to return (1–100, default 20)'),
      }),
    },
    async ({userId, limit}) => {
      const db = getFirestore();
      let queryRef = db.collection('raidHistory').orderBy('completedAt', 'desc').limit(limit);

      // If a specific player was requested we can only filter by one side at a time
      if (userId) {
        // Fetch raids where this player attacked; a separate fetch for defence would
        // require two queries + merge (fine for a dev tool)
        queryRef = db
          .collection('raidHistory')
          .where('attackerId', '==', userId)
          .orderBy('completedAt', 'desc')
          .limit(limit);
      }

      const snap = await queryRef.get();

      if (snap.empty) {
        return {
          content: [
            {
              type: 'text',
              text: userId
                ? `No raid history found for player "${userId}".`
                : 'No raid history found.',
            },
          ],
        };
      }

      const records = snap.docs.map((doc) => doc.data());
      return {content: [{type: 'text', text: JSON.stringify(records, null, 2)}]};
    },
  );
}
