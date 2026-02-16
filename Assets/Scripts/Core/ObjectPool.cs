using System.Collections.Generic;
using UnityEngine;

namespace EmpireOfGlass.Core
{
    /// <summary>
    /// Generic object pooling system for performance optimization.
    /// Reduces garbage collection by reusing GameObjects instead of instantiate/destroy.
    /// Essential for 500+ shardling swarm optimization (Var 1, 6).
    /// </summary>
    public class ObjectPool : MonoBehaviour
    {
        [System.Serializable]
        public class Pool
        {
            public string Tag;
            public GameObject Prefab;
            public int Size;
            public bool ExpandIfNeeded = true;
        }

        public static ObjectPool Instance { get; private set; }

        [Header("Pool Configuration")]
        [SerializeField] private List<Pool> pools = new List<Pool>();

        private Dictionary<string, Queue<GameObject>> poolDictionary;
        private Dictionary<string, GameObject> prefabDictionary;
        private Dictionary<string, int> spawnCounts;

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
        /// Initialize all configured pools.
        /// </summary>
        private void InitializePools()
        {
            poolDictionary = new Dictionary<string, Queue<GameObject>>();
            prefabDictionary = new Dictionary<string, GameObject>();
            spawnCounts = new Dictionary<string, int>();

            foreach (Pool pool in pools)
            {
                Queue<GameObject> objectPool = new Queue<GameObject>();
                prefabDictionary[pool.Tag] = pool.Prefab;
                poolDictionary[pool.Tag] = objectPool;

                for (int i = 0; i < pool.Size; i++)
                {
                    GameObject obj = CreatePooledObject(pool.Prefab, pool.Tag);
                    objectPool.Enqueue(obj);
                }

                spawnCounts[pool.Tag] = 0;

                Debug.Log($"[ObjectPool] Initialized pool '{pool.Tag}' with {pool.Size} objects");
            }
        }

        /// <summary>
        /// Create a new pooled object.
        /// </summary>
        private GameObject CreatePooledObject(GameObject prefab, string tag)
        {
            GameObject obj = Instantiate(prefab);
            obj.name = $"{tag}_{poolDictionary[tag].Count}";
            obj.SetActive(false);
            obj.transform.SetParent(transform);
            return obj;
        }

        /// <summary>
        /// Spawn an object from the pool.
        /// </summary>
        public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"[ObjectPool] Pool with tag '{tag}' doesn't exist");
                return null;
            }

            GameObject objectToSpawn;

            if (poolDictionary[tag].Count == 0)
            {
                // Pool exhausted - create new object if expansion is allowed
                Pool poolConfig = pools.Find(p => p.Tag == tag);
                if (poolConfig != null && poolConfig.ExpandIfNeeded)
                {
                    objectToSpawn = CreatePooledObject(prefabDictionary[tag], tag);
                    Debug.LogWarning($"[ObjectPool] Pool '{tag}' expanded - created new object");
                }
                else
                {
                    Debug.LogWarning($"[ObjectPool] Pool '{tag}' exhausted and expansion disabled");
                    return null;
                }
            }
            else
            {
                objectToSpawn = poolDictionary[tag].Dequeue();
            }

            objectToSpawn.SetActive(true);
            objectToSpawn.transform.position = position;
            objectToSpawn.transform.rotation = rotation;
            objectToSpawn.transform.SetParent(null);

            spawnCounts[tag]++;

            // Notify pooled object component if it exists
            IPooledObject pooledObj = objectToSpawn.GetComponent<IPooledObject>();
            pooledObj?.OnObjectSpawn();

            return objectToSpawn;
        }

        /// <summary>
        /// Return an object to the pool.
        /// </summary>
        public void ReturnToPool(string tag, GameObject obj)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"[ObjectPool] Pool with tag '{tag}' doesn't exist");
                Destroy(obj);
                return;
            }

            // Notify pooled object component if it exists
            IPooledObject pooledObj = obj.GetComponent<IPooledObject>();
            pooledObj?.OnObjectReturn();

            obj.SetActive(false);
            obj.transform.SetParent(transform);
            poolDictionary[tag].Enqueue(obj);
        }

        /// <summary>
        /// Register a new pool at runtime.
        /// </summary>
        public void RegisterPool(string tag, GameObject prefab, int size, bool expandIfNeeded = true)
        {
            if (poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"[ObjectPool] Pool with tag '{tag}' already exists");
                return;
            }

            Pool newPool = new Pool
            {
                Tag = tag,
                Prefab = prefab,
                Size = size,
                ExpandIfNeeded = expandIfNeeded
            };

            pools.Add(newPool);

            Queue<GameObject> objectPool = new Queue<GameObject>();
            prefabDictionary[tag] = prefab;

            for (int i = 0; i < size; i++)
            {
                GameObject obj = CreatePooledObject(prefab, tag);
                objectPool.Enqueue(obj);
            }

            poolDictionary[tag] = objectPool;
            spawnCounts[tag] = 0;

            Debug.Log($"[ObjectPool] Registered new pool '{tag}' with {size} objects");
        }

        /// <summary>
        /// Get pool statistics for performance monitoring.
        /// </summary>
        public PoolStats GetPoolStats(string tag)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                return new PoolStats { Tag = tag, IsValid = false };
            }

            return new PoolStats
            {
                Tag = tag,
                IsValid = true,
                AvailableCount = poolDictionary[tag].Count,
                TotalSpawnCount = spawnCounts[tag]
            };
        }

        /// <summary>
        /// Clear all pools and reset.
        /// </summary>
        public void ClearPools()
        {
            foreach (var pool in poolDictionary)
            {
                while (pool.Value.Count > 0)
                {
                    GameObject obj = pool.Value.Dequeue();
                    if (obj != null) Destroy(obj);
                }
            }

            poolDictionary.Clear();
            prefabDictionary.Clear();
            spawnCounts.Clear();

            Debug.Log("[ObjectPool] All pools cleared");
        }

        /// <summary>
        /// Get all active objects by tag (for debugging).
        /// </summary>
        public int GetActiveObjectCount(string tag)
        {
            if (!poolDictionary.ContainsKey(tag)) return 0;

            Pool poolConfig = pools.Find(p => p.Tag == tag);
            if (poolConfig == null) return 0;

            int totalCreated = spawnCounts[tag];
            int available = poolDictionary[tag].Count;
            return totalCreated - available;
        }
    }

    /// <summary>
    /// Interface for objects that need to reset state when pooled/spawned.
    /// </summary>
    public interface IPooledObject
    {
        void OnObjectSpawn();
        void OnObjectReturn();
    }

    /// <summary>
    /// Pool statistics structure.
    /// </summary>
    public struct PoolStats
    {
        public string Tag;
        public bool IsValid;
        public int AvailableCount;
        public int TotalSpawnCount;
    }
}
