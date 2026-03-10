/**
 * Shared data-transfer types used across all Empire of Glass Cloud Functions.
 */

export interface BuildingCell {
  x: number;
  y: number;
  buildingType: string;
  level: number;
}

export interface InventoryItem {
  itemId: string;
  quantity: number;
}

export interface PlayerSettings {
  musicVolume: number;
  sfxVolume: number;
  analyticsEnabled: boolean;
}

export interface PlayerCurrencies {
  gems: number;
  coins: number;
  energy: number;
}

export interface PlayerStats {
  loopsCompleted: number;
  bossesDefeated: number;
  mathGatesHit: number;
  totalPlaytime: number;
  deaths: number;
}

export interface PlayerData {
  userId: string;
  displayName: string;
  createdAt: string;
  lastLogin: string;
  level: number;
  xp: number;
  currencies: PlayerCurrencies;
  stats: PlayerStats;
  baseLayout: BuildingCell[];
  inventory: InventoryItem[];
  settings: PlayerSettings;
}

export interface LeaderboardEntry {
  userId: string;
  displayName: string;
  score: number;
  rank: number;
  updatedAt: string;
}

export interface RaidTarget {
  targetUserId: string;
  displayName: string;
  baseLevel: number;
  baseLayout: BuildingCell[];
  potentialLoot: {
    coins: number;
    gems: number;
  };
  defensePower: number;
}

export type LeaderboardCategory =
  | 'loops_completed'
  | 'bosses_defeated'
  | 'total_score';
