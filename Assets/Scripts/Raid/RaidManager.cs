using UnityEngine;

namespace EmpireOfGlass.Raid
{
    /// <summary>
    /// Manages the Raid loop: Coin-Master style PvP where the player orbits a rival base,
    /// aims a frequency beam, and shatters vaults for loot (Var 15).
    /// Raid energy is powered by the Swarm run results.
    /// </summary>
    public class RaidManager : MonoBehaviour
    {
        [Header("Raid Settings")]
        [SerializeField] private int baseRaidEnergy = 100;
        [SerializeField] private float raidDuration = 60f;
        [SerializeField] private float frequencyTolerance = 0.15f;

        [Header("Loot")]
        [SerializeField] private int baseLootGold = 50;
        [SerializeField] private int baseLootShards = 5;
        [SerializeField] private float revengeLootMultiplier = 2f;

        private int currentRaidEnergy;
        private bool raidActive;
        private float raidTimer;
        private float targetFrequency;

        public bool IsRaidActive => raidActive;
        public int CurrentEnergy => currentRaidEnergy;

        public event System.Action<RaidResult> OnRaidComplete;

        /// <summary>
        /// Start a raid with the given energy (from Swarm run).
        /// </summary>
        public void StartRaid(int energy)
        {
            currentRaidEnergy = energy;
            raidActive = true;
            raidTimer = raidDuration;
            targetFrequency = Random.Range(0.2f, 0.8f);

            Debug.Log($"[RaidManager] Raid started with {energy} energy. Target frequency: {targetFrequency:F2}");
        }

        private void Update()
        {
            if (!raidActive) return;

            raidTimer -= Time.deltaTime;
            if (raidTimer <= 0f)
            {
                EndRaid(0f);
            }
        }

        /// <summary>
        /// Player fires the frequency beam at a specific frequency.
        /// Precision determines loot quality.
        /// </summary>
        public void FireFrequencyBeam(float playerFrequency)
        {
            if (!raidActive) return;

            float precision = 1f - Mathf.Abs(playerFrequency - targetFrequency);
            precision = Mathf.Clamp01(precision);

            EndRaid(precision);
        }

        /// <summary>
        /// Calculates raid loot based on tap precision vs shield status (from TGDD Section 1).
        /// Algorithm: LootTier = floor(precision * 5), Gold = baseLoot * (1 + tier), Shards = baseShard * tier.
        /// </summary>
        public RaidResult CalculateLoot(float precision, bool isRevenge = false)
        {
            int lootTier = Mathf.FloorToInt(precision * 5);
            lootTier = Mathf.Clamp(lootTier, 0, 5);

            float multiplier = isRevenge ? revengeLootMultiplier : 1f;

            int gold = Mathf.RoundToInt(baseLootGold * (1 + lootTier) * multiplier);
            int shards = Mathf.RoundToInt(baseLootShards * lootTier * multiplier);

            return new RaidResult
            {
                LootTier = lootTier,
                Gold = gold,
                Shards = shards,
                Precision = precision,
                WasRevenge = isRevenge
            };
        }

        private void EndRaid(float precision)
        {
            raidActive = false;
            RaidResult result = CalculateLoot(precision);
            Debug.Log($"[RaidManager] Raid complete — Tier {result.LootTier}, Gold: {result.Gold}, Shards: {result.Shards}");
            OnRaidComplete?.Invoke(result);
        }
    }

    /// <summary>
    /// Data structure for raid results including loot tier and rewards.
    /// </summary>
    [System.Serializable]
    public struct RaidResult
    {
        public int LootTier;
        public int Gold;
        public int Shards;
        public float Precision;
        public bool WasRevenge;
    }
}
