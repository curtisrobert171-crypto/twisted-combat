using System;
using System.Collections.Generic;
using UnityEngine;

namespace EmpireOfGlass.Data
{
    /// <summary>
    /// Manages player progression, leveling, skill tree, and unlock systems.
    /// Integrates with PlayerData for persistent save/load.
    /// </summary>
    public class ProgressionManager : MonoBehaviour
    {
        public static ProgressionManager Instance { get; private set; }

        [Header("Leveling Configuration")]
        [SerializeField] private int baseXPRequired = 100;
        [SerializeField] private float xpScalingFactor = 1.5f;
        [SerializeField] private int maxLevel = 100;

        [Header("Rewards")]
        [SerializeField] private int goldPerLevel = 100;
        [SerializeField] private int gemsPerLevel = 5;

        // Events
        public event Action<int> OnLevelUp;
        public event Action<string> OnSkillUnlocked;
        public event Action<string> OnAchievementUnlocked;
        public event Action<int> OnXPGained;

        private PlayerProgressionData progression;
        private Dictionary<string, SkillNode> skillTree;
        private Dictionary<string, Achievement> achievements;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeSkillTree();
            InitializeAchievements();
        }

        /// <summary>
        /// Initialize progression data from SaveManager.
        /// </summary>
        public void Initialize()
        {
            var playerData = SaveManager.Instance?.CurrentPlayer;
            if (playerData != null)
            {
                progression = new PlayerProgressionData
                {
                    CurrentXP = 0,
                    Level = playerData.Level,
                    SkillPoints = 0,
                    UnlockedSkills = new List<string>(),
                    UnlockedAchievements = new List<string>()
                };
            }
            else
            {
                progression = PlayerProgressionData.CreateNew();
            }

            Debug.Log($"[ProgressionManager] Initialized - Level {progression.Level}");
        }

        /// <summary>
        /// Add experience points and handle level ups.
        /// </summary>
        public void AddXP(int amount)
        {
            progression.CurrentXP += amount;
            OnXPGained?.Invoke(amount);

            // Check for level up
            int xpNeeded = GetXPForLevel(progression.Level);
            while (progression.CurrentXP >= xpNeeded && progression.Level < maxLevel)
            {
                progression.CurrentXP -= xpNeeded;
                progression.Level++;
                progression.SkillPoints++;

                HandleLevelUp();
                xpNeeded = GetXPForLevel(progression.Level);
            }

            // Track analytics
            if (Analytics.AnalyticsManager.Instance != null)
            {
                Analytics.AnalyticsManager.Instance.TrackEvent("XP_Gained", new Dictionary<string, object>
                {
                    { "amount", amount },
                    { "level", progression.Level },
                    { "total_xp", progression.CurrentXP }
                });
            }
        }

        /// <summary>
        /// Handle level up rewards and notifications.
        /// </summary>
        private void HandleLevelUp()
        {
            var playerData = SaveManager.Instance?.CurrentPlayer;
            if (playerData != null)
            {
                playerData.Level = progression.Level;
                playerData.Gold += goldPerLevel;
                playerData.PremiumGems += gemsPerLevel;
            }

            OnLevelUp?.Invoke(progression.Level);

            // Track analytics
            if (Analytics.AnalyticsManager.Instance != null)
            {
                Analytics.AnalyticsManager.Instance.TrackEvent("Level_Up", new Dictionary<string, object>
                {
                    { "new_level", progression.Level },
                    { "gold_reward", goldPerLevel },
                    { "gem_reward", gemsPerLevel }
                });
            }

            Debug.Log($"[ProgressionManager] Level Up! Now Level {progression.Level}");
        }

        /// <summary>
        /// Calculate XP required for a given level.
        /// </summary>
        public int GetXPForLevel(int level)
        {
            return Mathf.FloorToInt(baseXPRequired * Mathf.Pow(xpScalingFactor, level - 1));
        }

        /// <summary>
        /// Get progress to next level as a percentage (0-1).
        /// </summary>
        public float GetLevelProgress()
        {
            if (progression.Level >= maxLevel) return 1f;
            int xpNeeded = GetXPForLevel(progression.Level);
            return Mathf.Clamp01((float)progression.CurrentXP / xpNeeded);
        }

