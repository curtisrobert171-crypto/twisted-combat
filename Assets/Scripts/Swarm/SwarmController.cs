using System.Collections.Generic;
using UnityEngine;

namespace EmpireOfGlass.Swarm
{
    /// <summary>
    /// Manages the swarm of shardlings during the Swarm runner loop.
    /// Implements the core math: (CurrentUnits * GateValue) - EnemyHP.
    /// Handles GPU-instanced spawning of up to 500+ physics-based shardlings.
    /// </summary>
    public class SwarmController : MonoBehaviour
    {
        [Header("Swarm Settings")]
        [SerializeField] private GameObject shardlingPrefab;
        [SerializeField] private int initialShardlingCount = 1;
        [SerializeField] private int maxShardlings = 500;
        [SerializeField] private float spawnRadius = 2f;
        [SerializeField] private float flockingRadius = 5f;

        [Header("Formation")]
        [SerializeField] private float separationWeight = 1.5f;
        [SerializeField] private float cohesionWeight = 1.0f;
        [SerializeField] private float alignmentWeight = 1.0f;

        private readonly List<ShardlingBehavior> activeShardlings = new List<ShardlingBehavior>();
        private Transform heroTransform;

        public int ShardlingCount => activeShardlings.Count;

        public event System.Action<int> OnSwarmSizeChanged;

        private void Start()
        {
            var hero = FindAnyObjectByType<Core.HeroController>();
            if (hero != null)
            {
                heroTransform = hero.transform;
            }
        }

        /// <summary>
        /// Initialize the swarm with the starting shardling count.
        /// </summary>
        public void InitializeSwarm()
        {
            ClearSwarm();
            SpawnShardlings(initialShardlingCount);
        }

        /// <summary>
        /// Apply a math gate multiplier to the swarm.
        /// Supports x2, x5, +10, etc. as described in Var 13.
        /// </summary>
        public void ApplyMathGate(MathGate.GateOperation operation, int value)
        {
            int currentCount = activeShardlings.Count;
            int newCount;

            switch (operation)
            {
                case MathGate.GateOperation.Multiply:
                    newCount = Mathf.Min(currentCount * value, maxShardlings);
                    break;
                case MathGate.GateOperation.Add:
                    newCount = Mathf.Min(currentCount + value, maxShardlings);
                    break;
                case MathGate.GateOperation.Subtract:
                    newCount = Mathf.Max(1, currentCount - value);
                    break;
                default:
                    newCount = currentCount;
                    break;
            }

            int delta = newCount - currentCount;

            if (delta > 0)
            {
                SpawnShardlings(delta);
            }
            else if (delta < 0)
            {
                RemoveShardlings(-delta);
            }

            OnSwarmSizeChanged?.Invoke(activeShardlings.Count);
            Debug.Log($"[SwarmController] Gate applied: {operation} {value}. Swarm: {currentCount} → {activeShardlings.Count}");
        }

        /// <summary>
        /// Calculates damage against an enemy wall/boss.
        /// Formula: SwarmCount - EnemyHP (remaining shardlings survive).
        /// </summary>
        public int CalculateSwarmDamage(int enemyHP)
        {
            int damage = activeShardlings.Count;
            int losses = Mathf.Min(enemyHP, damage);
            RemoveShardlings(losses);
            OnSwarmSizeChanged?.Invoke(activeShardlings.Count);
            return damage;
        }

        /// <summary>
        /// Returns the energy generated from the swarm run for powering raids.
        /// </summary>
        public int GetRaidEnergy()
        {
            return activeShardlings.Count * 10;
        }

        private void SpawnShardlings(int count)
        {
            if (shardlingPrefab == null) return;

            for (int i = 0; i < count; i++)
            {
                if (activeShardlings.Count >= maxShardlings) break;

                Vector3 spawnPos = heroTransform != null
                    ? heroTransform.position + Random.insideUnitSphere * spawnRadius
                    : transform.position + Random.insideUnitSphere * spawnRadius;

                GameObject obj = Instantiate(shardlingPrefab, spawnPos, Quaternion.identity, transform);
                var shardling = obj.GetComponent<ShardlingBehavior>();
                if (shardling != null)
                {
                    shardling.Initialize(this, separationWeight, cohesionWeight, alignmentWeight, flockingRadius);
                    activeShardlings.Add(shardling);
                }
            }
        }

        private void RemoveShardlings(int count)
        {
            for (int i = 0; i < count && activeShardlings.Count > 0; i++)
            {
                int lastIndex = activeShardlings.Count - 1;
                var shardling = activeShardlings[lastIndex];
                activeShardlings.RemoveAt(lastIndex);
                if (shardling != null)
                {
                    shardling.Shatter();
                }
            }
        }

        private void ClearSwarm()
        {
            foreach (var shardling in activeShardlings)
            {
                if (shardling != null)
                {
                    Destroy(shardling.gameObject);
                }
            }
            activeShardlings.Clear();
        }

        /// <summary>
        /// Returns neighboring shardlings for flocking calculations.
        /// </summary>
        public List<ShardlingBehavior> GetNeighbors(Vector3 position, float radius)
        {
            var neighbors = new List<ShardlingBehavior>();
            float sqrRadius = radius * radius;

            foreach (var shardling in activeShardlings)
            {
                if (shardling == null) continue;
                if ((shardling.transform.position - position).sqrMagnitude <= sqrRadius)
                {
                    neighbors.Add(shardling);
                }
            }
            return neighbors;
        }
    }
}
