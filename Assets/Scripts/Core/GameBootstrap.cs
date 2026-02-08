using UnityEngine;

namespace EmpireOfGlass.Core
{
    /// <summary>
    /// Bootstraps the game by initializing all core systems on launch.
    /// Attach this to the root GameObject in the starting scene.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Core Prefabs")]
        [SerializeField] private GameObject gameManagerPrefab;
        [SerializeField] private GameObject saveManagerPrefab;
        [SerializeField] private GameObject monetizationManagerPrefab;
        [SerializeField] private GameObject audioManagerPrefab;
        [SerializeField] private GameObject hapticManagerPrefab;

        [Header("New System Prefabs")]
        [SerializeField] private GameObject socialManagerPrefab;
        [SerializeField] private GameObject liveOpsManagerPrefab;
        [SerializeField] private GameObject gachaManagerPrefab;
        [SerializeField] private GameObject antiCheatManagerPrefab;
        [SerializeField] private GameObject adMediationManagerPrefab;
        [SerializeField] private GameObject viralManagerPrefab;
        [SerializeField] private GameObject analyticsManagerPrefab;

        private void Awake()
        {
            InitializeSystems();
        }

        private void InitializeSystems()
        {
            // Ensure singletons are created
            if (GameManager.Instance == null && gameManagerPrefab != null)
                Instantiate(gameManagerPrefab);

            if (Data.SaveManager.Instance == null && saveManagerPrefab != null)
                Instantiate(saveManagerPrefab);

            if (Monetization.MonetizationManager.Instance == null && monetizationManagerPrefab != null)
                Instantiate(monetizationManagerPrefab);

            if (AudioManager.Instance == null && audioManagerPrefab != null)
                Instantiate(audioManagerPrefab);

            if (HapticManager.Instance == null && hapticManagerPrefab != null)
                Instantiate(hapticManagerPrefab);

            // New systems (Vars 18–19, 21, 27, 41, 42, 43, 48)
            if (Social.SocialManager.Instance == null && socialManagerPrefab != null)
                Instantiate(socialManagerPrefab);

            if (LiveOps.LiveOpsManager.Instance == null && liveOpsManagerPrefab != null)
                Instantiate(liveOpsManagerPrefab);

            if (Gacha.GachaManager.Instance == null && gachaManagerPrefab != null)
                Instantiate(gachaManagerPrefab);

            if (AntiCheat.AntiCheatManager.Instance == null && antiCheatManagerPrefab != null)
                Instantiate(antiCheatManagerPrefab);

            if (Ads.AdMediationManager.Instance == null && adMediationManagerPrefab != null)
                Instantiate(adMediationManagerPrefab);

            if (Viral.ViralManager.Instance == null && viralManagerPrefab != null)
                Instantiate(viralManagerPrefab);

            if (Analytics.AnalyticsManager.Instance == null && analyticsManagerPrefab != null)
                Instantiate(analyticsManagerPrefab);

            // Load player data
            var save = Data.SaveManager.Instance;
            if (save != null)
            {
                var playerData = save.LoadPlayerData();

                // Claim offline rewards (Var 20)
                float offlineGold = save.ClaimOfflineRewards(goldPerSecond: 0.5f);
                if (offlineGold > 0)
                {
                    Debug.Log($"[Bootstrap] Welcome back! Earned {offlineGold:F0} gold while away.");
                }
            }

            Debug.Log("[Bootstrap] Empire of Glass initialized");
        }
    }
}
