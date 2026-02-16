using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EmpireOfGlass.Backend;

namespace EmpireOfGlass.UI
{
    /// <summary>
    /// UI panel for displaying leaderboards and player rankings.
    /// </summary>
    public class LeaderboardPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform entryContainer;
        [SerializeField] private GameObject entryPrefab;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Button refreshButton;
        [SerializeField] private GameObject loadingIndicator;

        [Header("Configuration")]
        [SerializeField] private string leaderboardId = "swarm_highscore";
        [SerializeField] private int entryLimit = 50;
        [SerializeField] private Color highlightColor = Color.yellow;

        private List<GameObject> entryObjects = new List<GameObject>();

        private void Start()
        {
            if (refreshButton != null)
            {
                refreshButton.onClick.AddListener(RefreshLeaderboard);
            }

            if (loadingIndicator != null)
            {
                loadingIndicator.SetActive(false);
            }

            RefreshLeaderboard();
        }

        /// <summary>
        /// Refresh leaderboard data from backend.
        /// </summary>
        public void RefreshLeaderboard()
        {
            if (APIClient.Instance == null || !APIClient.Instance.IsAuthenticated)
            {
                Debug.LogWarning("[LeaderboardPanel] Not authenticated, cannot refresh leaderboard");
                return;
            }

            if (loadingIndicator != null)
            {
                loadingIndicator.SetActive(true);
            }

            APIClient.Instance.GetLeaderboard(leaderboardId, entryLimit, OnLeaderboardReceived);
        }

        private void OnLeaderboardReceived(List<APIClient.LeaderboardEntry> entries)
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.SetActive(false);
            }

            ClearEntries();

            if (entries == null || entries.Count == 0)
            {
                Debug.Log("[LeaderboardPanel] No leaderboard entries received");
                return;
            }

            foreach (var entry in entries)
            {
                CreateEntry(entry);
            }

            Debug.Log($"[LeaderboardPanel] Displayed {entries.Count} leaderboard entries");
        }

        private void CreateEntry(APIClient.LeaderboardEntry entry)
        {
            if (entryPrefab == null || entryContainer == null) return;

            GameObject entryObj = Instantiate(entryPrefab, entryContainer);
            entryObjects.Add(entryObj);

            // Get text components (assumes prefab has these named children)
            var rankText = entryObj.transform.Find("RankText")?.GetComponent<TextMeshProUGUI>();
            var nameText = entryObj.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            var scoreText = entryObj.transform.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();

            if (rankText != null)
            {
                rankText.text = $"#{entry.rank}";
            }

            if (nameText != null)
            {
                nameText.text = entry.displayName;
            }

            if (scoreText != null)
            {
                scoreText.text = entry.score.ToString("N0");
            }

            // Highlight current player
            if (APIClient.Instance != null && entry.userId == APIClient.Instance.UserId)
            {
                var background = entryObj.GetComponent<Image>();
                if (background != null)
                {
                    background.color = highlightColor;
                }
            }

            // Add trophy icons for top 3
            if (entry.rank <= 3)
            {
                AddTrophyIcon(entryObj, entry.rank);
            }
        }

        private void AddTrophyIcon(GameObject entryObj, int rank)
        {
            var iconTransform = entryObj.transform.Find("TrophyIcon");
            if (iconTransform == null) return;

            var iconImage = iconTransform.GetComponent<Image>();
            if (iconImage == null) return;

            // Set trophy color based on rank
            Color trophyColor = rank switch
            {
                1 => new Color(1f, 0.84f, 0f), // Gold
                2 => new Color(0.75f, 0.75f, 0.75f), // Silver
                3 => new Color(0.8f, 0.5f, 0.2f), // Bronze
                _ => Color.white
            };

            iconImage.color = trophyColor;
            iconTransform.gameObject.SetActive(true);
        }

        private void ClearEntries()
        {
            foreach (var entry in entryObjects)
            {
                if (entry != null)
                {
                    Destroy(entry);
                }
            }
            entryObjects.Clear();
        }

        /// <summary>
        /// Set which leaderboard to display.
        /// </summary>
        public void SetLeaderboard(string newLeaderboardId, string displayTitle)
        {
            leaderboardId = newLeaderboardId;
            if (titleText != null)
            {
                titleText.text = displayTitle;
            }
            RefreshLeaderboard();
        }

        private void OnDestroy()
        {
            if (refreshButton != null)
            {
                refreshButton.onClick.RemoveListener(RefreshLeaderboard);
            }
        }
    }
}