        /// <summary>
        /// Unlock a skill in the skill tree.
        /// </summary>
        public bool UnlockSkill(string skillId)
        {
            if (!skillTree.ContainsKey(skillId))
            {
                Debug.LogWarning($"[ProgressionManager] Skill not found: {skillId}");
                return false;
            }

            var skill = skillTree[skillId];

            // Check if already unlocked
            if (progression.UnlockedSkills.Contains(skillId))
            {
                Debug.LogWarning($"[ProgressionManager] Skill already unlocked: {skillId}");
                return false;
            }

            // Check prerequisites
            foreach (var prereq in skill.Prerequisites)
            {
                if (!progression.UnlockedSkills.Contains(prereq))
                {
                    Debug.LogWarning($"[ProgressionManager] Prerequisite not met: {prereq}");
                    return false;
                }
            }

            // Check skill points
            if (progression.SkillPoints < skill.Cost)
            {
                Debug.LogWarning($"[ProgressionManager] Not enough skill points. Need {skill.Cost}, have {progression.SkillPoints}");
                return false;
            }

            // Unlock skill
            progression.SkillPoints -= skill.Cost;
            progression.UnlockedSkills.Add(skillId);
            OnSkillUnlocked?.Invoke(skillId);

            // Track analytics
            if (Analytics.AnalyticsManager.Instance != null)
            {
                Analytics.AnalyticsManager.Instance.TrackEvent("Skill_Unlocked", new Dictionary<string, object>
                {
                    { "skill_id", skillId },
                    { "skill_name", skill.Name },
                    { "cost", skill.Cost },
                    { "level", progression.Level }
                });
            }

            Debug.Log($"[ProgressionManager] Unlocked skill: {skill.Name}");
            return true;
        }

        /// <summary>
        /// Check if a skill is unlocked.
        /// </summary>
        public bool IsSkillUnlocked(string skillId)
        {
            return progression.UnlockedSkills.Contains(skillId);
        }

        /// <summary>
        /// Unlock an achievement.
        /// </summary>
        public void UnlockAchievement(string achievementId)
        {
            if (!achievements.ContainsKey(achievementId))
            {
                Debug.LogWarning($"[ProgressionManager] Achievement not found: {achievementId}");
                return;
            }

            if (progression.UnlockedAchievements.Contains(achievementId))
            {
                return; // Already unlocked
            }

            var achievement = achievements[achievementId];
            progression.UnlockedAchievements.Add(achievementId);

            // Grant rewards
            var playerData = SaveManager.Instance?.CurrentPlayer;
            if (playerData != null)
            {
                playerData.Gold += achievement.GoldReward;
                playerData.PremiumGems += achievement.GemReward;
            }

            OnAchievementUnlocked?.Invoke(achievementId);

            // Track analytics
            if (Analytics.AnalyticsManager.Instance != null)
            {
                Analytics.AnalyticsManager.Instance.TrackEvent("Achievement_Unlocked", new Dictionary<string, object>
                {
                    { "achievement_id", achievementId },
                    { "achievement_name", achievement.Name },
                    { "gold_reward", achievement.GoldReward },
                    { "gem_reward", achievement.GemReward }
                });
            }

            Debug.Log($"[ProgressionManager] Unlocked achievement: {achievement.Name}");
        }

        /// <summary>
        /// Check achievement progress based on game metrics.
        /// </summary>
        public void CheckAchievements()
        {
            var playerData = SaveManager.Instance?.CurrentPlayer;
            if (playerData == null) return;

            // Check each achievement
            foreach (var kvp in achievements)
            {
                string id = kvp.Key;
                var achievement = kvp.Value;

                if (progression.UnlockedAchievements.Contains(id)) continue;

                bool unlocked = achievement.Condition switch
                {
                    AchievementCondition.ReachLevel => progression.Level >= achievement.RequiredValue,
                    AchievementCondition.CompleteRaids => playerData.RaidsCompleted >= achievement.RequiredValue,
                    AchievementCondition.SwarmHighScore => playerData.SwarmHighScore >= achievement.RequiredValue,
                    AchievementCondition.CityLevel => playerData.CitySizeLevel >= achievement.RequiredValue,
                    AchievementCondition.CollectGold => playerData.Gold >= achievement.RequiredValue,
                    _ => false
                };

                if (unlocked)
                {
                    UnlockAchievement(id);
                }
            }
        }

