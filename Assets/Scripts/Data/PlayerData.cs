using System;
using System.Collections.Generic;
using UnityEngine;

namespace EmpireOfGlass.Data
{
    /// <summary>
    /// Player data model for JSON save/load.
    /// Schema matches TGDD Section 3: UserID, Currencies, BaseLayout[][], Inventory[].
    /// Supports cloud save via OAuth 2.0 login (Var 40).
    /// </summary>
    [Serializable]
    public class PlayerData
    {
        public string UserID;
        public string DisplayName;
        public int Level;
        public long LastLoginTimestamp;

        // Currencies
        public int Gold;
        public int PremiumGems;
        public int RaidEnergy;
        public int ShieldCount;

        // Progression
        public int SwarmHighScore;
        public int RaidsCompleted;
        public int CitySizeLevel;

        // Piggy Bank (Var 25)
        public int PiggyBankGems;
        public bool PiggyBankBroken;

        // Battle Pass (Var 26)
        public int BattlePassTier;
        public bool BattlePassPremium;

        // VIP System (Var 31)
        public int VIPLevel;
        public long VIPExpiryTimestamp;

        // Gacha (Var 27)
        public int GachaPity; // pity counter for guaranteed rare pull

        // Social Proof (Var 29)
        public int TotalRarePulls;

        // Endowment Effect (Var 32)
        public string TrialHeroID;
        public long TrialHeroExpiryTimestamp;

        // Reciprocity (Var 33)
        public long LastDailyGiftTimestamp;
        public int DailyGiftStreak;

        // Shield Mechanics (Var 34)
        public long ShieldExpiryTimestamp;
        public long LastShieldRechargeTimestamp;

        // Base Layout (grid-based defense layout for raids — Var 22)
        public int[][] BaseLayout;

        // Inventory (hero shards, items, skins)
        public List<InventoryItem> Inventory;

        // Offline Progression (Var 20)
        public long LastOfflineTimestamp;
        public float OfflineAccumulatedGold;

        /// <summary>
        /// Creates a new player profile with default values.
        /// </summary>
        public static PlayerData CreateNew(string userId)
        {
            return new PlayerData
            {
                UserID = userId,
                DisplayName = "Shardling",
                Level = 1,
                LastLoginTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Gold = 100,
                PremiumGems = 0,
                RaidEnergy = 50,
                ShieldCount = 3,
                SwarmHighScore = 0,
                RaidsCompleted = 0,
                CitySizeLevel = 0,
                PiggyBankGems = 0,
                PiggyBankBroken = false,
                BattlePassTier = 0,
                BattlePassPremium = false,
                VIPLevel = 0,
                VIPExpiryTimestamp = 0,
                GachaPity = 0,
                TotalRarePulls = 0,
                TrialHeroID = "",
                TrialHeroExpiryTimestamp = 0,
                LastDailyGiftTimestamp = 0,
                DailyGiftStreak = 0,
                ShieldExpiryTimestamp = 0,
                LastShieldRechargeTimestamp = 0,
                BaseLayout = CreateEmptyBaseLayout(10, 10),
                Inventory = new List<InventoryItem>(),
                LastOfflineTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                OfflineAccumulatedGold = 0f
            };
        }

        /// <summary>
        /// Serialize to JSON for cloud save.
        /// </summary>
        public string ToJson()
        {
            return JsonUtility.ToJson(this, true);
        }

        /// <summary>
        /// Deserialize from JSON cloud save.
        /// </summary>
        public static PlayerData FromJson(string json)
        {
            return JsonUtility.FromJson<PlayerData>(json);
        }

        /// <summary>
        /// Creates an empty base layout grid with proper initialization.
        /// </summary>
        private static int[][] CreateEmptyBaseLayout(int width, int height)
        {
            var layout = new int[width][];
            for (int i = 0; i < width; i++)
            {
                layout[i] = new int[height];
            }
            return layout;
        }

        /// <summary>
        /// Calculate offline idle rewards (capped at 10 hours — Var 20).
        /// </summary>
        public float CalculateOfflineRewards(float goldPerSecond, float maxOfflineHours = 10f)
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            float elapsedSeconds = now - LastOfflineTimestamp;
            float maxSeconds = maxOfflineHours * 3600f;
            elapsedSeconds = Mathf.Min(elapsedSeconds, maxSeconds);
            return elapsedSeconds * goldPerSecond;
        }
    }

    /// <summary>
    /// Individual inventory item (hero shards, skins, consumables).
    /// </summary>
    [Serializable]
    public class InventoryItem
    {
        public string ItemID;
        public string ItemName;
        public ItemType Type;
        public int Quantity;
        public int Rarity; // 1-5 stars
    }

    /// <summary>
    /// Item type categories.
    /// </summary>
    public enum ItemType
    {
        HeroShard,
        Skin,
        Consumable,
        DefenseModule,
        Material
    }
}
