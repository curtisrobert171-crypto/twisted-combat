using UnityEngine;

namespace EmpireOfGlass.Monetization
{
    /// <summary>
    /// Central monetization manager implementing the psychological monetization systems (Vars 23–34).
    /// Handles piggy bank, battle pass, starter pack, scarcity timers, and IAP hooks.
    /// </summary>
    public class MonetizationManager : MonoBehaviour
    {
        public static MonetizationManager Instance { get; private set; }

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

        private float merchantTimer;
        private bool merchantActive;

        public event System.Action OnPiggyBankBroken;
        public event System.Action OnStarterPackPurchased;
        public event System.Action<float> OnMerchantTimerUpdate;
        public event System.Action<int> OnBattlePassTierUp;

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
        }

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

        /// <summary>
        /// Activate the wandering merchant with a 15-minute scarcity timer (Var 28).
        /// </summary>
        public void ActivateWanderingMerchant()
        {
            merchantActive = true;
            merchantTimer = wanderingMerchantDuration;
            Debug.Log("[MonetizationManager] Wandering merchant appeared! 15-minute timer started");
        }

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

        /// <summary>
        /// Offer a revive when hero dies at high progress (Loss Aversion — Var 24).
        /// Returns true if a revive offer should be shown.
        /// </summary>
        public bool ShouldOfferRevive(float progressPercent)
        {
            return progressPercent >= 0.8f; // Offer at 80%+ progress
        }

        public float GetMerchantTimeRemaining() => merchantActive ? merchantTimer : 0f;
        public bool IsMerchantActive() => merchantActive;
    }
}
