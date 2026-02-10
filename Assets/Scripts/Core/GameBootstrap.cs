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
        [SerializeField] private GameObject analyticsManagerPrefab;
        [SerializeField] private GameObject notificationManagerPrefab;
        [SerializeField] private GameObject cloudSaveServicePrefab;
        [SerializeField] private GameObject antiCheatValidatorPrefab;
        [SerializeField] private GameObject allianceManagerPrefab;
        [SerializeField] private GameObject liveOpsManagerPrefab;
        [SerializeField] private GameObject ftueControllerPrefab;

        private void Awake()
        {
            InitializeSystems();
        }

        private void InitializeSystems()
        {
            // Core singletons
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

            // New system singletons
            if (AnalyticsManager.Instance == null && analyticsManagerPrefab != null)
                Instantiate(analyticsManagerPrefab);

            if (NotificationManager.Instance == null && notificationManagerPrefab != null)
                Instantiate(notificationManagerPrefab);

            if (Data.CloudSaveService.Instance == null && cloudSaveServicePrefab != null)
                Instantiate(cloudSaveServicePrefab);

            if (Data.AntiCheatValidator.Instance == null && antiCheatValidatorPrefab != null)
                Instantiate(antiCheatValidatorPrefab);

            if (AllianceManager.Instance == null && allianceManagerPrefab != null)
                Instantiate(allianceManagerPrefab);

            if (Monetization.LiveOpsManager.Instance == null && liveOpsManagerPrefab != null)
                Instantiate(liveOpsManagerPrefab);

            if (FTUEController.Instance == null && ftueControllerPrefab != null)
                Instantiate(ftueControllerPrefab);

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

                // Track session start (Var 48)
                AnalyticsManager.Instance?.TrackSessionStart(playerData.UserID, playerData.Level);

                // Check FTUE (Var 12) — only invoke for new players
                if (FTUEController.Instance != null && FTUEController.Instance.ShouldStartFTUE())
                {
                    FTUEController.Instance.StartFTUE();
                }
            }

            Debug.Log("[Bootstrap] Empire of Glass initialized");
        }
    }
}
