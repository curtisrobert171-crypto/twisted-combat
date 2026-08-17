/**
 * MCP tools — Leaderboards
 *
 * Exposed tools:
 *   get_leaderboard  — Fetch top-N entries for a category
 *   get_player_rank  — Fetch a specific player's rank in a category
 */

import {z} from 'zod';
import {McpServer} from '@modelcontextprotocol/sdk/server/mcp.js';

import {getFirestore} from '../firebase.js';

const VALID_CATEGORIES = ['loops_completed', 'bosses_defeated', 'total_score'] as const;
type LeaderboardCategory = (typeof VALID_CATEGORIES)[number];

export function registerLeaderboardTools(server: McpServer): void {
  // ── get_leaderboard ───────────────────────────────────────────────────────

  server.registerTool(
    'get_leaderboard',
    {
      description:
        'Fetch the top entries from an Empire of Glass leaderboard. ' +
        'Valid categories: loops_completed | bosses_defeated | total_score.',
      inputSchema: z.object({
        category: z
          .enum(VALID_CATEGORIES)
          .describe('Leaderboard category to query'),
        limit: z
          .number()
          .int()
          .min(1)
          .max(200)
          .optional()
          .default(20)
          .describe('Number of entries to return (1–200, default 20)'),
      }),
    },
    async ({category, limit}) => {
      const db = getFirestore();
      const snap = await db
        .collection('leaderboards')
        .doc(category)
        .collection('entries')
        .orderBy('score', 'desc')
        .limit(limit)
        .get();

      if (snap.empty) {
        return {
          content: [{type: 'text', text: `Leaderboard "${category}" has no entries yet.`}],
        };
      }

      const entries = snap.docs.map((doc, idx) => ({
        rank: idx + 1,
        ...(doc.data() as Record<string, unknown>),
      }));

      return {content: [{type: 'text', text: JSON.stringify(entries, null, 2)}]};
    },
  );

  // ── get_player_rank ───────────────────────────────────────────────────────

  server.registerTool(
    'get_player_rank',
    {
      description:
        "Look up a specific player's rank and score in one leaderboard category. " +
        'Returns rank, score, and total entries in the leaderboard.',
      inputSchema: z.object({
        category: z.enum(VALID_CATEGORIES).describe('Leaderboard category'),
        userId: z.string().describe('Player UID to look up'),
      }),
    },
    async ({category, userId}) => {
      const db = getFirestore();
      const collRef = db
        .collection('leaderboards')
        .doc(category as LeaderboardCategory)
        .collection('entries');

      const [playerSnap, totalSnap] = await Promise.all([
        collRef.doc(userId).get(),
        collRef.count().get(),
      ]);

      if (!playerSnap.exists) {
        return {
          content: [
            {
              type: 'text',
              text: `Player "${userId}" has no entry in the "${category}" leaderboard.`,
            },
          ],
        };
      }

      const data = playerSnap.data() as {score: number; displayName: string};
      const higherSnap = await collRef.where('score', '>', data.score).count().get();
      const rank = higherSnap.data().count + 1;

      return {
        content: [
          {
            type: 'text',
            text: JSON.stringify(
              {
                userId,
                displayName: data.displayName,
                score: data.score,
                rank,
                totalEntries: totalSnap.data().count,
              },
              null,
              2,
            ),
          },
        ],
      };
    },
  );
}
