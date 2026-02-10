using UnityEngine;

namespace EmpireOfGlass.Data
{
    /// <summary>
    /// Client-side anti-cheat validation system (Var 41).
    /// Validates currency transactions and raid results before applying them.
    /// In production, all critical operations are also validated server-side
    /// via Azure Functions (Var 45).
    /// </summary>
    public class AntiCheatValidator : MonoBehaviour
    {
        public static AntiCheatValidator Instance { get; private set; }

        [Header("Validation Limits")]
        [SerializeField] private int maxGoldPerRaid = 5000;
        [SerializeField] private int maxShardsPerRaid = 100;
        [SerializeField] private int maxGoldPerSession = 50000;
        [SerializeField] private float minRaidDurationSeconds = 5f;
        [SerializeField] private int maxCurrencyPerTransaction = 100000;

        private int sessionGoldEarned;
        private float sessionStartTime;

        public event System.Action<string> OnCheatDetected;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            sessionStartTime = Time.time;
        }

        // ==================== Currency Validation ====================

        /// <summary>
        /// Validate a gold transaction before applying it.
        /// Returns true if the transaction is legitimate.
        /// </summary>
        public bool ValidateGoldTransaction(int amount, string source)
        {
            if (amount < 0)
            {
                ReportCheat($"Negative gold transaction: {amount} from {source}");
                return false;
            }

            if (amount > maxCurrencyPerTransaction)
            {
                ReportCheat($"Gold exceeds max per transaction: {amount} from {source}");
                return false;
            }

            sessionGoldEarned += amount;
            if (sessionGoldEarned > maxGoldPerSession)
            {
                ReportCheat($"Session gold cap exceeded: {sessionGoldEarned} from {source}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validate a gem transaction before applying it.
        /// </summary>
        public bool ValidateGemTransaction(int amount, string source)
        {
            if (amount < 0)
            {
                ReportCheat($"Negative gem transaction: {amount} from {source}");
                return false;
            }

            if (amount > maxCurrencyPerTransaction)
            {
                ReportCheat($"Gems exceed max per transaction: {amount} from {source}");
                return false;
            }

            return true;
        }

        // ==================== Raid Validation ====================

        /// <summary>
        /// Validate raid results are within expected bounds (Var 41).
        /// In production, this check runs server-side via Azure Functions.
        /// </summary>
        public bool ValidateRaidResult(Raid.RaidResult result, float raidDurationSeconds)
        {
            if (raidDurationSeconds < minRaidDurationSeconds)
            {
                ReportCheat($"Raid completed too quickly: {raidDurationSeconds:F1}s (min {minRaidDurationSeconds}s)");
                return false;
            }

            if (result.Gold > maxGoldPerRaid)
            {
                ReportCheat($"Raid gold exceeds max: {result.Gold} (max {maxGoldPerRaid})");
                return false;
            }

            if (result.Shards > maxShardsPerRaid)
            {
                ReportCheat($"Raid shards exceeds max: {result.Shards} (max {maxShardsPerRaid})");
                return false;
            }

            if (result.Precision < 0f || result.Precision > 1f)
            {
                ReportCheat($"Invalid raid precision: {result.Precision}");
                return false;
            }

            if (result.LootTier < 0 || result.LootTier > 5)
            {
                ReportCheat($"Invalid loot tier: {result.LootTier}");
                return false;
            }

            return true;
        }

        // ==================== Swarm Validation ====================

        /// <summary>
        /// Validate swarm count is within expected bounds.
        /// </summary>
        public bool ValidateSwarmCount(int count, int maxAllowed)
        {
            if (count < 0 || count > maxAllowed)
            {
                ReportCheat($"Invalid swarm count: {count} (max {maxAllowed})");
                return false;
            }
            return true;
        }

        // ==================== Server Validation Hook ====================

        /// <summary>
        /// Submit a transaction to the server for authoritative validation (Var 41, 45).
        /// In production: sends to Azure Function for server-side check.
        /// Returns true if server approves (always true in prototype).
        /// </summary>
        public bool ServerValidate(string transactionType, string payload)
        {
            // In production: POST to Azure Function endpoint
            // var response = await httpClient.PostAsync(azureEndpoint, payload);
            // return response.IsValid;
            Debug.Log($"[AntiCheatValidator] Server validation: {transactionType} ({payload.Length} bytes)");
            return true;
        }

        private void ReportCheat(string details)
        {
            Debug.LogWarning($"[AntiCheatValidator] CHEAT DETECTED: {details}");
            OnCheatDetected?.Invoke(details);

            // In production: send report to backend for review/ban
        }

        /// <summary>
        /// Reset session tracking (call on new session start).
        /// </summary>
        public void ResetSession()
        {
            sessionGoldEarned = 0;
            sessionStartTime = Time.time;
        }
    }
}
