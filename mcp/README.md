# Empire of Glass — MCP Server

A [Model Context Protocol](https://modelcontextprotocol.io/) (MCP) server that exposes live **Empire of Glass** game-development tools to any MCP-capable AI assistant (Claude Desktop, Cursor, Continue, etc.).

Once connected, your AI assistant can:

- 🔍 **Inspect player data** — look up profiles, search by name, list all players
- 🏆 **Query leaderboards** — fetch rankings and individual player ranks
- ⚔️  **Test matchmaking** — simulate finding a raid target for any player
- 📊 **Analyse metrics** — live player counts, raid activity, economy health
- 🛠  **Dev helpers** — reset players, inject currency, seed leaderboards, set levels

---

## Prerequisites

| Requirement | Version |
|---|---|
| Node.js | ≥ 20 |
| Firebase project | Any |
| Google credentials | Service-account JSON **or** ADC |

---

## Setup

### 1 — Build the server

```bash
cd mcp
npm install
npm run build   # produces lib/index.js via esbuild
```

### 2 — Configure credentials

The server needs read/write access to your Firestore database.

**Option A — Service-account JSON (recommended for local dev)**

1. In the [Firebase Console](https://console.firebase.google.com), go to  
   **Project settings → Service accounts → Generate new private key**.
2. Save the JSON file somewhere safe (e.g. `~/.config/empire-of-glass-sa.json`).
3. Set the environment variable in your MCP client config (see below):
   ```
   GOOGLE_APPLICATION_CREDENTIALS=/path/to/empire-of-glass-sa.json
   ```

**Option B — Application Default Credentials**

If you are already authenticated with `gcloud`:

```bash
gcloud auth application-default login
```

---

## Connecting to Claude Desktop

Add the following block to your `claude_desktop_config.json`  
(usually at `~/Library/Application Support/Claude/claude_desktop_config.json` on macOS):

```json
{
  "mcpServers": {
    "empire-of-glass": {
      "command": "node",
      "args": ["/absolute/path/to/mcp/lib/index.js"],
      "env": {
        "FIREBASE_PROJECT_ID": "your-firebase-project-id",
        "GOOGLE_APPLICATION_CREDENTIALS": "/path/to/empire-of-glass-sa.json"
      }
    }
  }
}
```

Restart Claude Desktop. You should see **empire-of-glass** in the MCP tools list.

---

## Connecting to Other MCP Clients

Any stdio-based MCP client works. Use this command:

```bash
FIREBASE_PROJECT_ID=your-project-id \
GOOGLE_APPLICATION_CREDENTIALS=/path/to/sa.json \
node /path/to/mcp/lib/index.js
```

---

## Available Tools

### Player tools

| Tool | Description |
|---|---|
| `get_player` | Fetch a player's full profile by UID |
| `list_players` | List players ordered by level (desc) |
| `search_players` | Prefix-search players by display name |

### Leaderboard tools

| Tool | Description |
|---|---|
| `get_leaderboard` | Top-N entries for `loops_completed`, `bosses_defeated`, or `total_score` |
| `get_player_rank` | A specific player's rank and score in one category |

### Raid tools

| Tool | Description |
|---|---|
| `find_raid_target` | Simulate matchmaking — find eligible opponents for a player |
| `get_raid_history` | Recent raid records, optionally filtered by player UID |

### Dev helper tools ⚠️ destructive

| Tool | Description |
|---|---|
| `reset_player` | Wipe progress to Day-1 defaults (keeps UID + display name) |
| `grant_currency` | Add gems / coins / energy to a player's wallet |
| `set_player_level` | Teleport a player to a specific level |
| `seed_leaderboard` | Populate a leaderboard with synthetic bot entries |
| `delete_player` | Permanently delete a player record (requires `confirm: true`) |

### Metrics tools

| Tool | Description |
|---|---|
| `get_metrics` | Live snapshot: total players, raids today, leaderboard entry counts |
| `get_economy_summary` | Average / total currency balances across a player sample |
| `get_top_players` | Top-N players by any stat field |

---

## Example prompts

Once connected to Claude Desktop, try:

```
Show me the top 10 players by level.
```

```
What does the loops_completed leaderboard look like right now?
```

```
Reset player uid_abc123 to Day-1 for a fresh FTUE test.
```

```
Grant 5000 gems and 10000 coins to uid_abc123 for economy testing.
```

```
Who can uid_xyz789 raid? Simulate the matchmaking for them.
```

```
Give me a live metrics snapshot of the game.
```

---

## Security notes

- The MCP server runs **locally on your machine** and communicates directly with Firestore.  
  It is **never** exposed to the internet.
- Dev tools (`reset_player`, `delete_player`, etc.) should only be used against QA/test accounts.
- Never commit your service-account JSON to the repository.

---

## Development

```bash
# Type-check without building
npm run typecheck

# Rebuild after source changes
npm run build
```
