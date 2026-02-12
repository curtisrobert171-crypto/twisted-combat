using UnityEngine;
using System.Collections.Generic;

namespace EmpireOfGlass.Core
{
    /// <summary>
    /// Level of Detail (LOD) system for dynamic performance optimization.
    /// Adjusts visual quality based on distance, performance, and swarm count.
    /// Critical for maintaining 60 FPS with 500+ shardlings (Var 1, 6).
    /// </summary>
    public class LODSystem : MonoBehaviour
    {
        public static LODSystem Instance { get; private set; }

        [Header("LOD Configuration")]
        [SerializeField] private float highDetailDistance = 20f;
        [SerializeField] private float mediumDetailDistance = 50f;
        [SerializeField] private float updateInterval = 0.5f;

        [Header("Performance Thresholds")]
        [SerializeField] private int highQualityFPSThreshold = 55;
        [SerializeField] private int mediumQualityFPSThreshold = 40;
        [SerializeField] private int maxActiveShardlings = 500;

        [Header("Dynamic Scaling")]
        [SerializeField] private bool enableDynamicScaling = true;
        [SerializeField] private float scalingCheckInterval = 2f;

        public enum LODLevel
        {
            High,
            Medium,
            Low,
            VeryLow
        }

        private LODLevel currentGlobalLOD = LODLevel.High;
        private List<ILODObject> lodObjects = new List<ILODObject>();
        private Camera mainCamera;
        private float nextUpdateTime = 0f;
        private float nextScalingCheckTime = 0f;

        // Performance tracking
        private float[] fpsHistory = new float[60];
        private int fpsHistoryIndex = 0;

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
            mainCamera = Camera.main;
        }

        private void Update()
        {
            if (Time.time >= nextUpdateTime)
            {
                UpdateLODObjects();
                nextUpdateTime = Time.time + updateInterval;
            }

            if (enableDynamicScaling && Time.time >= nextScalingCheckTime)
            {
                CheckDynamicScaling();
                nextScalingCheckTime = Time.time + scalingCheckInterval;
            }

            TrackFPS();
        }

        /// <summary>
        /// Register an object for LOD management.
        /// </summary>
        public void RegisterLODObject(ILODObject lodObject)
        {
            if (!lodObjects.Contains(lodObject))
            {
                lodObjects.Add(lodObject);
            }
        }

        /// <summary>
        /// Unregister an object from LOD management.
        /// </summary>
        public void UnregisterLODObject(ILODObject lodObject)
        {
            lodObjects.Remove(lodObject);
        }

