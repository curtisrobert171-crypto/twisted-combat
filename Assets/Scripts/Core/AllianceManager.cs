using System.Collections.Generic;
using UnityEngine;

namespace EmpireOfGlass.Core
{
    /// <summary>
    /// Manages alliance/guild social collaboration features (Var 19).
    /// Handles alliance creation, member management, and mega-structure contributions.
    /// Alliances combine base pieces to build mega-structures.
    /// </summary>
    public class AllianceManager : MonoBehaviour
    {
        public static AllianceManager Instance { get; private set; }

        [Header("Alliance Settings (Var 19)")]
        [SerializeField] private int maxAllianceMembers = 30;
        [SerializeField] private int megaStructureContributionGoal = 10000;
        [SerializeField] private int allianceCreationCost = 500;

        private Alliance currentAlliance;

        public Alliance CurrentAlliance => currentAlliance;
        public bool IsInAlliance => currentAlliance != null;

        public event System.Action<Alliance> OnAllianceJoined;
        public event System.Action OnAllianceLeft;
        public event System.Action<string> OnMemberJoined;
        public event System.Action<string> OnMemberLeft;
        public event System.Action<int> OnMegaStructureProgress;

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
        /// Create a new alliance (costs gold).
        /// </summary>
        public bool CreateAlliance(string allianceName)
        {
            var player = Data.SaveManager.Instance?.CurrentPlayer;
            if (player == null || player.Gold < allianceCreationCost) return false;
            if (IsInAlliance) return false;

            player.Gold -= allianceCreationCost;

            currentAlliance = new Alliance
            {
                AllianceID = System.Guid.NewGuid().ToString(),
                Name = allianceName,
                LeaderID = player.UserID,
                Members = new List<AllianceMember>
                {
                    new AllianceMember
                    {
                        UserID = player.UserID,
                        DisplayName = player.DisplayName,
                        Role = AllianceRole.Leader,
                        ContributionPoints = 0
                    }
                },
                MegaStructureProgress = 0,
                MegaStructureGoal = megaStructureContributionGoal,
                CreatedTimestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            Debug.Log($"[AllianceManager] Alliance '{allianceName}' created by {player.DisplayName}");
            OnAllianceJoined?.Invoke(currentAlliance);
            return true;
        }

        /// <summary>
        /// Join an existing alliance.
        /// In production, fetches alliance data from backend.
        /// </summary>
        public bool JoinAlliance(Alliance alliance)
        {
            var player = Data.SaveManager.Instance?.CurrentPlayer;
            if (player == null || IsInAlliance) return false;
            if (alliance.Members.Count >= maxAllianceMembers) return false;

            alliance.Members.Add(new AllianceMember
            {
                UserID = player.UserID,
                DisplayName = player.DisplayName,
                Role = AllianceRole.Member,
                ContributionPoints = 0
            });

            currentAlliance = alliance;

            Debug.Log($"[AllianceManager] {player.DisplayName} joined '{alliance.Name}'");
            OnAllianceJoined?.Invoke(currentAlliance);
            OnMemberJoined?.Invoke(player.DisplayName);
            return true;
        }

        /// <summary>
        /// Leave the current alliance.
        /// </summary>
        public void LeaveAlliance()
        {
            if (!IsInAlliance) return;

            var player = Data.SaveManager.Instance?.CurrentPlayer;
            if (player != null)
            {
                currentAlliance.Members.RemoveAll(m => m.UserID == player.UserID);
                Debug.Log($"[AllianceManager] {player.DisplayName} left '{currentAlliance.Name}'");
                OnMemberLeft?.Invoke(player.DisplayName);
            }

            currentAlliance = null;
            OnAllianceLeft?.Invoke();
        }

        /// <summary>
        /// Contribute resources toward the alliance mega-structure (Var 19).
        /// </summary>
        public bool ContributeToMegaStructure(int goldAmount)
        {
            var player = Data.SaveManager.Instance?.CurrentPlayer;
            if (player == null || !IsInAlliance || player.Gold < goldAmount) return false;

            player.Gold -= goldAmount;
            currentAlliance.MegaStructureProgress += goldAmount;

            // Track individual contribution
            var member = currentAlliance.Members.Find(m => m.UserID == player.UserID);
            if (member != null)
            {
                member.ContributionPoints += goldAmount;
            }

            Debug.Log($"[AllianceManager] {player.DisplayName} contributed {goldAmount} gold. Progress: {currentAlliance.MegaStructureProgress}/{currentAlliance.MegaStructureGoal}");
            OnMegaStructureProgress?.Invoke(currentAlliance.MegaStructureProgress);

            if (currentAlliance.MegaStructureProgress >= currentAlliance.MegaStructureGoal)
            {
                Debug.Log($"[AllianceManager] Mega-structure complete for '{currentAlliance.Name}'!");
            }

            return true;
        }

        /// <summary>
        /// Get the current mega-structure progress as a percentage (0-1).
        /// </summary>
        public float GetMegaStructureProgress()
        {
            if (!IsInAlliance) return 0f;
            return (float)currentAlliance.MegaStructureProgress / currentAlliance.MegaStructureGoal;
        }

        /// <summary>
        /// Get the alliance leaderboard sorted by contribution points.
        /// </summary>
        public List<AllianceMember> GetContributionLeaderboard()
        {
            if (!IsInAlliance) return new List<AllianceMember>();

            var sorted = new List<AllianceMember>(currentAlliance.Members);
            sorted.Sort((a, b) => b.ContributionPoints.CompareTo(a.ContributionPoints));
            return sorted;
        }
    }

    /// <summary>
    /// Alliance data model.
    /// </summary>
    [System.Serializable]
    public class Alliance
    {
        public string AllianceID;
        public string Name;
        public string LeaderID;
        public List<AllianceMember> Members;
        public int MegaStructureProgress;
        public int MegaStructureGoal;
        public long CreatedTimestamp;
    }

    /// <summary>
    /// Alliance member data.
    /// </summary>
    [System.Serializable]
    public class AllianceMember
    {
        public string UserID;
        public string DisplayName;
        public AllianceRole Role;
        public int ContributionPoints;
    }

    /// <summary>
    /// Alliance member roles.
    /// </summary>
    public enum AllianceRole
    {
        Member,
        Officer,
        Leader
    }
}
