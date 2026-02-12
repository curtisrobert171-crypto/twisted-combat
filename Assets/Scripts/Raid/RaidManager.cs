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

        [Header("Coin-Master Clone Spin Settings")]
        [SerializeField] private int maxSpins = 5;
        [SerializeField] private int spinCostEnergy = 20;
        [SerializeField] private int coinRewardPerCoinSymbol = 100;
        [SerializeField] private int shieldRewardPerShieldSymbol = 1;
        [SerializeField] private int attackRewardPerHammerSymbol = 1;

        private int currentRaidEnergy;
        private bool raidActive;
        private float raidTimer;
        private float targetFrequency;
        private int currentSpins;

        public bool IsRaidActive => raidActive;
        public int CurrentEnergy => currentRaidEnergy;
        public int CurrentSpins => currentSpins;
        public float TargetFrequency => targetFrequency;

        public event System.Action<RaidResult> OnRaidComplete;
        public event System.Action<SpinResult> OnSpinResolved;

        /// <summary>
        /// Start a raid with the given energy (from Swarm run).
        /// </summary>
        public void StartRaid(int energy)
        {
            currentRaidEnergy = Mathf.Max(energy, baseRaidEnergy);
            raidActive = true;
            raidTimer = raidDuration;
            targetFrequency = Random.Range(0.2f, 0.8f);
            currentSpins = Mathf.Min(maxSpins, Mathf.Max(0, currentRaidEnergy / spinCostEnergy));

            Debug.Log($"[RaidManager] Raid started with {currentRaidEnergy} energy. Target frequency: {targetFrequency:F2}. Spins: {currentSpins}");
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

            float precision = EvaluateFrequencyPrecision(playerFrequency);
            EndRaid(precision);
        }

        /// <summary>
        /// Evaluate how close player frequency was to target. Uses frequencyTolerance as the max perfect window.
        /// </summary>
        public float EvaluateFrequencyPrecision(float playerFrequency)
        {
            float distance = Mathf.Abs(playerFrequency - targetFrequency);
            float normalizedTolerance = Mathf.Max(0.01f, frequencyTolerance);
            float precision = 1f - (distance / normalizedTolerance);
            return Mathf.Clamp01(precision);
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

        /// <summary>
        /// Executes a Coin-Master-style slot spin.
        /// 3-of-a-kind Hammer = Attack, Pig = Raid, Shield = Shield gain, Coin = coin payout.
        /// Mixed symbols still grant partial payouts for Coin/Shield/Hammer symbols.
        /// </summary>
        public SpinResult Spin()
        {
            if (!raidActive || currentSpins <= 0 || currentRaidEnergy < spinCostEnergy)
            {
                return SpinResult.Empty;
            }

            currentRaidEnergy -= spinCostEnergy;
            currentSpins--;

            SpinSymbol[] reel = new SpinSymbol[3];
            for (int i = 0; i < reel.Length; i++)
            {
                reel[i] = (SpinSymbol)Random.Range(0, 4);
            }

            SpinResult result = EvaluateSpin(reel);
            OnSpinResolved?.Invoke(result);
            return result;
        }

        /// <summary>
        /// Shared evaluator for random and test-driven spin outcomes.
        /// </summary>
        public SpinResult EvaluateSpin(SpinSymbol[] reel)
        {
            if (reel == null || reel.Length != 3)
            {
                return SpinResult.Empty;
            }

            int coins = 0;
            int shields = 0;
            int attacks = 0;
            bool triggeredRaid = false;

            bool allMatch = reel[0] == reel[1] && reel[1] == reel[2];
            if (allMatch)
            {
                switch (reel[0])
                {
                    case SpinSymbol.Coin:
                        coins = coinRewardPerCoinSymbol * 10;
                        break;
                    case SpinSymbol.Shield:
                        shields = shieldRewardPerShieldSymbol * 3;
                        break;
                    case SpinSymbol.Hammer:
                        attacks = attackRewardPerHammerSymbol * 3;
                        break;
                    case SpinSymbol.Pig:
                        triggeredRaid = true;
                        break;
                }
            }
            else
            {
                for (int i = 0; i < reel.Length; i++)
                {
                    switch (reel[i])
                    {
                        case SpinSymbol.Coin:
                            coins += coinRewardPerCoinSymbol;
                            break;
                        case SpinSymbol.Shield:
                            shields += shieldRewardPerShieldSymbol;
                            break;
                        case SpinSymbol.Hammer:
                            attacks += attackRewardPerHammerSymbol;
                            break;
                    }
                }
            }

            return new SpinResult
            {
                First = reel[0],
                Second = reel[1],
                Third = reel[2],
                CoinsAwarded = coins,
                ShieldsAwarded = shields,
                AttacksAwarded = attacks,
                TriggeredRaid = triggeredRaid
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

    public enum SpinSymbol
    {
        Coin = 0,
        Shield = 1,
        Hammer = 2,
        Pig = 3
    }

    [System.Serializable]
    public struct SpinResult
    {
        public SpinSymbol First;
        public SpinSymbol Second;
        public SpinSymbol Third;
        public int CoinsAwarded;
        public int ShieldsAwarded;
        public int AttacksAwarded;
        public bool TriggeredRaid;

        public static SpinResult Empty => new SpinResult
        {
            First = SpinSymbol.Coin,
            Second = SpinSymbol.Coin,
            Third = SpinSymbol.Coin,
            CoinsAwarded = 0,
            ShieldsAwarded = 0,
            AttacksAwarded = 0,
            TriggeredRaid = false
        };
    }
}
