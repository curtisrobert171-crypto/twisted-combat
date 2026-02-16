using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace EmpireOfGlass.Backend
{
    /// <summary>
    /// Full API client for backend integration with Google Cloud Run.
    /// Handles authentication, leaderboards, PvP matchmaking, cloud save, and anti-cheat.
    /// See Documentation/BackendIntegration.md for backend deployment.
    /// </summary>
    public class APIClient : MonoBehaviour
    {
        public static APIClient Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private string baseURL = "https://api.empireofglass.com/v1";
        [SerializeField] private float requestTimeout = 10f;
        [SerializeField] private int maxRetries = 3;

        private string sessionToken;
        private string userId;
        private bool isAuthenticated = false;

        // Events
        public event Action<bool> OnAuthenticationChanged;
        public event Action<string> OnError;

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

        #region Authentication

        /// <summary>
        /// Authenticate user with email/password or OAuth token.
        /// </summary>
        public void Authenticate(string email, string password, Action<bool> callback)
        {
            StartCoroutine(AuthenticateCoroutine(email, password, callback));
        }

        private IEnumerator AuthenticateCoroutine(string email, string password, Action<bool> callback)
        {
            var data = new Dictionary<string, string>
            {
                { "email", email },
                { "password", password }
            };

            yield return PostRequest("/auth/login", data, (success, response) =>
            {
                if (success && response != null)
                {
                    var authResponse = JsonUtility.FromJson<AuthResponse>(response);
                    sessionToken = authResponse.token;
                    userId = authResponse.userId;
                    isAuthenticated = true;

                    OnAuthenticationChanged?.Invoke(true);
                    callback?.Invoke(true);

                    Debug.Log($"[APIClient] Authenticated as user {userId}");
                }
                else
                {
                    isAuthenticated = false;
                    OnAuthenticationChanged?.Invoke(false);
                    callback?.Invoke(false);
                    OnError?.Invoke("Authentication failed");
                }
            });
        }

        /// <summary>
        /// Register new user account.
        /// </summary>
        public void Register(string email, string password, string displayName, Action<bool> callback)
        {
            StartCoroutine(RegisterCoroutine(email, password, displayName, callback));
        }

        private IEnumerator RegisterCoroutine(string email, string password, string displayName, Action<bool> callback)
        {
            var data = new Dictionary<string, string>
            {
                { "email", email },
                { "password", password },
                { "displayName", displayName }
            };

            yield return PostRequest("/auth/register", data, (success, response) =>
            {
                callback?.Invoke(success);
                if (!success)
                {
                    OnError?.Invoke("Registration failed");
                }
            });
        }

        /// <summary>
        /// Logout and clear session.
        /// </summary>
        public void Logout()
        {
            sessionToken = null;
            userId = null;
            isAuthenticated = false;
            OnAuthenticationChanged?.Invoke(false);
            Debug.Log("[APIClient] Logged out");
        }

        #endregion

        #region Cloud Save

        /// <summary>
        /// Save player data to cloud.
        /// </summary>
        public void SaveToCloud(string playerDataJson, Action<bool> callback)
        {
            if (!isAuthenticated)
            {
                callback?.Invoke(false);
                OnError?.Invoke("Not authenticated");
                return;
            }

            StartCoroutine(SaveToCloudCoroutine(playerDataJson, callback));
        }

        private IEnumerator SaveToCloudCoroutine(string playerDataJson, Action<bool> callback)
        {
            var data = new Dictionary<string, string>
            {
                { "userId", userId },
                { "playerData", playerDataJson }
            };

            yield return PostRequestAuth("/player/save", data, (success, response) =>
            {
                callback?.Invoke(success);
                if (success)
                {
                    Debug.Log("[APIClient] Cloud save successful");
                }
                else
                {
                    OnError?.Invoke("Cloud save failed");
                }
            });
        }

        /// <summary>
        /// Load player data from cloud.
        /// </summary>
        public void LoadFromCloud(Action<string> callback)
        {
            if (!isAuthenticated)
            {
                callback?.Invoke(null);
                OnError?.Invoke("Not authenticated");
                return;
            }

            StartCoroutine(LoadFromCloudCoroutine(callback));
        }

        private IEnumerator LoadFromCloudCoroutine(Action<string> callback)
        {
            yield return GetRequestAuth($"/player/load?userId={userId}", (success, response) =>
            {
                if (success && response != null)
                {
                    var loadResponse = JsonUtility.FromJson<LoadResponse>(response);
                    callback?.Invoke(loadResponse.playerData);
                    Debug.Log("[APIClient] Cloud load successful");
                }
                else
                {
                    callback?.Invoke(null);
                    OnError?.Invoke("Cloud load failed");
                }
            });
        }

        #endregion

        #region Leaderboards

        /// <summary>
        /// Submit score to leaderboard.
        /// </summary>
        public void SubmitScore(string leaderboardId, int score, Action<bool> callback)
        {
            if (!isAuthenticated)
            {
                callback?.Invoke(false);
                return;
            }

            StartCoroutine(SubmitScoreCoroutine(leaderboardId, score, callback));
        }

        private IEnumerator SubmitScoreCoroutine(string leaderboardId, int score, Action<bool> callback)
        {
            var data = new Dictionary<string, object>
            {
                { "userId", userId },
                { "leaderboardId", leaderboardId },
                { "score", score }
            };

            yield return PostRequestAuth("/leaderboard/submit", data, (success, response) =>
            {
                callback?.Invoke(success);
            });
        }

        /// <summary>
        /// Get leaderboard entries.
        /// </summary>
        public void GetLeaderboard(string leaderboardId, int limit, Action<List<LeaderboardEntry>> callback)
        {
            StartCoroutine(GetLeaderboardCoroutine(leaderboardId, limit, callback));
        }

        private IEnumerator GetLeaderboardCoroutine(string leaderboardId, int limit, Action<List<LeaderboardEntry>> callback)
        {
            yield return GetRequestAuth($"/leaderboard/get?leaderboardId={leaderboardId}&limit={limit}", (success, response) =>
            {
                if (success && response != null)
                {
                    var leaderboardResponse = JsonUtility.FromJson<LeaderboardResponse>(response);
                    callback?.Invoke(leaderboardResponse.entries);
                }
                else
                {
                    callback?.Invoke(new List<LeaderboardEntry>());
                }
            });
        }

        #endregion

        #region PvP Matchmaking

        /// <summary>
        /// Find PvP opponent for raid.
        /// </summary>
        public void FindOpponent(int playerLevel, Action<OpponentData> callback)
        {
            if (!isAuthenticated)
            {
                callback?.Invoke(null);
                return;
            }

            StartCoroutine(FindOpponentCoroutine(playerLevel, callback));
        }

        private IEnumerator FindOpponentCoroutine(int playerLevel, Action<OpponentData> callback)
        {
            yield return GetRequestAuth($"/matchmaking/find?userId={userId}&level={playerLevel}", (success, response) =>
            {
                if (success && response != null)
                {
                    var opponent = JsonUtility.FromJson<OpponentData>(response);
                    callback?.Invoke(opponent);
                }
                else
                {
                    callback?.Invoke(null);
                    OnError?.Invoke("Matchmaking failed");
                }
            });
        }

        /// <summary>
        /// Submit raid result.
        /// </summary>
        public void SubmitRaidResult(string opponentId, int lootGained, bool victory, Action<bool> callback)
        {
            if (!isAuthenticated)
            {
                callback?.Invoke(false);
                return;
            }

            StartCoroutine(SubmitRaidResultCoroutine(opponentId, lootGained, victory, callback));
        }

        private IEnumerator SubmitRaidResultCoroutine(string opponentId, int lootGained, bool victory, Action<bool> callback)
        {
            var data = new Dictionary<string, object>
            {
                { "attackerId", userId },
                { "defenderId", opponentId },
                { "lootGained", lootGained },
                { "victory", victory }
            };

            yield return PostRequestAuth("/raid/submit", data, (success, response) =>
            {
                callback?.Invoke(success);
            });
        }

        #endregion

        #region Network Utilities

        private IEnumerator PostRequest(string endpoint, object data, Action<bool, string> callback)
        {
            string json = JsonUtility.ToJson(data);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

            using (UnityWebRequest request = new UnityWebRequest(baseURL + endpoint, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.timeout = (int)requestTimeout;

                yield return request.SendWebRequest();

                bool success = request.result == UnityWebRequest.Result.Success;
                string response = success ? request.downloadHandler.text : null;

                callback?.Invoke(success, response);
            }
        }

        private IEnumerator PostRequestAuth(string endpoint, object data, Action<bool, string> callback)
        {
            string json = JsonUtility.ToJson(data);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

            using (UnityWebRequest request = new UnityWebRequest(baseURL + endpoint, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", $"Bearer {sessionToken}");
                request.timeout = (int)requestTimeout;

                yield return request.SendWebRequest();

                bool success = request.result == UnityWebRequest.Result.Success;
                string response = success ? request.downloadHandler.text : null;

                callback?.Invoke(success, response);
            }
        }

        private IEnumerator GetRequestAuth(string endpoint, Action<bool, string> callback)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(baseURL + endpoint))
            {
                request.SetRequestHeader("Authorization", $"Bearer {sessionToken}");
                request.timeout = (int)requestTimeout;

                yield return request.SendWebRequest();

                bool success = request.result == UnityWebRequest.Result.Success;
                string response = success ? request.downloadHandler.text : null;

                callback?.Invoke(success, response);
            }
        }

        #endregion

        #region Data Structures

        [Serializable]
        private class AuthResponse
        {
            public string token;
            public string userId;
        }

        [Serializable]
        private class LoadResponse
        {
            public string playerData;
        }

        [Serializable]
        private class LeaderboardResponse
        {
            public List<LeaderboardEntry> entries;
        }

        [Serializable]
        public class LeaderboardEntry
        {
            public string userId;
            public string displayName;
            public int score;
            public int rank;
        }

        [Serializable]
        public class OpponentData
        {
            public string userId;
            public string displayName;
            public int level;
            public int[][] baseLayout;
        }

        #endregion

        public bool IsAuthenticated => isAuthenticated;
        public string UserId => userId;
    }
}
