using UnityEngine;
using System.Collections.Generic;

namespace EmpireOfGlass.Performance
{
    /// <summary>
    /// Performance monitor that tracks FPS, draw calls, and memory usage.
    /// Implements dynamic performance scaling as outlined in Phase 6.
    /// </summary>
    public class PerformanceMonitor : MonoBehaviour
    {
        public static PerformanceMonitor Instance { get; private set; }

        [Header("Display Settings")]
        [SerializeField] private bool showDebugOverlay = true;
        [SerializeField] private KeyCode toggleKey = KeyCode.F3;

        [Header("Performance Thresholds")]
        [SerializeField] private float targetFPS = 60f;
        [SerializeField] private float minAcceptableFPS = 30f;
        [SerializeField] private int sampleSize = 60;

        [Header("Scaling Settings")]
        [SerializeField] private bool enableDynamicScaling = true;

        private Queue<float> fpsHistory = new Queue<float>();
        private float currentFPS;
        private float averageFPS;
        private float minFPS = float.MaxValue;
        private float maxFPS = 0f;

        // Performance metrics
        private int frameCount;
        private float deltaTime;
        private GUIStyle overlayStyle;

        public float CurrentFPS => currentFPS;
        public float AverageFPS => averageFPS;
        public float MinFPS => minFPS;
        public float MaxFPS => maxFPS;

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
            // Initialize GUI style
            overlayStyle = new GUIStyle
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 14,
                normal = { textColor = Color.white }
            };
        }

        private void Update()
        {
            // Toggle overlay visibility
            if (Input.GetKeyDown(toggleKey))
            {
                showDebugOverlay = !showDebugOverlay;
            }

            // Calculate FPS
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            currentFPS = 1f / deltaTime;

            // Track FPS history
            fpsHistory.Enqueue(currentFPS);
            if (fpsHistory.Count > sampleSize)
            {
                fpsHistory.Dequeue();
            }

            // Calculate statistics
            CalculateStatistics();

            // Dynamic scaling check
            if (enableDynamicScaling)
            {
                CheckPerformanceScaling();
            }

            // Report to analytics periodically
            frameCount++;
            if (frameCount % 300 == 0)  // Every 300 frames (~5 seconds at 60 FPS)
            {
                ReportPerformanceMetrics();
            }
        }

        private void CalculateStatistics()
        {
            float sum = 0f;
            minFPS = float.MaxValue;
            maxFPS = 0f;

            foreach (float fps in fpsHistory)
            {
                sum += fps;
                if (fps < minFPS) minFPS = fps;
                if (fps > maxFPS) maxFPS = fps;
            }

            averageFPS = fpsHistory.Count > 0 ? sum / fpsHistory.Count : 0f;
        }

        private void CheckPerformanceScaling()
        {
            if (averageFPS < minAcceptableFPS)
            {
                // Performance is poor, scale down
                Debug.LogWarning($"[PerformanceMonitor] FPS below acceptable ({averageFPS:F1}). Consider scaling down.");
                // TODO: Implement scaling actions (reduce swarm size, lower quality, etc.)
            }
        }

        private void ReportPerformanceMetrics()
        {
            if (Analytics.AnalyticsManager.Instance != null)
            {
                var swarm = FindAnyObjectByType<Swarm.SwarmController>();
                int swarmCount = swarm != null ? swarm.ShardlingCount : 0;
                
                Analytics.AnalyticsManager.Instance.TrackFrameRateSnapshot(
                    averageFPS,
                    minFPS,
                    swarmCount
                );
            }
        }

        private void OnGUI()
        {
            if (!showDebugOverlay) return;

            int w = Screen.width;
            int h = Screen.height;

            // Create semi-transparent background
            GUI.Box(new Rect(10, 10, 250, 140), "");

            // Display performance metrics
            string text = $"=== PERFORMANCE ===\n" +
                         $"FPS: {currentFPS:F1}\n" +
                         $"Avg: {averageFPS:F1}\n" +
                         $"Min: {minFPS:F1}\n" +
                         $"Max: {maxFPS:F1}\n" +
                         $"Memory: {(System.GC.GetTotalMemory(false) / 1024 / 1024):F0} MB\n" +
                         $"Press {toggleKey} to toggle";

            // Color code based on performance
            if (currentFPS < minAcceptableFPS)
                overlayStyle.normal.textColor = Color.red;
            else if (currentFPS < targetFPS)
                overlayStyle.normal.textColor = Color.yellow;
            else
                overlayStyle.normal.textColor = Color.green;

            GUI.Label(new Rect(15, 15, 240, 130), text, overlayStyle);
        }

        /// <summary>
        /// Get performance summary for analytics or debugging.
        /// </summary>
        public Dictionary<string, float> GetPerformanceSummary()
        {
            return new Dictionary<string, float>
            {
                { "current_fps", currentFPS },
                { "average_fps", averageFPS },
                { "min_fps", minFPS },
                { "max_fps", maxFPS },
                { "memory_mb", System.GC.GetTotalMemory(false) / 1024f / 1024f }
            };
        }

        /// <summary>
        /// Reset performance statistics.
        /// </summary>
        public void ResetStatistics()
        {
            fpsHistory.Clear();
            minFPS = float.MaxValue;
            maxFPS = 0f;
            Debug.Log("[PerformanceMonitor] Statistics reset");
        }
    }
}
