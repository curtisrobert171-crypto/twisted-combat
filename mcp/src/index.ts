#!/usr/bin/env node
/**
 * Empire of Glass — MCP Server
 *
 * Starts a Model Context Protocol server over stdio that exposes the
 * game-development tools to any MCP-capable AI assistant (Claude Desktop,
 * Continue, Cursor, etc.).
 *
 * Usage:
 *   node lib/index.js
 *
 * Required env vars:
 *   FIREBASE_PROJECT_ID          — Your Firebase project ID
 *   GOOGLE_APPLICATION_CREDENTIALS — Path to service-account JSON (or use ADC)
 */

import {McpServer} from '@modelcontextprotocol/sdk/server/mcp.js';
import {StdioServerTransport} from '@modelcontextprotocol/sdk/server/stdio.js';

import {registerPlayerTools} from './tools/player.js';
import {registerLeaderboardTools} from './tools/leaderboard.js';
import {registerRaidTools} from './tools/raid.js';
import {registerDevTools} from './tools/devtools.js';
import {registerMetricsTools} from './tools/metrics.js';

async function main(): Promise<void> {
  const server = new McpServer({
    name: 'empire-of-glass',
    version: '1.0.0',
  });

  // Register all tool groups
  registerPlayerTools(server);
  registerLeaderboardTools(server);
  registerRaidTools(server);
  registerDevTools(server);
  registerMetricsTools(server);

  const transport = new StdioServerTransport();
  await server.connect(transport);

  // Log to stderr so it doesn't interfere with the stdio MCP protocol
  process.stderr.write('[empire-of-glass MCP] Server started\n');
}

main().catch((err) => {
  process.stderr.write(`[empire-of-glass MCP] Fatal error: ${err}\n`);
  process.exit(1);
});
