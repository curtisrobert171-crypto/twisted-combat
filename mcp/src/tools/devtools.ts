/**
 * MCP tools — Developer helpers (write operations)
 *
 * Exposed tools:
 *   reset_player      — Wipe a player's progress back to Day-1 defaults
 *   grant_currency    — Inject gems / coins for playtesting
 *   set_player_level  — Teleport a player to a specific level for targeted testing
 *   seed_leaderboard  — Populate a leaderboard with synthetic bot data
 *   delete_player     — Permanently remove a player record (QA only)
 */

import {z} from 'zod';
import {McpServer} from '@modelcontextprotocol/sdk/server/mcp.js';

import {getFirestore} from '../firebase.js';

const VALID_CATEGORIES = ['loops_completed', 'bosses_defeated', 'total_score'] as const;

export function registerDevTools(server: McpServer): void {
  // ── reset_player ──────────────────────────────────────────────────────────

  server.registerTool(
    'reset_player',
    {
      description:
        'Reset a player\'s progression to Day-1 defaults (level 1, starter currencies, empty base). ' +
        'Preserves UID and display name. Use this to reproduce FTUE bugs or test onboarding flows.',
      inputSchema: z.object({
        userId: z.string().describe('UID of the player to reset'),
      }),
    },
    async ({userId}) => {
      const db = getFirestore();
      const ref = db.collection('players').doc(userId);
      const snap = await ref.get();

      if (!snap.exists) {
        return {
          content: [{type: 'text', text: `Player "${userId}" not found.`}],
          isError: true,
        };
      }

      const existing = snap.data() as {displayName?: string};
      const resetAt = new Date().toISOString();

      await ref.update({
        level: 1,
        xp: 0,
        currencies: {gems: 100, coins: 500, energy: 100},
        stats: {
          loopsCompleted: 0,
          bossesDefeated: 0,
          mathGatesHit: 0,
          totalPlaytime: 0,
          deaths: 0,
          raidsWon: 0,
        },
        baseLayout: [],
        inventory: [],
        rating: 1000,
        lastResetAt: resetAt,
        displayName: existing.displayName ?? `Player_${userId.slice(0, 6)}`,
      });

      return {
        content: [
          {type: 'text', text: `Player "${userId}" reset to Day-1 defaults at ${resetAt}.`},
        ],
      };
    },
  );

  // ── grant_currency ────────────────────────────────────────────────────────

  server.registerTool(
    'grant_currency',
    {
      description:
        'Inject gems and/or coins into a player\'s wallet for playtesting economy balance. ' +
        'Values are added on top of the current balance (not set absolutely).',
      inputSchema: z.object({
        userId: z.string().describe('UID of the player to top up'),
        gems: z
          .number()
          .int()
          .min(0)
          .optional()
          .default(0)
          .describe('Number of gems to add (default 0)'),
        coins: z
          .number()
          .int()
          .min(0)
          .optional()
          .default(0)
          .describe('Number of coins to add (default 0)'),
        energy: z
          .number()
          .int()
          .min(0)
          .optional()
          .default(0)
          .describe('Energy to add (default 0)'),
      }),
    },
    async ({userId, gems, coins, energy}) => {
      const db = getFirestore();

      const newCurrencies = await db.runTransaction(async (tx) => {
        const ref = db.collection('players').doc(userId);
        const snap = await tx.get(ref);

        if (!snap.exists) throw new Error(`Player "${userId}" not found`);

        const d = snap.data() as {
          currencies?: {gems?: number; coins?: number; energy?: number};
        };
        const updated = {
          gems: (d.currencies?.gems ?? 0) + gems,
          coins: (d.currencies?.coins ?? 0) + coins,
          energy: (d.currencies?.energy ?? 0) + energy,
        };

        tx.update(ref, {
          'currencies.gems': updated.gems,
          'currencies.coins': updated.coins,
          'currencies.energy': updated.energy,
        });
        return updated;
      });

      return {
        content: [
          {
            type: 'text',
            text: `Currency granted to "${userId}". New wallet: ${JSON.stringify(newCurrencies)}.`,
          },
        ],
      };
    },
  );

  // ── set_player_level ──────────────────────────────────────────────────────

  server.registerTool(
    'set_player_level',
    {
      description:
        'Teleport a player to a specific level for targeted content testing. ' +
        'Also sets XP to zero within the new level.',
      inputSchema: z.object({
        userId: z.string().describe('UID of the player'),
        level: z.number().int().min(1).max(1000).describe('Target level (1–1000)'),
      }),
    },
    async ({userId, level}) => {
      const db = getFirestore();
      const ref = db.collection('players').doc(userId);
      const snap = await ref.get();

      if (!snap.exists) {
        return {
          content: [{type: 'text', text: `Player "${userId}" not found.`}],
          isError: true,
        };
      }

      await ref.update({level, xp: 0});
      return {
        content: [{type: 'text', text: `Player "${userId}" set to level ${level}.`}],
      };
    },
  );

  // ── seed_leaderboard ──────────────────────────────────────────────────────

  server.registerTool(
    'seed_leaderboard',
    {
      description:
        'Populate a leaderboard with synthetic bot entries so the leaderboard UI can be tested ' +
        'before real players exist. Each call appends new unique bot entries.',
      inputSchema: z.object({
        category: z.enum(VALID_CATEGORIES).describe('Leaderboard category to seed'),
        count: z
          .number()
          .int()
          .min(1)
          .max(100)
          .optional()
          .default(20)
          .describe('Number of bot entries to create (1–100, default 20)'),
        maxScore: z
          .number()
          .int()
          .min(1)
          .optional()
          .default(10000)
          .describe('Upper bound for randomly generated scores (default 10000)'),
      }),
    },
    async ({category, count, maxScore}) => {
      const db = getFirestore();
      const collRef = db.collection('leaderboards').doc(category).collection('entries');
      const batch = db.batch();
      const now = new Date().toISOString();

      for (let i = 0; i < count; i++) {
        const suffix = `${Date.now()}_${i.toString().padStart(3, '0')}`;
        const botId = `bot_${category}_${suffix}`;
        const score = Math.floor(Math.random() * maxScore) + i * 10;
        batch.set(collRef.doc(botId), {
          userId: botId,
          displayName: `TestBot_${i + 1}`,
          score,
          updatedAt: now,
        });
      }

      await batch.commit();
      return {
        content: [{type: 'text', text: `Seeded ${count} bot entries into "${category}" leaderboard.`}],
      };
    },
  );

  // ── delete_player ─────────────────────────────────────────────────────────

  server.registerTool(
    'delete_player',
    {
      description:
        'Permanently delete a player document from Firestore. ' +
        'Use ONLY on test/QA accounts — this cannot be undone.',
      inputSchema: z.object({
        userId: z.string().describe('UID of the player to delete'),
        confirm: z
          .literal(true)
          .describe('Must be explicitly set to true to prevent accidental deletion'),
      }),
    },
    async ({userId}) => {
      const db = getFirestore();
      const ref = db.collection('players').doc(userId);
      const snap = await ref.get();

      if (!snap.exists) {
        return {
          content: [{type: 'text', text: `Player "${userId}" not found.`}],
          isError: true,
        };
      }

      await ref.delete();
      return {
        content: [{type: 'text', text: `Player "${userId}" permanently deleted.`}],
      };
    },
  );
}
