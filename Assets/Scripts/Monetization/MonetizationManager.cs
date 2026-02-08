using System.Collections.Generic;
using UnityEngine;

namespace EmpireOfGlass.Monetization
{
    /// <summary>
    /// Central monetization manager implementing the psychological monetization systems (Vars 23–34).
    /// Handles piggy bank, battle pass, starter pack, scarcity timers, anchoring offers,
    /// gacha, social proof, VIP, endowment effect, reciprocity, shields, and IAP hooks.
    /// </summary>
    public class MonetizationManager : MonoBehaviour
    {
        public static MonetizationManager Instance { get; private set; }

        [Header("Anchoring Offer (Var 23)")]
        [SerializeField] private float anchoringDecoyPrice = 99.99f;
        [SerializeField] private float anchoringTargetPrice = 19.99f;
        [SerializeField] private int anchoringTargetGems = 500;

        [Header("Piggy Bank (Var 25)")]
        [SerializeField] private int piggyBankCapacity = 500;
        [SerializeField] private float piggyBankBreakPrice = 4.99f;

        [Header("Starter Pack (Var 30)")]
        [SerializeField] private float starterPackPrice = 0.99f;
        [SerializeField] private int starterPackGold = 500;
        [SerializeField] private int starterPackGems = 50;

        [Header("Scarcity (Var 28)")]
        [SerializeField] private float wanderingMerchantDuration = 900f; // 15 minutes

        [Header("Battle Pass (Var 26)")]
        [SerializeField] private int battlePassMaxTier = 50;
        [SerializeField] private int xpPerTier = 100;

        [Header("Gacha (Var 27)")]
        [SerializeField] private int gachaPityThreshold = 90;
        [SerializeField] private float gachaRareDropRate = 0.02f;

        [Header("Social Proof (Var 29)")]
        [SerializeField] private int socialProofMaxEntries = 20;

        [Header("VIP System (Var 31)")]
        [SerializeField] private int vipMaxLevel = 10;
        [SerializeField] private float vipMonthlyPrice = 9.99f;

        [Header("Endowment Effect (Var 32)")]
        [SerializeField] private float trialHeroDuration = 3600f; // 1 hour in seconds

        [Header("Reciprocity (Var 33)")]
        [SerializeField] private int dailyGiftGold = 50;
        [SerializeField] private int dailyGiftStreakBonus = 10;

        [Header("Shield Mechanics (Var 34)")]
        [SerializeField] private int maxShields = 3;
        [SerializeField] private float shieldDurationHours = 8f;
        [SerializeField] private float shieldRechargeCooldownHours = 4f;

        private float merchantTimer;
        private bool merchantActive;
        private readonly List<string> socialProofFeed = new List<string>();

        public event System.Action OnPiggyBankBroken;
        public event System.Action OnStarterPackPurchased;
        public event System.Action<float> OnMerchantTimerUpdate;
        public event System.Action<int> OnBattlePassTierUp;
        public event System.Action<string> OnSocialProofEntry;
        public event System.Action<Data.InventoryItem> OnGachaPull;
        public event System.Action OnVIPActivated;
        public event System.Action<string> OnTrialHeroGranted;
        public event System.Action<string> OnTrialHeroExpired;
        public event System.Action OnDailyGiftClaimed;
        public event System.Action<int> OnShieldReplenished;

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

        private void Update()
        {
            if (merchantActive)
            {
                merchantTimer -= Time.deltaTime;
                OnMerchantTimerUpdate?.Invoke(merchantTimer);
                if (merchantTimer <= 0f)
                {
                    merchantActive = false;
                    Debug.Log("[MonetizationManager] Wandering merchant left");
                }
            }

            CheckTrialHeroExpiry();
        }

        // ==================== Var 23: Anchoring Offer ====================

        /// <summary>
        /// Get the anchoring offer data: a $99 decoy bundle shown alongside the $19.99 target bundle (Var 23).
        /// </summary>
        public (float decoyPrice, float targetPrice, int targetGems) GetAnchoringOffer()
        {
            Debug.Log($"[MonetizationManager] Anchoring offer displayed: decoy ${anchoringDecoyPrice} vs target ${anchoringTargetPrice}");
            return (anchoringDecoyPrice, anchoringTargetPrice, anchoringTargetGems);
        }

        /// <summary>
        /// Purchase the anchored target bundle ($19.99).
        /// </summary>
        public void PurchaseAnchoredBundle()
        {
            var player = Data.SaveManager.Instance?.CurrentPlayer;
            if (player == null) return;

            player.PremiumGems += anchoringTargetGems;
            Debug.Log($"[MonetizationManager] Anchored bundle purchased (${anchoringTargetPrice}): +{anchoringTargetGems} gems");
        }

