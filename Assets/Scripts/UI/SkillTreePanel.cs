using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EmpireOfGlass.Data;

namespace EmpireOfGlass.UI
{
    /// <summary>
    /// UI panel for displaying and unlocking skills in the skill tree.
    /// </summary>
    public class SkillTreePanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform skillContainer;
        [SerializeField] private GameObject skillNodePrefab;
        [SerializeField] private TextMeshProUGUI skillPointsText;
        [SerializeField] private GameObject skillDetailPanel;
        [SerializeField] private TextMeshProUGUI skillNameText;
        [SerializeField] private TextMeshProUGUI skillDescriptionText;
        [SerializeField] private TextMeshProUGUI skillCostText;
        [SerializeField] private Button unlockButton;

        [Header("Colors")]
        [SerializeField] private Color unlockedColor = Color.green;
        [SerializeField] private Color availableColor = Color.yellow;
        [SerializeField] private Color lockedColor = Color.gray;

        private Dictionary<string, GameObject> skillNodeObjects = new Dictionary<string, GameObject>();
        private string selectedSkillId = null;

        private void Start()
        {
            if (unlockButton != null)
            {
                unlockButton.onClick.AddListener(OnUnlockButtonClicked);
            }

            if (skillDetailPanel != null)
            {
                skillDetailPanel.SetActive(false);
            }

            if (ProgressionManager.Instance != null)
            {
                ProgressionManager.Instance.OnSkillUnlocked += HandleSkillUnlocked;
                InitializeSkillTree();
            }

            UpdateDisplay();
        }

        private void OnDestroy()
        {
            if (ProgressionManager.Instance != null)
            {
                ProgressionManager.Instance.OnSkillUnlocked -= HandleSkillUnlocked;
            }

            if (unlockButton != null)
            {
                unlockButton.onClick.RemoveListener(OnUnlockButtonClicked);
            }
        }

        /// <summary>
        /// Initialize skill tree UI nodes.
        /// </summary>
        private void InitializeSkillTree()
        {
            if (skillNodePrefab == null || skillContainer == null) return;

            var skillTree = ProgressionManager.Instance.GetSkillTree();
            if (skillTree == null) return;

            int index = 0;
            foreach (var kvp in skillTree)
            {
                string skillId = kvp.Key;
                var skill = kvp.Value;

                GameObject nodeObj = Instantiate(skillNodePrefab, skillContainer);
                skillNodeObjects[skillId] = nodeObj;

                // Set node name
                var nameText = nodeObj.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
                if (nameText != null)
                {
                    nameText.text = skill.Name;
                }

                // Set node cost
                var costText = nodeObj.transform.Find("CostText")?.GetComponent<TextMeshProUGUI>();
                if (costText != null)
                {
                    costText.text = $"{skill.Cost} SP";
                }

                // Add click handler
                var button = nodeObj.GetComponent<Button>();
                if (button != null)
                {
                    string capturedId = skillId; // Capture for closure
                    button.onClick.AddListener(() => OnSkillNodeClicked(capturedId));
                }

                index++;
            }

            UpdateSkillNodeStates();
        }

        /// <summary>
        /// Update all skill node visual states.
        /// </summary>
        private void UpdateSkillNodeStates()
        {
            if (ProgressionManager.Instance == null) return;

            var skillTree = ProgressionManager.Instance.GetSkillTree();
            if (skillTree == null) return;

            foreach (var kvp in skillNodeObjects)
            {
                string skillId = kvp.Key;
                GameObject nodeObj = kvp.Value;

                if (!skillTree.ContainsKey(skillId)) continue;

                var skill = skillTree[skillId];
                bool isUnlocked = ProgressionManager.Instance.IsSkillUnlocked(skillId);
                bool isAvailable = CanUnlockSkill(skillId, skill);

                // Update node color
                var nodeImage = nodeObj.GetComponent<Image>();
                if (nodeImage != null)
                {
                    if (isUnlocked)
                    {
                        nodeImage.color = unlockedColor;
                    }
                    else if (isAvailable)
                    {
                        nodeImage.color = availableColor;
                    }
                    else
                    {
                        nodeImage.color = lockedColor;
                    }
                }

                // Update interactability
                var button = nodeObj.GetComponent<Button>();
                if (button != null)
                {
                    button.interactable = !isUnlocked;
                }
            }
        }

        /// <summary>
        /// Check if a skill can be unlocked.
        /// </summary>
        private bool CanUnlockSkill(string skillId, SkillNode skill)
        {
            if (ProgressionManager.Instance == null) return false;

            var progression = ProgressionManager.Instance.GetProgressionData();
            if (progression == null) return false;

            // Check if already unlocked
            if (progression.UnlockedSkills.Contains(skillId)) return false;

            // Check prerequisites
            foreach (var prereq in skill.Prerequisites)
            {
                if (!progression.UnlockedSkills.Contains(prereq))
                {
                    return false;
                }
            }

            // Check skill points
            return progression.SkillPoints >= skill.Cost;
        }

        private void OnSkillNodeClicked(string skillId)
        {
            selectedSkillId = skillId;
            ShowSkillDetails(skillId);
        }

        private void ShowSkillDetails(string skillId)
        {
            if (ProgressionManager.Instance == null) return;

            var skillTree = ProgressionManager.Instance.GetSkillTree();
            if (skillTree == null || !skillTree.ContainsKey(skillId)) return;

            var skill = skillTree[skillId];
            bool isUnlocked = ProgressionManager.Instance.IsSkillUnlocked(skillId);

            if (skillDetailPanel != null)
            {
                skillDetailPanel.SetActive(true);
            }

            if (skillNameText != null)
            {
                skillNameText.text = skill.Name;
            }

            if (skillDescriptionText != null)
            {
                skillDescriptionText.text = skill.Description;
            }

            if (skillCostText != null)
            {
                skillCostText.text = $"Cost: {skill.Cost} Skill Points";
            }

            if (unlockButton != null)
            {
                unlockButton.gameObject.SetActive(!isUnlocked);
                unlockButton.interactable = CanUnlockSkill(skillId, skill);
            }
        }

        private void OnUnlockButtonClicked()
        {
            if (selectedSkillId == null || ProgressionManager.Instance == null) return;

            bool success = ProgressionManager.Instance.UnlockSkill(selectedSkillId);

            if (success)
            {
                // Play feedback
                if (Core.JuiceManager.Instance != null)
                {
                    Core.JuiceManager.Instance.LightShake();
                }

                if (Core.AudioManager.Instance != null)
                {
                    Core.AudioManager.Instance.PlayBuildComplete();
                }

                UpdateDisplay();
                ShowSkillDetails(selectedSkillId); // Refresh detail panel
            }
        }

        private void HandleSkillUnlocked(string skillId)
        {
            UpdateDisplay();
        }

        /// <summary>
        /// Update UI display with current progression data.
        /// </summary>
        public void UpdateDisplay()
        {
            if (ProgressionManager.Instance == null) return;

            var progression = ProgressionManager.Instance.GetProgressionData();
            if (progression == null) return;

            if (skillPointsText != null)
            {
                skillPointsText.text = $"Available Skill Points: {progression.SkillPoints}";
            }

            UpdateSkillNodeStates();
        }
    }
}
