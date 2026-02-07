using UnityEngine;

namespace EmpireOfGlass.Data
{
    /// <summary>
    /// Manages saving and loading player data.
    /// Supports local save (PlayerPrefs) and prepares for cloud save via PlayFab/Firebase (Var 40, 45).
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        private const string SaveKey = "EmpireOfGlass_PlayerSave";

        public static SaveManager Instance { get; private set; }

        private PlayerData currentPlayer;

        public PlayerData CurrentPlayer => currentPlayer;

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
        /// Load player data from local storage. Falls back to creating a new profile.
        /// </summary>
        public PlayerData LoadPlayerData()
        {
            if (PlayerPrefs.HasKey(SaveKey))
            {
                string json = PlayerPrefs.GetString(SaveKey);
                currentPlayer = PlayerData.FromJson(json);
                Debug.Log($"[SaveManager] Loaded player: {currentPlayer.DisplayName} (Level {currentPlayer.Level})");
            }
            else
            {
                currentPlayer = PlayerData.CreateNew(System.Guid.NewGuid().ToString());
                Debug.Log("[SaveManager] Created new player profile");
            }

            return currentPlayer;
        }

        /// <summary>
        /// Save current player data to local storage.
        /// </summary>
        public void SavePlayerData()
        {
            if (currentPlayer == null) return;

            currentPlayer.LastLoginTimestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string json = currentPlayer.ToJson();
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();

            Debug.Log("[SaveManager] Player data saved");
        }

        /// <summary>
        /// Apply offline rewards when player returns.
        /// </summary>
        public float ClaimOfflineRewards(float goldPerSecond)
        {
            if (currentPlayer == null) return 0f;

            float rewards = currentPlayer.CalculateOfflineRewards(goldPerSecond);
            currentPlayer.Gold += Mathf.FloorToInt(rewards);
            currentPlayer.LastOfflineTimestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            Debug.Log($"[SaveManager] Offline rewards claimed: {rewards:F0} gold");
            return rewards;
        }
    }
}