        /// <summary>
        /// Update LOD levels for all registered objects.
        /// </summary>
        private void UpdateLODObjects()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null) return;
            }

            Vector3 cameraPos = mainCamera.transform.position;

            foreach (var lodObject in lodObjects)
            {
                if (lodObject == null || lodObject.ObjectTransform == null) continue;

                float distance = Vector3.Distance(cameraPos, lodObject.ObjectTransform.position);
                LODLevel level = CalculateLODLevel(distance);

                // Apply global LOD override if lower quality needed
                if (currentGlobalLOD > level)
                {
                    level = currentGlobalLOD;
                }

                lodObject.SetLODLevel(level);
            }
        }

        /// <summary>
        /// Calculate LOD level based on distance.
        /// </summary>
        private LODLevel CalculateLODLevel(float distance)
        {
            if (distance < highDetailDistance)
                return LODLevel.High;
            else if (distance < mediumDetailDistance)
                return LODLevel.Medium;
            else if (distance < mediumDetailDistance * 2f)
                return LODLevel.Low;
            else
                return LODLevel.VeryLow;
        }

        /// <summary>
        /// Check performance and adjust global LOD if needed.
        /// </summary>
        private void CheckDynamicScaling()
        {
            float avgFPS = GetAverageFPS();
            int activeCount = lodObjects.Count;

            LODLevel targetLOD = LODLevel.High;

            // Determine target LOD based on performance
            if (avgFPS < mediumQualityFPSThreshold || activeCount > maxActiveShardlings * 0.8f)
            {
                targetLOD = LODLevel.Low;
            }
            else if (avgFPS < highQualityFPSThreshold || activeCount > maxActiveShardlings * 0.6f)
            {
                targetLOD = LODLevel.Medium;
            }

            if (targetLOD != currentGlobalLOD)
            {
                currentGlobalLOD = targetLOD;
                Debug.Log($"[LODSystem] Global LOD changed to {currentGlobalLOD} (FPS: {avgFPS:F1}, Active: {activeCount})");

                // Track analytics
                if (Analytics.AnalyticsManager.Instance != null)
                {
                    Analytics.AnalyticsManager.Instance.TrackEvent("LOD_Changed", new Dictionary<string, object>
                    {
                        { "lod_level", currentGlobalLOD.ToString() },
                        { "avg_fps", avgFPS },
                        { "active_count", activeCount }
                    });
                }
            }
        }

        /// <summary>
        /// Track FPS for dynamic scaling decisions.
        /// </summary>
        private void TrackFPS()
        {
            fpsHistory[fpsHistoryIndex] = 1f / Time.unscaledDeltaTime;
            fpsHistoryIndex = (fpsHistoryIndex + 1) % fpsHistory.Length;
        }

        /// <summary>
        /// Get average FPS from recent history.
        /// </summary>
        private float GetAverageFPS()
        {
            float sum = 0f;
            foreach (float fps in fpsHistory)
            {
                sum += fps;
            }
            return sum / fpsHistory.Length;
        }

        /// <summary>
        /// Force a specific global LOD level (for testing or user settings).
        /// </summary>
        public void SetGlobalLOD(LODLevel level)
        {
            currentGlobalLOD = level;
            enableDynamicScaling = false;
            Debug.Log($"[LODSystem] Manual global LOD set to {level}");
        }

        /// <summary>
        /// Re-enable dynamic LOD scaling.
        /// </summary>
        public void EnableDynamicLOD()
        {
            enableDynamicScaling = true;
            Debug.Log("[LODSystem] Dynamic LOD scaling enabled");
        }

        /// <summary>
        /// Get current global LOD level.
        /// </summary>
        public LODLevel GetGlobalLOD() => currentGlobalLOD;

        /// <summary>
        /// Get count of registered LOD objects.
        /// </summary>
        public int GetLODObjectCount() => lodObjects.Count;
    }

    /// <summary>
    /// Interface for objects that support LOD.
    /// </summary>
    public interface ILODObject
    {
        Transform ObjectTransform { get; }
        void SetLODLevel(LODSystem.LODLevel level);
    }

    /// <summary>
    /// Example LOD-enabled component for shardlings.
    /// </summary>
    public class ShardlingLOD : MonoBehaviour, ILODObject
    {
        [Header("LOD Renderers")]
        [SerializeField] private GameObject highDetailModel;
        [SerializeField] private GameObject mediumDetailModel;
        [SerializeField] private GameObject lowDetailModel;

        [Header("Components")]
        [SerializeField] private ParticleSystem particleEffect;
        [SerializeField] private TrailRenderer trailRenderer;

        private LODSystem.LODLevel currentLOD = LODSystem.LODLevel.High;

        public Transform ObjectTransform => transform;

        private void OnEnable()
        {
            LODSystem.Instance?.RegisterLODObject(this);
        }

        private void OnDisable()
        {
            LODSystem.Instance?.UnregisterLODObject(this);
        }

        public void SetLODLevel(LODSystem.LODLevel level)
        {
            if (currentLOD == level) return;

            currentLOD = level;

            switch (level)
            {
                case LODSystem.LODLevel.High:
                    SetActiveModels(true, false, false);
                    SetEffects(true, true);
                    break;

                case LODSystem.LODLevel.Medium:
                    SetActiveModels(false, true, false);
                    SetEffects(true, false);
                    break;

                case LODSystem.LODLevel.Low:
                    SetActiveModels(false, false, true);
                    SetEffects(false, false);
                    break;

                case LODSystem.LODLevel.VeryLow:
                    SetActiveModels(false, false, true);
                    SetEffects(false, false);
                    break;
            }
        }

        private void SetActiveModels(bool high, bool medium, bool low)
        {
            if (highDetailModel != null) highDetailModel.SetActive(high);
            if (mediumDetailModel != null) mediumDetailModel.SetActive(medium);
            if (lowDetailModel != null) lowDetailModel.SetActive(low);
        }

        private void SetEffects(bool particles, bool trail)
        {
            if (particleEffect != null)
            {
                if (particles && !particleEffect.isPlaying)
                    particleEffect.Play();
                else if (!particles && particleEffect.isPlaying)
                    particleEffect.Stop();
            }

            if (trailRenderer != null)
            {
                trailRenderer.enabled = trail;
            }
        }
    }
}
