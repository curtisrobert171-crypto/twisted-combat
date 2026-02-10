using System.Collections.Generic;
using UnityEngine;

namespace EmpireOfGlass.Core
{
    /// <summary>
    /// Centralized analytics tracking for all game events (Var 48).
    /// In production, integrates with Amplitude, Crashlytics, and AppsFlyer.
    /// Provides hooks for tracking retention, monetization, and engagement metrics.
    /// </summary>
    public class AnalyticsManager : MonoBehaviour
    {
        public static AnalyticsManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private bool enableDebugLogging = true;
        [SerializeField] private int eventBatchSize = 10;

        private readonly List<AnalyticsEvent> eventQueue = new List<AnalyticsEvent>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Track a generic game event with optional properties.
        /// </summary>
        public void TrackEvent(string eventName, Dictionary<string, object> properties = null)
        {
            var evt = new AnalyticsEvent
            {
                Name = eventName,
                Timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Properties = properties ?? new Dictionary<string, object>()
            };

            eventQueue.Add(evt);

            if (enableDebugLogging)
            {
                Debug.Log($"[AnalyticsManager] Event: {eventName}");
            }

            // In production: send to Amplitude SDK
            if (eventQueue.Count >= eventBatchSize)
            {
                FlushEvents();
            }
        }

        // ==================== Session Events ====================

        /// <summary>
        /// Track session start with player context.
        /// </summary>
        public void TrackSessionStart(string userId, int playerLevel)
        {
            TrackEvent("session_start", new Dictionary<string, object>
            {
                { "user_id", userId },
                { "player_level", playerLevel }
            });
        }

        /// <summary>
        /// Track session end with duration.
        /// </summary>
        public void TrackSessionEnd(float durationSeconds)
        {
            TrackEvent("session_end", new Dictionary<string, object>
            {
                { "duration_seconds", durationSeconds }
            });
        }

        // ==================== Gameplay Events ====================

        /// <summary>
        /// Track swarm run completion with stats.
        /// </summary>
        public void TrackSwarmComplete(int shardlingCount, int bossHP, bool bossDefeated)
        {
            TrackEvent("swarm_complete", new Dictionary<string, object>
            {
                { "shardling_count", shardlingCount },
                { "boss_hp", bossHP },
                { "boss_defeated", bossDefeated }
            });
        }

        /// <summary>
        /// Track raid completion with loot tier and precision.
        /// </summary>
        public void TrackRaidComplete(int lootTier, float precision, int goldEarned, bool wasRevenge)
        {
            TrackEvent("raid_complete", new Dictionary<string, object>
            {
                { "loot_tier", lootTier },
                { "precision", precision },
                { "gold_earned", goldEarned },
                { "was_revenge", wasRevenge }
            });
        }

        /// <summary>
        /// Track city building placement.
        /// </summary>
        public void TrackBuildingPlaced(string buildingType, int gridX, int gridY)
        {
            TrackEvent("building_placed", new Dictionary<string, object>
            {
                { "building_type", buildingType },
                { "grid_x", gridX },
                { "grid_y", gridY }
            });
        }

        // ==================== Monetization Events ====================

        /// <summary>
        /// Track an in-app purchase.
        /// </summary>
        public void TrackPurchase(string productId, float price, string currency = "USD")
        {
            TrackEvent("purchase", new Dictionary<string, object>
            {
                { "product_id", productId },
                { "price", price },
                { "currency", currency }
            });
        }

        /// <summary>
        /// Track a rewarded ad view.
        /// </summary>
        public void TrackAdView(string placement, bool completed)
        {
            TrackEvent("ad_view", new Dictionary<string, object>
            {
                { "placement", placement },
                { "completed", completed }
            });
        }

        /// <summary>
        /// Track gacha pull results for drop rate monitoring.
        /// </summary>
        public void TrackGachaPull(int rarity, int pityCount)
        {
            TrackEvent("gacha_pull", new Dictionary<string, object>
            {
                { "rarity", rarity },
                { "pity_count", pityCount }
            });
        }

        /// <summary>
        /// Track battle pass tier progression.
        /// </summary>
        public void TrackBattlePassTierUp(int tier, bool isPremium)
        {
            TrackEvent("battle_pass_tier_up", new Dictionary<string, object>
            {
                { "tier", tier },
                { "is_premium", isPremium }
            });
        }

        // ==================== Attribution Events ====================

        /// <summary>
        /// Track install attribution source (AppsFlyer integration — Var 48).
        /// </summary>
        public void TrackAttribution(string source, string campaign, string medium)
        {
            TrackEvent("attribution", new Dictionary<string, object>
            {
                { "source", source },
                { "campaign", campaign },
                { "medium", medium }
            });
        }

        /// <summary>
        /// Track referral invite (Var 43 — Viral Loop).
        /// </summary>
        public void TrackReferral(string referrerUserId)
        {
            TrackEvent("referral", new Dictionary<string, object>
            {
                { "referrer_user_id", referrerUserId }
            });
        }

        // ==================== Flush ====================

        /// <summary>
        /// Flush queued events to the analytics backend.
        /// In production, sends batch to Amplitude/backend.
        /// </summary>
        public void FlushEvents()
        {
            if (eventQueue.Count == 0) return;

            // In production: serialize and send to Amplitude REST API
            if (enableDebugLogging)
            {
                Debug.Log($"[AnalyticsManager] Flushing {eventQueue.Count} events");
            }

            eventQueue.Clear();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                FlushEvents();
            }
        }

        private void OnApplicationQuit()
        {
            FlushEvents();
        }
    }

    /// <summary>
    /// Analytics event data structure for batching.
    /// </summary>
    [System.Serializable]
    public class AnalyticsEvent
    {
        public string Name;
        public long Timestamp;
        public Dictionary<string, object> Properties;
    }
}
