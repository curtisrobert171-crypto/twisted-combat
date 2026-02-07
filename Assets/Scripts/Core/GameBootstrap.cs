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