        // ==================== Var 25: Piggy Bank ====================

        /// <summary>
        /// Add gems to the piggy bank (visible in glass vault UI).
        /// </summary>
        public void AddToPiggyBank(int gems)
        {
            var player = Data.SaveManager.Instance?.CurrentPlayer;
            if (player == null || player.PiggyBankBroken) return;

            player.PiggyBankGems = Mathf.Min(player.PiggyBankGems + gems, piggyBankCapacity);
            Debug.Log($"[MonetizationManager] Piggy bank: {player.PiggyBankGems}/{piggyBankCapacity} gems");
        }

        /// <summary>
        /// Break the piggy bank ($4.99 IAP) and claim accumulated gems.
        /// </summary>
        public int BreakPiggyBank()
        {
            var player = Data.SaveManager.Instance?.CurrentPlayer;
            if (player == null || player.PiggyBankBroken) return 0;

            int gems = player.PiggyBankGems;
            player.PremiumGems += gems;
            player.PiggyBankGems = 0;
            player.PiggyBankBroken = true;

            Debug.Log($"[MonetizationManager] Piggy bank broken! Claimed {gems} gems (${piggyBankBreakPrice})");
            OnPiggyBankBroken?.Invoke();
            return gems;
        }

        // ==================== Var 30: Starter Pack ====================

        /// <summary>
        /// Offer and process the $0.99 starter pack after first scripted defeat (Var 30).
        /// </summary>
        public void PurchaseStarterPack()
        {
            var player = Data.SaveManager.Instance?.CurrentPlayer;
            if (player == null) return;

            player.Gold += starterPackGold;
            player.PremiumGems += starterPackGems;

            Debug.Log($"[MonetizationManager] Starter pack purchased (${starterPackPrice}): +{starterPackGold} gold, +{starterPackGems} gems");
            OnStarterPackPurchased?.Invoke();
        }

        // ==================== Var 28: Scarcity ====================

        /// <summary>
        /// Activate the wandering merchant with a 15-minute scarcity timer (Var 28).
        /// </summary>
        public void ActivateWanderingMerchant()
        {
            merchantActive = true;
            merchantTimer = wanderingMerchantDuration;
            Debug.Log("[MonetizationManager] Wandering merchant appeared! 15-minute timer started");
        }

        // ==================== Var 26: Battle Pass ====================

        /// <summary>
        /// Award battle pass XP and check for tier advancement (Var 26).
        /// </summary>
        public void AwardBattlePassXP(int xp)
        {
            var player = Data.SaveManager.Instance?.CurrentPlayer;
            if (player == null) return;

            if (player.BattlePassTier >= battlePassMaxTier) return;

            // Simplified: accumulate XP and check tier thresholds
            int totalXpForNextTier = (player.BattlePassTier + 1) * xpPerTier;
            // In a full implementation, track cumulative XP
            player.BattlePassTier++;
            OnBattlePassTierUp?.Invoke(player.BattlePassTier);

            Debug.Log($"[MonetizationManager] Battle pass tier up: {player.BattlePassTier}");
        }

        // ==================== Var 24: Loss Aversion ====================

        /// <summary>
        /// Offer a revive when hero dies at high progress (Loss Aversion — Var 24).
        /// Returns true if a revive offer should be shown.
        /// </summary>
        public bool ShouldOfferRevive(float progressPercent)
        {
            return progressPercent >= 0.8f; // Offer at 80%+ progress
        }

        // ==================== Var 27: Gacha / Loot Box ====================

        /// <summary>
        /// Perform a gacha pull — prism beam splits into hero shards (Var 27).
        /// Uses pity system: guaranteed rare at threshold.
        /// </summary>
        public Data.InventoryItem PerformGachaPull()
        {
            var player = Data.SaveManager.Instance?.CurrentPlayer;
            if (player == null) return null;

            player.GachaPity++;
            bool isRare = player.GachaPity >= gachaPityThreshold || Random.value < gachaRareDropRate;

            int rarity;
            if (isRare)
            {
                rarity = Random.Range(4, 6); // 4-5 star
                player.GachaPity = 0;
                player.TotalRarePulls++;
                string name = string.IsNullOrEmpty(player.DisplayName) ? "A player" : player.DisplayName;
                AddSocialProofEntry($"{name} pulled a {rarity}-star hero shard!");
            }
            else
            {
                rarity = Random.Range(1, 4); // 1-3 star
            }

            var item = new Data.InventoryItem
            {
                ItemID = System.Guid.NewGuid().ToString(),
                ItemName = $"Hero Shard ({rarity}★)",
                Type = Data.ItemType.HeroShard,
                Quantity = 1,
                Rarity = rarity
            };

            if (player.Inventory == null)
                player.Inventory = new List<Data.InventoryItem>();
            player.Inventory.Add(item);

            Debug.Log($"[MonetizationManager] Gacha pull: {item.ItemName} (pity: {player.GachaPity}/{gachaPityThreshold})");
            OnGachaPull?.Invoke(item);
            return item;
        }

