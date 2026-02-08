using System.Collections.Generic;
using UnityEngine;

namespace EmpireOfGlass.Gacha
{
    /// <summary>
    /// Manages the Gacha/Loot Box system: light beam hits prism and splits into hero shards
    /// with high spectacle (Var 27).
    /// </summary>
    public class GachaManager : MonoBehaviour
    {
        public static GachaManager Instance { get; private set; }

        [Header("Gacha Settings (Var 27)")]
        [SerializeField] private int singlePullCostGems = 100;
        [SerializeField] private int tenPullCostGems = 900;
        [SerializeField] private int pityThreshold = 90;

        [Header("Drop Rates")]
        [SerializeField] private float fiveStarRate = 0.006f;
        [SerializeField] private float fourStarRate = 0.051f;
        [SerializeField] private float threeStarRate = 0.43f;

        private int pullsSinceLastFiveStar;

        public int PullsSinceLastFiveStar => pullsSinceLastFiveStar;
        public int SinglePullCost => singlePullCostGems;
        public int TenPullCost => tenPullCostGems;

        public event System.Action<GachaResult> OnPullComplete;
        public event System.Action<List<GachaResult>> OnMultiPullComplete;

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
        /// Perform a single gacha pull (prism-beam hero shard — Var 27).
        /// </summary>
        public GachaResult PerformSinglePull()
        {
            var player = Data.SaveManager.Instance?.CurrentPlayer;
            if (player == null || player.PremiumGems < singlePullCostGems)
            {
                Debug.Log("[GachaManager] Not enough gems for single pull");
                return null;
            }

            player.PremiumGems -= singlePullCostGems;
            var result = RollGacha();

            Debug.Log($"[GachaManager] Single pull: {result.Rarity}★ {result.ItemName}");
            OnPullComplete?.Invoke(result);
            return result;
        }

        /// <summary>
        /// Perform a 10-pull with guaranteed 4★ or higher.
        /// </summary>
        public List<GachaResult> PerformTenPull()
        {
            var player = Data.SaveManager.Instance?.CurrentPlayer;
            if (player == null || player.PremiumGems < tenPullCostGems)
            {
                Debug.Log("[GachaManager] Not enough gems for 10-pull");
                return null;
            }

            player.PremiumGems -= tenPullCostGems;
            var results = new List<GachaResult>();
            bool guaranteedFourStar = false;

            for (int i = 0; i < 10; i++)
            {
                var result = RollGacha();
                if (result.Rarity >= 4) guaranteedFourStar = true;
                results.Add(result);
            }

            // Guarantee at least one 4★ in a 10-pull
            if (!guaranteedFourStar)
            {
                results[9] = CreateResult(4);
            }

            Debug.Log($"[GachaManager] 10-pull complete. Best: {GetBestRarity(results)}★");
            OnMultiPullComplete?.Invoke(results);
            return results;
        }

        private GachaResult RollGacha()
        {
            pullsSinceLastFiveStar++;

            // Pity system: guarantee 5★ at threshold
            if (pullsSinceLastFiveStar >= pityThreshold)
            {
                pullsSinceLastFiveStar = 0;
                return CreateResult(5);
            }

            float roll = Random.value;
            if (roll < fiveStarRate)
            {
                pullsSinceLastFiveStar = 0;
                return CreateResult(5);
            }
            if (roll < fiveStarRate + fourStarRate)
            {
                return CreateResult(4);
            }
            if (roll < fiveStarRate + fourStarRate + threeStarRate)
            {
                return CreateResult(3);
            }
            return CreateResult(2);
        }

        private GachaResult CreateResult(int rarity)
        {
            return new GachaResult
            {
                ItemId = System.Guid.NewGuid().ToString(),
                ItemName = $"Hero Shard ({rarity}★)",
                Rarity = rarity,
                ShardCount = rarity * 10
            };
        }

        private int GetBestRarity(List<GachaResult> results)
        {
            int best = 0;
            foreach (var r in results)
            {
                if (r.Rarity > best) best = r.Rarity;
            }
            return best;
        }
    }

    /// <summary>
    /// Result of a single gacha pull.
    /// </summary>
    [System.Serializable]
    public class GachaResult
    {
        public string ItemId;
        public string ItemName;
        public int Rarity;
        public int ShardCount;
    }
}
