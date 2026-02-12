using UnityEngine;
using System.Collections.Generic;

namespace EmpireOfGlass.Core
{
    /// <summary>
    /// Manages particle effects with pooling for performance.
    /// Provides pre-configured effects for common game events.
    /// </summary>
    public class ParticleEffectManager : MonoBehaviour
    {
        public static ParticleEffectManager Instance { get; private set; }

        [Header("Particle Prefabs")]
        [SerializeField] private ParticleSystem gateHitEffect;
        [SerializeField] private ParticleSystem shatterEffect;
        [SerializeField] private ParticleSystem multiplierEffect;
        [SerializeField] private ParticleSystem lootCollectEffect;
        [SerializeField] private ParticleSystem buildCompleteEffect;
        [SerializeField] private ParticleSystem levelUpEffect;
        [SerializeField] private ParticleSystem impactEffect;

        [Header("Configuration")]
        [SerializeField] private int poolSize = 20;
        [SerializeField] private bool usePooling = true;

        private Dictionary<string, Queue<ParticleSystem>> particlePools;
        private Dictionary<string, ParticleSystem> particlePrefabs;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializePools();
        }

        /// <summary>
        /// Initialize particle effect pools.
        /// </summary>
        private void InitializePools()
        {
            if (!usePooling) return;

            particlePools = new Dictionary<string, Queue<ParticleSystem>>();
            particlePrefabs = new Dictionary<string, ParticleSystem>
            {
                { "gate_hit", gateHitEffect },
                { "shatter", shatterEffect },
                { "multiplier", multiplierEffect },
                { "loot_collect", lootCollectEffect },
                { "build_complete", buildCompleteEffect },
                { "level_up", levelUpEffect },
                { "impact", impactEffect }
            };

            foreach (var kvp in particlePrefabs)
            {
                if (kvp.Value == null) continue;

                Queue<ParticleSystem> pool = new Queue<ParticleSystem>();

                for (int i = 0; i < poolSize; i++)
                {
                    ParticleSystem ps = Instantiate(kvp.Value);
                    ps.gameObject.SetActive(false);
                    ps.transform.SetParent(transform);
                    pool.Enqueue(ps);
                }

                particlePools[kvp.Key] = pool;
            }

            Debug.Log($"[ParticleEffectManager] Initialized {particlePrefabs.Count} particle pools");
        }

        /// <summary>
        /// Play a particle effect at a position.
        /// </summary>
        public void PlayEffect(string effectName, Vector3 position, Quaternion rotation, float scale = 1f)
        {
            ParticleSystem ps = GetParticleSystem(effectName);
            if (ps == null) return;

            ps.transform.position = position;
            ps.transform.rotation = rotation;
            ps.transform.localScale = Vector3.one * scale;
            ps.gameObject.SetActive(true);
            ps.Play();

            // Auto-return to pool
            if (usePooling)
            {
                StartCoroutine(ReturnToPoolAfterDuration(effectName, ps));
            }
            else
            {
                Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
            }
        }

        /// <summary>
        /// Get particle system from pool or create new one.
        /// </summary>
        private ParticleSystem GetParticleSystem(string effectName)
        {
            if (usePooling && particlePools.ContainsKey(effectName))
            {
                if (particlePools[effectName].Count > 0)
                {
                    return particlePools[effectName].Dequeue();
                }
                else if (particlePrefabs.ContainsKey(effectName))
                {
                    // Pool exhausted, create new
                    ParticleSystem ps = Instantiate(particlePrefabs[effectName]);
                    Debug.LogWarning($"[ParticleEffectManager] Pool '{effectName}' exhausted, creating new instance");
                    return ps;
                }
            }
            else if (particlePrefabs.ContainsKey(effectName))
            {
                return Instantiate(particlePrefabs[effectName]);
            }

            Debug.LogWarning($"[ParticleEffectManager] Effect '{effectName}' not found");
            return null;
        }

        /// <summary>
        /// Return particle system to pool after its duration.
        /// </summary>
        private System.Collections.IEnumerator ReturnToPoolAfterDuration(string effectName, ParticleSystem ps)
        {
            yield return new WaitForSeconds(ps.main.duration + ps.main.startLifetime.constantMax);

            if (ps != null)
            {
                ps.Stop();
                ps.gameObject.SetActive(false);
                ps.transform.SetParent(transform);

                if (particlePools.ContainsKey(effectName))
                {
                    particlePools[effectName].Enqueue(ps);
                }
            }
        }

        #region Convenience Methods

        /// <summary>
        /// Play gate hit effect.
        /// </summary>
        public void PlayGateHit(Vector3 position, float scale = 1f)
        {
            PlayEffect("gate_hit", position, Quaternion.identity, scale);

            // Play audio
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayGateActivate();
            }

            // Light shake
            if (JuiceManager.Instance != null)
            {
                JuiceManager.Instance.LightShake();
            }
        }

        /// <summary>
        /// Play shatter effect.
        /// </summary>
        public void PlayShatter(Vector3 position, float scale = 1f)
        {
            PlayEffect("shatter", position, Quaternion.identity, scale);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayShatter();
            }

            if (JuiceManager.Instance != null)
            {
                JuiceManager.Instance.MediumShake();
            }
        }

        /// <summary>
        /// Play multiplier effect.
        /// </summary>
        public void PlayMultiplier(Vector3 position, float multiplier)
        {
            float scale = Mathf.Clamp(multiplier / 5f, 0.5f, 2f);
            PlayEffect("multiplier", position, Quaternion.identity, scale);
        }

        /// <summary>
        /// Play loot collection effect.
        /// </summary>
        public void PlayLootCollect(Vector3 position)
        {
            PlayEffect("loot_collect", position, Quaternion.identity);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayLoot();
            }
        }

        /// <summary>
        /// Play building completion effect.
        /// </summary>
        public void PlayBuildComplete(Vector3 position)
        {
            PlayEffect("build_complete", position, Quaternion.identity);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayBuildComplete();
            }
        }

        /// <summary>
        /// Play level up effect.
        /// </summary>
        public void PlayLevelUp(Vector3 position)
        {
            PlayEffect("level_up", position, Quaternion.identity, 1.5f);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayBuildComplete();
            }

            if (JuiceManager.Instance != null)
            {
                JuiceManager.Instance.MediumShake();
            }
        }

        /// <summary>
        /// Play impact effect.
        /// </summary>
        public void PlayImpact(Vector3 position, float intensity = 1f)
        {
            PlayEffect("impact", position, Quaternion.identity, intensity);

            if (JuiceManager.Instance != null)
            {
                JuiceManager.Instance.ScreenShake(intensity);
                JuiceManager.Instance.HitPause();
            }
        }

        #endregion

        /// <summary>
        /// Clear all active particles.
        /// </summary>
        public void ClearAllParticles()
        {
            if (!usePooling) return;

            foreach (var pool in particlePools.Values)
            {
                foreach (var ps in pool)
                {
                    if (ps != null)
                    {
                        ps.Stop();
                        ps.Clear();
                    }
                }
            }
        }
    }
}