        public int GetGachaPity()
        {
            var player = Data.SaveManager.Instance?.CurrentPlayer;
            return player?.GachaPity ?? 0;
        }

        // ==================== Var 29: Social Proof ====================

        /// <summary>
        /// Add an entry to the global social proof ticker (Var 29).
        /// </summary>
        public void AddSocialProofEntry(string message)
        {
            socialProofFeed.Add(message);
            if (socialProofFeed.Count > socialProofMaxEntries)
                socialProofFeed.RemoveAt(0);

            Debug.Log($"[MonetizationManager] Social proof: {message}");
            OnSocialProofEntry?.Invoke(message);
        }

        /// <summary>
        /// Get the current social proof feed entries.
        /// </summary>
        public List<string> GetSocialProofFeed()
        {
            return new List<string>(socialProofFeed);
        }

        // ==================== Var 31: VIP System ====================

        /// <summary>
        /// Activate or renew VIP subscription with auto-features (Var 31).
        /// </summary>
        public void ActivateVIP(int level)
        {
            var player = Data.SaveManager.Instance?.CurrentPlayer;
            if (player == null) return;

            level = Mathf.Clamp(level, 1, vipMaxLevel);
            player.VIPLevel = level;
            player.VIPExpiryTimestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds() + (30L * 24 * 3600); // 30 days

            Debug.Log($"[MonetizationManager] VIP level {level} activated (${vipMonthlyPrice}/mo), expires in 30 days");
            OnVIPActivated?.Invoke();
        }

        /// <summary>
        /// Check whether the player's VIP subscription is currently active.
        /// </summary>
        public bool IsVIPActive()
        {
            var player = Data.SaveManager.Instance?.CurrentPlayer;
            if (player == null || player.VIPLevel <= 0) return false;
            return System.DateTimeOffset.UtcNow.ToUnixTimeSeconds() < player.VIPExpiryTimestamp;
        }

        /// <summary>
        /// Get the current VIP level (0 if inactive/expired).
        /// </summary>
        public int GetVIPLevel()
        {
            return IsVIPActive() ? (Data.SaveManager.Instance?.CurrentPlayer?.VIPLevel ?? 0) : 0;
        }

        // ==================== Var 32: Endowment Effect ====================

        /// <summary>
        /// Grant a trial hero for 1 hour — remove unless purchased (Var 32).
        /// </summary>
        public void GrantTrialHero(string heroID)
        {
            var player = Data.SaveManager.Instance?.CurrentPlayer;
            if (player == null) return;

            player.TrialHeroID = heroID;
            player.TrialHeroExpiryTimestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds() + (long)trialHeroDuration;

            Debug.Log($"[MonetizationManager] Trial hero '{heroID}' granted for {trialHeroDuration / 60f:F0} minutes");
            OnTrialHeroGranted?.Invoke(heroID);
        }

        /// <summary>
        /// Check if the trial hero has expired and remove it if so.
        /// </summary>
        private void CheckTrialHeroExpiry()
        {
            var player = Data.SaveManager.Instance?.CurrentPlayer;
            if (player == null || string.IsNullOrEmpty(player.TrialHeroID)) return;

            if (System.DateTimeOffset.UtcNow.ToUnixTimeSeconds() >= player.TrialHeroExpiryTimestamp)
            {
                string expiredHero = player.TrialHeroID;
                player.TrialHeroID = "";
                player.TrialHeroExpiryTimestamp = 0;

                Debug.Log($"[MonetizationManager] Trial hero '{expiredHero}' expired — purchase to keep!");
                OnTrialHeroExpired?.Invoke(expiredHero);
            }
        }

        /// <summary>
        /// Check whether the player has an active trial hero.
        /// </summary>
        public bool HasActiveTrialHero()
        {
            var player = Data.SaveManager.Instance?.CurrentPlayer;
            if (player == null || string.IsNullOrEmpty(player.TrialHeroID)) return false;
            return System.DateTimeOffset.UtcNow.ToUnixTimeSeconds() < player.TrialHeroExpiryTimestamp;
        }

