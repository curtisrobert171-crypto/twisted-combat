using UnityEngine;

namespace EmpireOfGlass.Ads
{
    /// <summary>
    /// Manages ad mediation and rewarded video placement (Var 42).
    /// Handles rewarded video for emergency shields, revive offers, and bonus rewards.
    /// </summary>
    public class AdMediationManager : MonoBehaviour
    {
        public static AdMediationManager Instance { get; private set; }

        [Header("Rewarded Video (Var 42)")]
        [SerializeField] private float adCooldownSeconds = 120f;
        [SerializeField] private int shieldRewardCount = 1;
        [SerializeField] private int bonusGoldReward = 50;

        private float lastAdTimestamp;
        private bool adReady;

        public bool IsAdReady => adReady;

        public event System.Action<AdRewardType> OnAdRewardGranted;
        public event System.Action OnAdFailed;

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
            RequestAd();
        }

        /// <summary>
        /// Request a rewarded video ad to be loaded and ready for display.
        /// </summary>
        public void RequestAd()
        {
            // Placeholder: in production, call ad SDK (AdMob/ironSource)
            adReady = true;
            Debug.Log("[AdMediationManager] Rewarded video ad loaded and ready");
        }

        /// <summary>
        /// Show a rewarded video ad for an emergency shield when under attack (Var 42).
        /// </summary>
        public void ShowRewardedAd(AdRewardType rewardType)
        {
            if (!CanShowAd())
            {
                Debug.Log("[AdMediationManager] Ad not available or on cooldown");
                OnAdFailed?.Invoke();
                return;
            }

            adReady = false;
            lastAdTimestamp = Time.time;

            // Placeholder: in production, show ad via SDK and wait for callback
            GrantReward(rewardType);
            RequestAd();
        }

        /// <summary>
        /// Check if an ad can be shown (loaded and not on cooldown).
        /// </summary>
        public bool CanShowAd()
        {
            if (!adReady) return false;
            if (Time.time - lastAdTimestamp < adCooldownSeconds) return false;
            return true;
        }

        /// <summary>
        /// Get remaining cooldown time before next ad can be shown.
        /// </summary>
        public float GetAdCooldownRemaining()
        {
            float elapsed = Time.time - lastAdTimestamp;
            return Mathf.Max(0f, adCooldownSeconds - elapsed);
        }

        private void GrantReward(AdRewardType rewardType)
        {
            var player = Data.SaveManager.Instance?.CurrentPlayer;
            if (player == null) return;

            switch (rewardType)
            {
                case AdRewardType.EmergencyShield:
                    player.ShieldCount += shieldRewardCount;
                    Debug.Log($"[AdMediationManager] Emergency shield granted! Shields: {player.ShieldCount}");
                    break;
                case AdRewardType.Revive:
                    Debug.Log("[AdMediationManager] Revive granted via rewarded ad");
                    break;
                case AdRewardType.BonusGold:
                    player.Gold += bonusGoldReward;
                    Debug.Log($"[AdMediationManager] Bonus gold granted: +{bonusGoldReward}");
                    break;
                case AdRewardType.DoubleLoot:
                    Debug.Log("[AdMediationManager] Double loot activated for next raid");
                    break;
            }

            OnAdRewardGranted?.Invoke(rewardType);
        }
    }

    /// <summary>
    /// Types of rewards available through rewarded video ads.
    /// </summary>
    public enum AdRewardType
    {
        EmergencyShield,
        Revive,
        BonusGold,
        DoubleLoot
    }
}
