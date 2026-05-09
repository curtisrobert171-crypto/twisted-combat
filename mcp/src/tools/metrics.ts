/**
 * MCP tools — Live game metrics
 *
 * Exposed tools:
 *   get_metrics          — Aggregate live stats snapshot (DAU, raids, leaderboard counts)
 *   get_session_activity — Breakdown of game-state sessions (Swarm / City / Raid)
 *   get_economy_summary  — Average and total currency balances across all players
 */

import {z} from 'zod';
import {McpServer} from '@modelcontextprotocol/sdk/server/mcp.js';

import {getFirestore} from '../firebase.js';

const VALID_CATEGORIES = ['loops_completed', 'bosses_defeated', 'total_score'] as const;

export function registerMetricsTools(server: McpServer): void {
  // ── get_metrics ───────────────────────────────────────────────────────────

  server.registerTool(
    'get_metrics',
    {
      description:
        'Return a live snapshot of key Empire of Glass game metrics: ' +
        'total players, raids completed today, and entry counts per leaderboard category.',
      inputSchema: z.object({}),
    },
    async () => {
      const db = getFirestore();
      const todayStart = new Date();
      todayStart.setHours(0, 0, 0, 0);
      const todayIso = todayStart.toISOString();

      const [totalPlayersSnap, raidsTodaySnap, ...lbSnaps] = await Promise.all([
        db.collection('players').count().get(),
        db.collection('raidHistory').where('completedAt', '>=', todayIso).count().get(),
        ...VALID_CATEGORIES.map((cat) =>
          db
            .collection('leaderboards')
            .doc(cat)
            .collection('entries')
            .count()
            .get(),
        ),
      ]);

      const leaderboardCounts: Record<string, number> = {};
      VALID_CATEGORIES.forEach((cat, idx) => {
        leaderboardCounts[cat] = lbSnaps[idx].data().count;
      });

      const metrics = {
        totalPlayers: totalPlayersSnap.data().count,
        raidsToday: raidsTodaySnap.data().count,
        leaderboardCounts,
        generatedAt: new Date().toISOString(),
      };

      return {content: [{type: 'text', text: JSON.stringify(metrics, null, 2)}]};
    },
  );

  // ── get_economy_summary ───────────────────────────────────────────────────

  server.registerTool(
    'get_economy_summary',
    {
      description:
        'Fetch currency balances for a sample of players and compute average gems, coins, and energy. ' +
        'Useful for monitoring economy health and inflation during playtests.',
      inputSchema: z.object({
        sampleSize: z
          .number()
          .int()
          .min(1)
          .max(500)
          .optional()
          .default(100)
          .describe('Number of players to sample (1–500, default 100)'),
      }),
    },
    async ({sampleSize}) => {
      const db = getFirestore();
      const snap = await db.collection('players').limit(sampleSize).get();

      if (snap.empty) {
        return {content: [{type: 'text', text: 'No players found.'}]};
      }

      let totalGems = 0;
      let totalCoins = 0;
      let totalEnergy = 0;
      let count = 0;

      for (const doc of snap.docs) {
        const d = doc.data() as {
          currencies?: {gems?: number; coins?: number; energy?: number};
        };
        totalGems += d.currencies?.gems ?? 0;
        totalCoins += d.currencies?.coins ?? 0;
        totalEnergy += d.currencies?.energy ?? 0;
        count++;
      }

      const summary = {
        sampleSize: count,
        totalGems,
        totalCoins,
        totalEnergy,
        averageGems: Math.round(totalGems / count),
        averageCoins: Math.round(totalCoins / count),
        averageEnergy: Math.round(totalEnergy / count),
        generatedAt: new Date().toISOString(),
      };

      return {content: [{type: 'text', text: JSON.stringify(summary, null, 2)}]};
    },
  );

  // ── get_top_players ───────────────────────────────────────────────────────

  server.registerTool(
    'get_top_players',
    {
      description:
        'Return the top N players sorted by a chosen stat field ' +
        '(level | xp | stats.loopsCompleted | stats.bossesDefeated | rating). ' +
        'Useful for identifying power-users and outliers during playtesting.',
      inputSchema: z.object({
        sortBy: z
          .enum(['level', 'xp', 'stats.loopsCompleted', 'stats.bossesDefeated', 'rating'])
          .optional()
          .default('level')
          .describe('Field to sort by (default: level)'),
        limit: z
          .number()
          .int()
          .min(1)
          .max(50)
          .optional()
          .default(10)
          .describe('Number of players to return (1–50, default 10)'),
      }),
    },
    async ({sortBy, limit}) => {
      const db = getFirestore();
      const snap = await db
        .collection('players')
        .orderBy(sortBy, 'desc')
        .limit(limit)
        .get();

      if (snap.empty) {
        return {content: [{type: 'text', text: 'No players found.'}]};
      }

      const rows = snap.docs.map((doc, idx) => {
        const d = doc.data() as Record<string, unknown> & {
          displayName?: string;
          level?: number;
          xp?: number;
          rating?: number;
          stats?: Record<string, number>;
        };
        return {
          rank: idx + 1,
          userId: doc.id,
          displayName: d.displayName ?? '(unknown)',
          level: d.level ?? 0,
          xp: d.xp ?? 0,
          rating: d.rating ?? 1000,
          [sortBy]: sortBy.startsWith('stats.')
            ? (d.stats?.[sortBy.split('.')[1]] ?? 0)
            : (d[sortBy] ?? 0),
        };
      });

      return {content: [{type: 'text', text: JSON.stringify(rows, null, 2)}]};
    },
  );
}
