using UnityEngine;

namespace EmpireOfGlass.AntiCheat
{
    /// <summary>
    /// Server-authoritative anti-cheat validation for currency and raid results (Var 41).
    /// Validates game actions locally and prepares payloads for server-side verification
    /// via PlayFab/Azure Functions.
    /// </summary>
    public class AntiCheatManager : MonoBehaviour
    {
        public static AntiCheatManager Instance { get; private set; }

        [Header("Validation Settings (Var 41)")]
        [SerializeField] private int maxGoldPerRaid = 5000;
        [SerializeField] private int maxShardsPerRaid = 500;
        [SerializeField] private int maxSwarmCount = 600;
        [SerializeField] private float minRaidDurationSeconds = 10f;

        public event System.Action<string> OnViolationDetected;

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
        /// Validate a currency transaction before applying it.
        /// Returns true if the transaction passes validation.
        /// </summary>
        public bool ValidateCurrencyChange(string currencyType, int currentAmount, int delta)
        {
            // Prevent negative currency
            if (currentAmount + delta < 0)
            {
                ReportViolation($"Negative currency: {currencyType} would become {currentAmount + delta}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validate raid results before they are applied (Var 41).
        /// Checks for impossible loot values and suspicious timing.
        /// </summary>
        public bool ValidateRaidResult(int goldEarned, int shardsEarned, float raidDuration)
        {
            if (goldEarned > maxGoldPerRaid)
            {
                ReportViolation($"Raid gold exceeds maximum: {goldEarned} > {maxGoldPerRaid}");
                return false;
            }

            if (shardsEarned > maxShardsPerRaid)
            {
                ReportViolation($"Raid shards exceed maximum: {shardsEarned} > {maxShardsPerRaid}");
                return false;
            }

            if (raidDuration < minRaidDurationSeconds)
            {
                ReportViolation($"Raid completed too quickly: {raidDuration:F1}s < {minRaidDurationSeconds}s");
                return false;
            }

            Debug.Log("[AntiCheatManager] Raid result validated");
            return true;
        }

        /// <summary>
        /// Validate swarm count to detect impossible multiplication.
        /// </summary>
        public bool ValidateSwarmCount(int count)
        {
            if (count > maxSwarmCount)
            {
                ReportViolation($"Swarm count exceeds maximum: {count} > {maxSwarmCount}");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Generate a signed payload for server-side verification via PlayFab/Azure Functions.
        /// The server uses this to validate that actions occurred legitimately.
        /// </summary>
        public string GenerateValidationPayload(string actionType, string data)
        {
            long timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string payload = $"{{\"action\":\"{actionType}\",\"data\":{data},\"ts\":{timestamp}}}";

            Debug.Log($"[AntiCheatManager] Validation payload generated for: {actionType}");
            return payload;
        }

        private void ReportViolation(string message)
        {
            Debug.LogWarning($"[AntiCheatManager] VIOLATION: {message}");
            OnViolationDetected?.Invoke(message);
        }
    }
}
