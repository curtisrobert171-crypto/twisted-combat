using System.Collections.Generic;
using UnityEngine;

namespace EmpireOfGlass.Social
{
    /// <summary>
    /// Manages social competition and alliance collaboration (Vars 18–19).
    /// Handles friend-attack notifications, 2x revenge loot, and alliance mega-structures.
    /// </summary>
    public class SocialManager : MonoBehaviour
    {
        public static SocialManager Instance { get; private set; }

        [Header("Social Competition (Var 18)")]
        [SerializeField] private float revengeLootMultiplier = 2f;
        [SerializeField] private int maxRevengeQueueSize = 20;

        [Header("Alliance (Var 19)")]
        [SerializeField] private int maxAllianceMembers = 30;
        [SerializeField] private int megaStructurePiecesRequired = 10;

        private readonly List<AttackRecord> revengeQueue = new List<AttackRecord>();
        private string currentAllianceId;
        private readonly List<string> allianceMembers = new List<string>();

        public IReadOnlyList<AttackRecord> RevengeQueue => revengeQueue;
        public string CurrentAllianceId => currentAllianceId;
        public IReadOnlyList<string> AllianceMembers => allianceMembers;

        public event System.Action<AttackRecord> OnAttackReceived;
        public event System.Action<string> OnAllianceJoined;
        public event System.Action OnAllianceLeft;
        public event System.Action<string> OnMegaStructureCompleted;

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
        /// Record an incoming attack from another player and queue for revenge (Var 18).
        /// </summary>
        public void RecordIncomingAttack(string attackerId, string attackerName, int lootStolen)
        {
            var record = new AttackRecord
            {
                AttackerId = attackerId,
                AttackerName = attackerName,
                LootStolen = lootStolen,
                Timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                RevengeAvailable = true
            };

            revengeQueue.Add(record);
            if (revengeQueue.Count > maxRevengeQueueSize)
            {
                revengeQueue.RemoveAt(0);
            }

            Debug.Log($"[SocialManager] Attack received from {attackerName}! Lost {lootStolen} gold. Revenge available.");
            OnAttackReceived?.Invoke(record);
        }

        /// <summary>
        /// Launch a revenge raid against a player who attacked you.
        /// Returns the revenge loot multiplier (2x — Var 18).
        /// </summary>
        public float StartRevengeRaid(string attackerId)
        {
            var record = revengeQueue.Find(r => r.AttackerId == attackerId && r.RevengeAvailable);
            if (record == null)
            {
                Debug.Log("[SocialManager] No revenge available for this player");
                return 1f;
            }

            record.RevengeAvailable = false;
            Debug.Log($"[SocialManager] Revenge raid started against {record.AttackerName}! {revengeLootMultiplier}x loot");
            return revengeLootMultiplier;
        }

        /// <summary>
        /// Join an alliance to collaborate on mega-structures (Var 19).
        /// </summary>
        public bool JoinAlliance(string allianceId)
        {
            if (!string.IsNullOrEmpty(currentAllianceId))
            {
                Debug.Log("[SocialManager] Already in an alliance. Leave first.");
                return false;
            }

            currentAllianceId = allianceId;
            Debug.Log($"[SocialManager] Joined alliance: {allianceId}");
            OnAllianceJoined?.Invoke(allianceId);
            return true;
        }

        /// <summary>
        /// Leave the current alliance.
        /// </summary>
        public void LeaveAlliance()
        {
            if (string.IsNullOrEmpty(currentAllianceId)) return;

            Debug.Log($"[SocialManager] Left alliance: {currentAllianceId}");
            currentAllianceId = null;
            allianceMembers.Clear();
            OnAllianceLeft?.Invoke();
        }

        /// <summary>
        /// Contribute a base piece to an alliance mega-structure (Var 19).
        /// </summary>
        public void ContributeMegaStructurePiece(string structureId)
        {
            Debug.Log($"[SocialManager] Contributed piece to mega-structure: {structureId}");
        }

        public float GetRevengeLootMultiplier() => revengeLootMultiplier;
        public int GetMaxAllianceMembers() => maxAllianceMembers;
    }

    /// <summary>
    /// Record of a player attack for the revenge system.
    /// </summary>
    [System.Serializable]
    public class AttackRecord
    {
        public string AttackerId;
        public string AttackerName;
        public int LootStolen;
        public long Timestamp;
        public bool RevengeAvailable;
    }
}
