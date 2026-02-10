using UnityEngine;

namespace EmpireOfGlass.Data
{
    /// <summary>
    /// Cloud save integration service for cross-platform progression (Var 40).
    /// Handles OAuth 2.0 login flows and sync with PlayFab/Firebase backend (Var 45).
    /// In production, replace stub methods with actual SDK calls.
    /// </summary>
    public class CloudSaveService : MonoBehaviour
    {
        public static CloudSaveService Instance { get; private set; }

        public enum AuthState
        {
            NotAuthenticated,
            Authenticating,
            Authenticated,
            Failed
        }

        public enum AuthProvider
        {
            Guest,
            Google,
            Apple,
            Facebook
        }

        [Header("Cloud Save (Var 40)")]
        [SerializeField] private bool autoSyncOnLogin = true;
        [SerializeField] private float autoSaveIntervalSeconds = 300f; // 5 minutes

        private AuthState currentAuthState = AuthState.NotAuthenticated;
        private AuthProvider currentProvider = AuthProvider.Guest;
        private float autoSaveTimer;

        public AuthState CurrentAuthState => currentAuthState;
        public AuthProvider CurrentProvider => currentProvider;
        public bool IsAuthenticated => currentAuthState == AuthState.Authenticated;

        public event System.Action<AuthState> OnAuthStateChanged;
        public event System.Action<bool> OnCloudSyncComplete;

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
            if (IsAuthenticated)
            {
                autoSaveTimer += Time.deltaTime;
                if (autoSaveTimer >= autoSaveIntervalSeconds)
                {
                    autoSaveTimer = 0f;
                    SyncToCloud();
                }
            }
        }

        // ==================== Authentication (Var 40: OAuth 2.0) ====================

        /// <summary>
        /// Begin OAuth 2.0 login flow with the specified provider.
        /// In production: calls PlayFab LoginWithGoogleAccount / LoginWithApple / etc.
        /// </summary>
        public void Login(AuthProvider provider)
        {
            currentProvider = provider;
            SetAuthState(AuthState.Authenticating);

            // In production: initiate OAuth 2.0 flow via PlayFab SDK
            // PlayFabClientAPI.LoginWithGoogleAccount(...)
            Debug.Log($"[CloudSaveService] OAuth 2.0 login initiated: {provider}");

            // Simulate successful auth for prototype
            SetAuthState(AuthState.Authenticated);

            if (autoSyncOnLogin)
            {
                SyncFromCloud();
            }
        }

        /// <summary>
        /// Login as guest (no cross-platform sync).
        /// </summary>
        public void LoginAsGuest()
        {
            Login(AuthProvider.Guest);
        }

        /// <summary>
        /// Logout and clear cloud session.
        /// </summary>
        public void Logout()
        {
            SetAuthState(AuthState.NotAuthenticated);
            currentProvider = AuthProvider.Guest;
            Debug.Log("[CloudSaveService] Logged out");
        }

        // ==================== Cloud Sync ====================

        /// <summary>
        /// Push local save data to the cloud (Var 40, 45).
        /// In production: calls PlayFab UpdateUserData.
        /// </summary>
        public void SyncToCloud()
        {
            if (!IsAuthenticated)
            {
                Debug.Log("[CloudSaveService] Cannot sync: not authenticated");
                return;
            }

            var player = SaveManager.Instance?.CurrentPlayer;
            if (player == null) return;

            string json = player.ToJson();
            // In production: PlayFabClientAPI.UpdateUserData(json)
            Debug.Log($"[CloudSaveService] Synced to cloud ({json.Length} bytes)");
            OnCloudSyncComplete?.Invoke(true);
        }

        /// <summary>
        /// Pull cloud save data and merge with local (Var 40).
        /// Uses last-write-wins conflict resolution.
        /// </summary>
        public void SyncFromCloud()
        {
            if (!IsAuthenticated)
            {
                Debug.Log("[CloudSaveService] Cannot sync: not authenticated");
                return;
            }

            // In production: PlayFabClientAPI.GetUserData(...)
            // Then compare timestamps and merge
            Debug.Log("[CloudSaveService] Synced from cloud (local data used in prototype)");
            OnCloudSyncComplete?.Invoke(true);
        }

        private void SetAuthState(AuthState state)
        {
            currentAuthState = state;
            OnAuthStateChanged?.Invoke(state);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && IsAuthenticated)
            {
                SyncToCloud();
            }
        }
    }
}