        /// <summary>
        /// Initialize the skill tree structure.
        /// </summary>
        private void InitializeSkillTree()
        {
            skillTree = new Dictionary<string, SkillNode>
            {
                // Core Skills
                { "health_boost_1", new SkillNode { Name = "Health Boost I", Description = "Increase hero max health by 20%", Cost = 1, Prerequisites = new List<string>() } },
                { "health_boost_2", new SkillNode { Name = "Health Boost II", Description = "Increase hero max health by 40%", Cost = 2, Prerequisites = new List<string> { "health_boost_1" } } },
                { "damage_boost_1", new SkillNode { Name = "Damage Boost I", Description = "Increase hero damage by 15%", Cost = 1, Prerequisites = new List<string>() } },
                { "damage_boost_2", new SkillNode { Name = "Damage Boost II", Description = "Increase hero damage by 30%", Cost = 2, Prerequisites = new List<string> { "damage_boost_1" } } },
                
                // Swarm Skills
                { "swarm_multiplier", new SkillNode { Name = "Swarm Multiplier", Description = "Math gates give +10% bonus", Cost = 2, Prerequisites = new List<string>() } },
                { "energy_efficiency", new SkillNode { Name = "Energy Efficiency", Description = "Swarm energy cost reduced by 20%", Cost = 2, Prerequisites = new List<string>() } },
                
                // Raid Skills
                { "raid_damage", new SkillNode { Name = "Raid Power", Description = "Increase raid damage by 25%", Cost = 2, Prerequisites = new List<string>() } },
                { "loot_bonus", new SkillNode { Name = "Loot Bonus", Description = "Increase raid rewards by 20%", Cost = 2, Prerequisites = new List<string> { "raid_damage" } } },
                
                // City Skills
                { "build_speed", new SkillNode { Name = "Construction Speed", Description = "Buildings complete 30% faster", Cost = 2, Prerequisites = new List<string>() } },
                { "gold_generation", new SkillNode { Name = "Gold Generation", Description = "City generates 25% more gold", Cost = 2, Prerequisites = new List<string> { "build_speed" } } }
            };
        }

        /// <summary>
        /// Initialize achievements.
        /// </summary>
        private void InitializeAchievements()
        {
            achievements = new Dictionary<string, Achievement>
            {
                { "first_steps", new Achievement { Name = "First Steps", Description = "Reach Level 5", Condition = AchievementCondition.ReachLevel, RequiredValue = 5, GoldReward = 500, GemReward = 10 } },
                { "experienced", new Achievement { Name = "Experienced", Description = "Reach Level 10", Condition = AchievementCondition.ReachLevel, RequiredValue = 10, GoldReward = 1000, GemReward = 25 } },
                { "veteran", new Achievement { Name = "Veteran", Description = "Reach Level 25", Condition = AchievementCondition.ReachLevel, RequiredValue = 25, GoldReward = 5000, GemReward = 100 } },
                { "raid_master", new Achievement { Name = "Raid Master", Description = "Complete 50 raids", Condition = AchievementCondition.CompleteRaids, RequiredValue = 50, GoldReward = 2000, GemReward = 50 } },
                { "swarm_legend", new Achievement { Name = "Swarm Legend", Description = "Score 10,000 in Swarm mode", Condition = AchievementCondition.SwarmHighScore, RequiredValue = 10000, GoldReward = 3000, GemReward = 75 } },
                { "city_builder", new Achievement { Name = "City Builder", Description = "Build city to level 10", Condition = AchievementCondition.CityLevel, RequiredValue = 10, GoldReward = 2500, GemReward = 60 } }
            };
        }

        /// <summary>
        /// Get all skills in the skill tree.
        /// </summary>
        public Dictionary<string, SkillNode> GetSkillTree() => skillTree;

        /// <summary>
        /// Get all achievements.
        /// </summary>
        public Dictionary<string, Achievement> GetAchievements() => achievements;

        /// <summary>
        /// Get current progression data.
        /// </summary>
        public PlayerProgressionData GetProgressionData() => progression;
    }

    /// <summary>
    /// Player progression data structure.
    /// </summary>
    [Serializable]
    public class PlayerProgressionData
    {
        public int CurrentXP;
        public int Level;
        public int SkillPoints;
        public List<string> UnlockedSkills;
        public List<string> UnlockedAchievements;

        public static PlayerProgressionData CreateNew()
        {
            return new PlayerProgressionData
            {
                CurrentXP = 0,
                Level = 1,
                SkillPoints = 0,
                UnlockedSkills = new List<string>(),
                UnlockedAchievements = new List<string>()
            };
        }
    }

    /// <summary>
    /// Skill node in the skill tree.
    /// </summary>
    [Serializable]
    public class SkillNode
    {
        public string Name;
        public string Description;
        public int Cost;
        public List<string> Prerequisites;
    }

    /// <summary>
    /// Achievement definition.
    /// </summary>
    [Serializable]
    public class Achievement
    {
        public string Name;
        public string Description;
        public AchievementCondition Condition;
        public int RequiredValue;
        public int GoldReward;
        public int GemReward;
    }

    /// <summary>
    /// Achievement unlock conditions.
    /// </summary>
    public enum AchievementCondition
    {
        ReachLevel,
        CompleteRaids,
        SwarmHighScore,
        CityLevel,
        CollectGold
    }
}