        /// <summary>
        /// Get the active trial hero ID, or empty string if none.
        /// </summary>
        public string GetTrialHeroID()
        {
            return HasActiveTrialHero() ? (Data.SaveManager.Instance?.CurrentPlayer?.TrialHeroID ?? "") : "";
        }

        // ==================== Var 33: Reciprocity ====================

        /// <summary>
        /// Claim the daily free gift to build habit (Var 33).
        /// Consecutive logins increase the bonus.
        /// </summary>
        public int ClaimDailyGift()
        {
            var player = Data.SaveManager.Instance?.CurrentPlayer;
            if (player == null) return 0;

            long now = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long secondsSinceLastGift = now - player.LastDailyGiftTimestamp;
            long oneDaySeconds = 24 * 3600;

            // Reset streak if more than 48 hours since last claim
            if (secondsSinceLastGift > oneDaySeconds * 2)
                player.DailyGiftStreak = 0;

            // Only allow one claim per 24-hour period
            if (secondsSinceLastGift < oneDaySeconds)
            {
                Debug.Log("[MonetizationManager] Daily gift already claimed today");
                return 0;
            }

            player.DailyGiftStreak++;
            player.LastDailyGiftTimestamp = now;

            int bonus = dailyGiftGold + (player.DailyGiftStreak * dailyGiftStreakBonus);
            player.Gold += bonus;

            Debug.Log($"[MonetizationManager] Daily gift claimed: +{bonus} gold (streak: {player.DailyGiftStreak})");
            OnDailyGiftClaimed?.Invoke();
            return bonus;
        }

        /// <summary>
        /// Send an alliance gift to another player (Var 33).
        /// Returns the gold amount gifted.
        /// </summary>
        public int SendAllianceGift(int giftAmount)
        {
            var player = Data.SaveManager.Instance?.CurrentPlayer;
            if (player == null || player.Gold < giftAmount) return 0;

            player.Gold -= giftAmount;
            string name = string.IsNullOrEmpty(player.DisplayName) ? "A player" : player.DisplayName;
            AddSocialProofEntry($"{name} sent a gift of {giftAmount} gold!");

            Debug.Log($"[MonetizationManager] Alliance gift sent: {giftAmount} gold");
            return giftAmount;
        }

        // ==================== Var 34: Shield Mechanics ====================

        /// <summary>
        /// Replenish shields (must log in to maintain — Var 34).
        /// Shields protect the base from raids.
        /// </summary>
        public void ReplenishShields()
        {
            var player = Data.SaveManager.Instance?.CurrentPlayer;
            if (player == null) return;

            long now = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long cooldownSeconds = (long)(shieldRechargeCooldownHours * 3600);

            if (now - player.LastShieldRechargeTimestamp < cooldownSeconds)
            {
                Debug.Log("[MonetizationManager] Shield recharge on cooldown");
                return;
            }

            if (player.ShieldCount >= maxShields)
            {
                Debug.Log("[MonetizationManager] Shields already at max");
                return;
            }

            player.ShieldCount = Mathf.Min(player.ShieldCount + 1, maxShields);
            player.LastShieldRechargeTimestamp = now;

            Debug.Log($"[MonetizationManager] Shield replenished: {player.ShieldCount}/{maxShields}");
            OnShieldReplenished?.Invoke(player.ShieldCount);
        }

        /// <summary>
        /// Activate a shield to protect the base for a set duration (Var 34).
        /// Returns true if a shield was consumed.
        /// </summary>
        public bool ActivateShield()
        {
            var player = Data.SaveManager.Instance?.CurrentPlayer;
            if (player == null || player.ShieldCount <= 0) return false;

            player.ShieldCount--;
            player.ShieldExpiryTimestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds() + (long)(shieldDurationHours * 3600);

            Debug.Log($"[MonetizationManager] Shield activated for {shieldDurationHours}h. Remaining: {player.ShieldCount}");
            return true;
        }

        /// <summary>
        /// Check whether the player's base is currently shielded.
        /// </summary>
        public bool IsBaseShielded()
        {
            var player = Data.SaveManager.Instance?.CurrentPlayer;
            if (player == null) return false;
            return System.DateTimeOffset.UtcNow.ToUnixTimeSeconds() < player.ShieldExpiryTimestamp;
        }

        // ==================== Utility ====================

        public float GetMerchantTimeRemaining() => merchantActive ? merchantTimer : 0f;
        public bool IsMerchantActive() => merchantActive;
    }
}
