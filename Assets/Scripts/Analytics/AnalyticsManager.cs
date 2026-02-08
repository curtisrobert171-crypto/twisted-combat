using System.Collections.Generic;
using UnityEngine;

namespace EmpireOfGlass.Analytics
{
    /// <summary>
    /// Manages analytics and attribution tracking: Amplitude, Crashlytics, AdMob, AppsFlyer (Var 48).
    /// Tracks player behavior, session metrics, and monetization events for retention optimization.
    /// </summary>
    public class AnalyticsManager : MonoBehaviour
    {
        public static AnalyticsManager Instance { get; private set; }

        [Header("Analytics Settings (Var 48)")]
        [SerializeField] private bool enableAnalytics = true;
        [SerializeField] private float sessionPingIntervalSeconds = 60f;

        private float sessionStartTime;
        private float lastPingTime;
        private int eventsTrackedThisSession;

        public float SessionDuration => Time.time - sessionStartTime;
        public int EventsTracked => eventsTrackedThisSession;

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

        private void Start()
        {
            sessionStartTime = Time.time;
            lastPingTime = Time.time;
            TrackEvent("session_start", null);
            Debug.Log("[AnalyticsManager] Analytics initialized (Amplitude, Crashlytics, AppsFlyer)");
        }

        private void Update()
        {
            if (!enableAnalytics) return;

            if (Time.time - lastPingTime >= sessionPingIntervalSeconds)
            {
                lastPingTime = Time.time;
                TrackEvent("session_ping", new Dictionary<string, string>
                {
                    { "duration", SessionDuration.ToString("F0") }
                });
            }
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                TrackEvent("session_background", new Dictionary<string, string>
                {
                    { "duration", SessionDuration.ToString("F0") }
                });
            }
            else
            {
                TrackEvent("session_resume", null);
            }
        }

        /// <summary>
        /// Track a custom analytics event with optional properties.
        /// In production, this sends to Amplitude and other analytics services.
        /// </summary>
        public void TrackEvent(string eventName, Dictionary<string, string> properties)
        {
            if (!enableAnalytics) return;

            eventsTrackedThisSession++;

            // Placeholder: in production, send to Amplitude SDK
            if (properties != null && properties.Count > 0)
            {
                Debug.Log($"[AnalyticsManager] Event: {eventName} | Properties: {properties.Count}");
            }
            else
            {
                Debug.Log($"[AnalyticsManager] Event: {eventName}");
            }
        }

        /// <summary>
        /// Track a game state transition for session pacing analysis.
        /// </summary>
        public void TrackStateTransition(string fromState, string toState)
        {
            TrackEvent("state_transition", new Dictionary<string, string>
            {
                { "from", fromState },
                { "to", toState }
            });
        }

        /// <summary>
        /// Track a monetization event for revenue analytics.
        /// </summary>
        public void TrackPurchase(string productId, float price, string currency)
        {
            TrackEvent("purchase", new Dictionary<string, string>
            {
                { "product_id", productId },
                { "price", price.ToString("F2") },
                { "currency", currency }
            });
        }

        /// <summary>
        /// Track swarm run metrics for gameplay tuning.
        /// </summary>
        public void TrackSwarmRun(int finalCount, int gatesHit, bool bossDefeated)
        {
            TrackEvent("swarm_run", new Dictionary<string, string>
            {
                { "final_count", finalCount.ToString() },
                { "gates_hit", gatesHit.ToString() },
                { "boss_defeated", bossDefeated.ToString() }
            });
        }

        /// <summary>
        /// Track raid completion for balancing and engagement.
        /// </summary>
        public void TrackRaidComplete(int lootTier, int goldEarned, bool wasRevenge)
        {
            TrackEvent("raid_complete", new Dictionary<string, string>
            {
                { "loot_tier", lootTier.ToString() },
                { "gold", goldEarned.ToString() },
                { "revenge", wasRevenge.ToString() }
            });
        }

        /// <summary>
        /// Record attribution data from AppsFlyer for user acquisition tracking.
        /// </summary>
        public void RecordAttribution(string source, string campaign, string adGroup)
        {
            TrackEvent("attribution", new Dictionary<string, string>
            {
                { "source", source },
                { "campaign", campaign },
                { "ad_group", adGroup }
            });
            Debug.Log($"[AnalyticsManager] Attribution: {source}/{campaign}/{adGroup}");
        }
    }
}
