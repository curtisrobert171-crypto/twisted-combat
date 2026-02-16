using NUnit.Framework;
using UnityEngine;
using EmpireOfGlass.Data;

namespace EmpireOfGlass.Tests.EditMode
{
    /// <summary>
    /// Unit tests for ProgressionManager.
    /// Tests leveling, XP gain, skill unlocking, and achievements.
    /// </summary>
    public class ProgressionManagerTests
    {
        private GameObject managerObject;
        private ProgressionManager progressionManager;

        [SetUp]
        public void SetUp()
        {
            // Create SaveManager (required dependency)
            var saveManagerObj = new GameObject("SaveManager");
            var saveManager = saveManagerObj.AddComponent<SaveManager>();
            saveManager.LoadPlayerData();

            // Create ProgressionManager
            managerObject = new GameObject("ProgressionManager");
            progressionManager = managerObject.AddComponent<ProgressionManager>();
            progressionManager.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            if (managerObject != null)
            {
                Object.DestroyImmediate(managerObject);
            }

            // Clean up SaveManager
            var saveManager = SaveManager.Instance;
            if (saveManager != null)
            {
                Object.DestroyImmediate(saveManager.gameObject);
            }
        }

        [Test]
        public void AddXP_IncreasesCurrentXP()
        {
            // Arrange
            int initialXP = progressionManager.GetProgressionData().CurrentXP;

            // Act
            progressionManager.AddXP(50);

            // Assert
            int finalXP = progressionManager.GetProgressionData().CurrentXP;
            Assert.AreEqual(initialXP + 50, finalXP, "XP should increase by 50");
        }

        [Test]
        public void AddXP_TriggersLevelUp_WhenEnoughXP()
        {
            // Arrange
            int initialLevel = progressionManager.GetProgressionData().Level;
            bool levelUpTriggered = false;
            progressionManager.OnLevelUp += (level) => levelUpTriggered = true;

            int xpNeeded = progressionManager.GetXPForLevel(initialLevel);

            // Act
            progressionManager.AddXP(xpNeeded);

            // Assert
            Assert.IsTrue(levelUpTriggered, "Level up event should be triggered");
            Assert.AreEqual(initialLevel + 1, progressionManager.GetProgressionData().Level, "Level should increase by 1");
        }

        [Test]
        public void LevelUp_GrantsSkillPoints()
        {
            // Arrange
            int initialSkillPoints = progressionManager.GetProgressionData().SkillPoints;
            int xpForLevel = progressionManager.GetXPForLevel(progressionManager.GetProgressionData().Level);

            // Act
            progressionManager.AddXP(xpForLevel);

            // Assert
            int finalSkillPoints = progressionManager.GetProgressionData().SkillPoints;
            Assert.Greater(finalSkillPoints, initialSkillPoints, "Skill points should increase after level up");
        }

        [Test]
        public void UnlockSkill_SucceedsWithEnoughPoints()
        {
            // Arrange
            progressionManager.GetProgressionData().SkillPoints = 5;
            string skillId = "health_boost_1";

            // Act
            bool success = progressionManager.UnlockSkill(skillId);

            // Assert
            Assert.IsTrue(success, "Skill unlock should succeed with enough points");
            Assert.IsTrue(progressionManager.IsSkillUnlocked(skillId), "Skill should be marked as unlocked");
        }

        [Test]
        public void UnlockSkill_FailsWithoutEnoughPoints()
        {
            // Arrange
            progressionManager.GetProgressionData().SkillPoints = 0;
            string skillId = "health_boost_1";

            // Act
            bool success = progressionManager.UnlockSkill(skillId);

            // Assert
            Assert.IsFalse(success, "Skill unlock should fail without enough points");
            Assert.IsFalse(progressionManager.IsSkillUnlocked(skillId), "Skill should not be unlocked");
        }

