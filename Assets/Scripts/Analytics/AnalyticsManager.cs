using UnityEngine;
using System;
using System.Collections.Generic;

namespace EmpireOfGlass.Analytics
{
    /// <summary>
    /// Central analytics manager for tracking player behavior and game metrics.
    /// Implements telemetry for playtest feedback and optimization.
    /// </summary>
    public class AnalyticsManager : MonoBehaviour
    {
        public static AnalyticsManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private bool enableAnalytics = true;
        [SerializeField] private bool debugMode = false;
        [SerializeField] private float batchInterval = 10f;

        private readonly Queue<AnalyticsEvent> eventQueue = new Queue<AnalyticsEvent>();
        private string sessionId;
        private float sessionStartTime;
        private float nextBatchTime;

        // Metric counters
        private int mathGatesHit;
        private int bossesDefeated;
        private int deathCount;
        private int loopsCompleted;

        public string SessionId => sessionId;
        public float SessionDuration => Time.realtimeSinceStartup - sessionStartTime;
        public int MathGatesHit => mathGatesHit;
        public int BossesDefeated => bossesDefeated;
        public int DeathCount => deathCount;
        public int LoopsCompleted => loopsCompleted;

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
            sessionId = Guid.NewGuid().ToString();
            sessionStartTime = Time.realtimeSinceStartup;
            nextBatchTime = Time.realtimeSinceStartup + batchInterval;

            TrackSessionStart();
        }

        private void Update()
        {
            if (Time.realtimeSinceStartup >= nextBatchTime)
            {
                BatchSendEvents();
                nextBatchTime = Time.realtimeSinceStartup + batchInterval;
            }
        }

        private void OnApplicationQuit()
        {
            TrackSessionEnd("normal_exit");
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                TrackSessionEnd("app_pause");
            }
        }

        // Session Events
        public void TrackSessionStart()
        {
            TrackEvent("session_start", new Dictionary<string, object>
            {
                { "session_id", sessionId },
                { "timestamp", DateTime.UtcNow.ToString("o") },
                { "device_model", SystemInfo.deviceModel },
                { "os", SystemInfo.operatingSystem },
                { "memory_mb", SystemInfo.systemMemorySize },
                { "graphics_memory_mb", SystemInfo.graphicsMemorySize }
            });
        }

        public void TrackSessionEnd(string reason)
        {
            TrackEvent("session_end", new Dictionary<string, object>
            {
                { "session_id", sessionId },
                { "duration", SessionDuration },
                { "reason", reason },
                { "loops_completed", loopsCompleted },
                { "gates_hit", mathGatesHit },
                { "deaths", deathCount }
            });
            BatchSendEvents();
        }

        // State Transition Events
        public void TrackStateTransition(string fromState, string toState)
        {
            TrackEvent("state_transition", new Dictionary<string, object>
            {
                { "from", fromState },
                { "to", toState },
                { "timestamp", Time.realtimeSinceStartup }
            });
        }

        // Swarm Events
        public void TrackSwarmStart(int initialCount)
        {
            TrackEvent("swarm_start", new Dictionary<string, object>
            {
                { "initial_count", initialCount },
                { "timestamp", Time.realtimeSinceStartup }
            });
        }

        public void TrackMathGateHit(string gateType, int multiplier, int beforeCount, int afterCount)
        {
            mathGatesHit++;
            TrackEvent("math_gate_hit", new Dictionary<string, object>
            {
                { "gate_type", gateType },
                { "multiplier", multiplier },
                { "before_count", beforeCount },
                { "after_count", afterCount },
                { "delta", afterCount - beforeCount }
            });
        }

        public void TrackBossEncounter(int bossHP, int swarmCount)
        {
            TrackEvent("boss_encounter", new Dictionary<string, object>
            {
                { "boss_hp", bossHP },
                { "swarm_count", swarmCount },
                { "timestamp", Time.realtimeSinceStartup }
            });
        }

        public void TrackBossDefeated(int finalSwarmCount, float timeElapsed)
        {
            bossesDefeated++;
            TrackEvent("boss_defeated", new Dictionary<string, object>
            {
                { "final_swarm_count", finalSwarmCount },
                { "time_elapsed", timeElapsed },
                { "total_bosses_defeated", bossesDefeated }
            });
        }

        public void TrackSwarmFailed(string reason, int swarmCount, float progressPercent)
        {
            deathCount++;
            TrackEvent("swarm_failed", new Dictionary<string, object>
            {
                { "reason", reason },
                { "swarm_count", swarmCount },
                { "progress_percent", progressPercent },
                { "death_count", deathCount }
            });
        }

        // Loop Completion
        public void TrackLoopCompleted(float duration)
        {
            loopsCompleted++;
            TrackEvent("loop_completed", new Dictionary<string, object>
            {
                { "duration", duration },
                { "loop_number", loopsCompleted }
            });
        }

        // Performance Events
        public void TrackFrameRateSnapshot(float avgFPS, float minFPS, int swarmCount)
        {
            TrackEvent("framerate_snapshot", new Dictionary<string, object>
            {
                { "avg_fps", avgFPS },
                { "min_fps", minFPS },
                { "swarm_count", swarmCount },
                { "timestamp", Time.realtimeSinceStartup }
            });
        }

        public void TrackLoadTime(string sceneName, float duration)
        {
            TrackEvent("load_time", new Dictionary<string, object>
            {
                { "scene", sceneName },
                { "duration", duration }
            });
        }

        // Generic Event Tracking
        private void TrackEvent(string eventName, Dictionary<string, object> parameters)
        {
            if (!enableAnalytics) return;

            var evt = new AnalyticsEvent
            {
                name = eventName,
                timestamp = Time.realtimeSinceStartup,
                parameters = parameters
            };

            eventQueue.Enqueue(evt);

            if (debugMode)
            {
                Debug.Log($"[Analytics] {eventName}: {string.Join(", ", parameters)}");
            }
        }

        private void BatchSendEvents()
        {
            if (eventQueue.Count == 0) return;

            if (debugMode)
            {
                Debug.Log($"[Analytics] Batching {eventQueue.Count} events");
            }

            // In production, send to analytics service
            // For now, just log locally
            while (eventQueue.Count > 0)
            {
                var evt = eventQueue.Dequeue();
                // TODO: Send to backend analytics service
            }
        }

        // Public API for querying metrics
        public Dictionary<string, object> GetSessionSummary()
        {
            return new Dictionary<string, object>
            {
                { "session_id", sessionId },
                { "duration", SessionDuration },
                { "loops_completed", loopsCompleted },
                { "math_gates_hit", mathGatesHit },
                { "bosses_defeated", bossesDefeated },
                { "deaths", deathCount }
            };
        }
    }

    [Serializable]
    public class AnalyticsEvent
    {
        public string name;
        public float timestamp;
        public Dictionary<string, object> parameters;
    }
}