        [Test]
        public void UnlockSkill_FailsWithoutPrerequisites()
        {
            // Arrange
            progressionManager.GetProgressionData().SkillPoints = 10;
            string skillId = "health_boost_2"; // Requires health_boost_1

            // Act
            bool success = progressionManager.UnlockSkill(skillId);

            // Assert
            Assert.IsFalse(success, "Skill unlock should fail without prerequisites");
        }

        [Test]
        public void UnlockSkill_SucceedsWithPrerequisites()
        {
            // Arrange
            progressionManager.GetProgressionData().SkillPoints = 10;
            progressionManager.UnlockSkill("health_boost_1"); // Unlock prerequisite

            // Act
            bool success = progressionManager.UnlockSkill("health_boost_2");

            // Assert
            Assert.IsTrue(success, "Skill unlock should succeed with prerequisites met");
        }

        [Test]
        public void GetXPForLevel_IncreasesWithLevel()
        {
            // Act
            int xpLevel1 = progressionManager.GetXPForLevel(1);
            int xpLevel5 = progressionManager.GetXPForLevel(5);
            int xpLevel10 = progressionManager.GetXPForLevel(10);

            // Assert
            Assert.Greater(xpLevel5, xpLevel1, "XP required should increase with level");
            Assert.Greater(xpLevel10, xpLevel5, "XP required should increase with level");
        }

        [Test]
        public void GetLevelProgress_ReturnsCorrectPercentage()
        {
            // Arrange
            var progression = progressionManager.GetProgressionData();
            int xpForLevel = progressionManager.GetXPForLevel(progression.Level);
            progression.CurrentXP = xpForLevel / 2; // 50% progress

            // Act
            float progress = progressionManager.GetLevelProgress();

            // Assert
            Assert.AreEqual(0.5f, progress, 0.01f, "Level progress should be 50%");
        }

        [Test]
        public void CheckAchievements_UnlocksWhenConditionMet()
        {
            // Arrange
            bool achievementUnlocked = false;
            progressionManager.OnAchievementUnlocked += (id) => achievementUnlocked = true;

            // Level up to 5 to unlock "first_steps" achievement
            while (progressionManager.GetProgressionData().Level < 5)
            {
                int xp = progressionManager.GetXPForLevel(progressionManager.GetProgressionData().Level);
                progressionManager.AddXP(xp);
            }

            // Act
            progressionManager.CheckAchievements();

            // Assert
            Assert.IsTrue(achievementUnlocked, "Achievement should be unlocked when condition is met");
        }

        [Test]
        public void SkillTree_HasValidStructure()
        {
            // Act
            var skillTree = progressionManager.GetSkillTree();

            // Assert
            Assert.IsNotNull(skillTree, "Skill tree should not be null");
            Assert.Greater(skillTree.Count, 0, "Skill tree should have skills");

            // Verify each skill has valid data
            foreach (var kvp in skillTree)
            {
                Assert.IsNotNull(kvp.Value.Name, $"Skill {kvp.Key} should have a name");
                Assert.IsNotNull(kvp.Value.Description, $"Skill {kvp.Key} should have a description");
                Assert.Greater(kvp.Value.Cost, 0, $"Skill {kvp.Key} should have a positive cost");
            }
        }

        [Test]
        public void Achievements_HasValidStructure()
        {
            // Act
            var achievements = progressionManager.GetAchievements();

            // Assert
            Assert.IsNotNull(achievements, "Achievements should not be null");
            Assert.Greater(achievements.Count, 0, "Should have achievements");

            // Verify each achievement has valid data
            foreach (var kvp in achievements)
            {
                Assert.IsNotNull(kvp.Value.Name, $"Achievement {kvp.Key} should have a name");
                Assert.IsNotNull(kvp.Value.Description, $"Achievement {kvp.Key} should have a description");
                Assert.Greater(kvp.Value.RequiredValue, 0, $"Achievement {kvp.Key} should have a positive requirement");
            }
        }
    }
}
