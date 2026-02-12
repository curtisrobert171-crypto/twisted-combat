using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EmpireOfGlass.Data;

namespace EmpireOfGlass.UI
{
    /// <summary>
    /// UI panel for displaying and managing player progression: level, XP, skill points.
    /// </summary>
    public class ProgressionPanel : MonoBehaviour
    {
        [Header("Level Display")]
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI xpText;
        [SerializeField] private Slider xpSlider;
        [SerializeField] private TextMeshProUGUI skillPointsText;

        [Header("Notifications")]
        [SerializeField] private GameObject levelUpNotification;
        [SerializeField] private TextMeshProUGUI levelUpText;
        [SerializeField] private float notificationDuration = 3f;

        [Header("Animation")]
        [SerializeField] private float xpFillSpeed = 2f;

        private float targetXPProgress = 0f;
        private float currentXPProgress = 0f;

        private void Start()
        {
            if (ProgressionManager.Instance != null)
            {
                ProgressionManager.Instance.OnLevelUp += HandleLevelUp;
                ProgressionManager.Instance.OnXPGained += HandleXPGained;
            }

            if (levelUpNotification != null)
            {
                levelUpNotification.SetActive(false);
            }

            UpdateDisplay();
        }

        private void OnDestroy()
        {
            if (ProgressionManager.Instance != null)
            {
                ProgressionManager.Instance.OnLevelUp -= HandleLevelUp;
                ProgressionManager.Instance.OnXPGained -= HandleXPGained;
            }
        }

        private void Update()
        {
            // Smooth XP bar animation
            if (Mathf.Abs(currentXPProgress - targetXPProgress) > 0.01f)
            {
                currentXPProgress = Mathf.Lerp(currentXPProgress, targetXPProgress, xpFillSpeed * Time.deltaTime);
                if (xpSlider != null)
                {
                    xpSlider.value = currentXPProgress;
                }
            }
        }

        /// <summary>
        /// Update all UI elements with current progression data.
        /// </summary>
        public void UpdateDisplay()
        {
            if (ProgressionManager.Instance == null) return;

            var progression = ProgressionManager.Instance.GetProgressionData();
            if (progression == null) return;

            // Update level
            if (levelText != null)
            {
                levelText.text = $"Level {progression.Level}";
            }

            // Update XP
            int xpForLevel = ProgressionManager.Instance.GetXPForLevel(progression.Level);
            if (xpText != null)
            {
                xpText.text = $"{progression.CurrentXP} / {xpForLevel} XP";
            }

            // Update XP slider
            targetXPProgress = ProgressionManager.Instance.GetLevelProgress();
            if (xpSlider != null)
            {
                xpSlider.maxValue = 1f;
            }

            // Update skill points
            if (skillPointsText != null)
            {
                skillPointsText.text = $"Skill Points: {progression.SkillPoints}";
            }
        }

        private void HandleLevelUp(int newLevel)
        {
            UpdateDisplay();
            ShowLevelUpNotification(newLevel);

            // Play juice effect
            if (Core.JuiceManager.Instance != null)
            {
                Core.JuiceManager.Instance.PulseUI(transform);
            }

            // Play audio
            if (Core.AudioManager.Instance != null)
            {
                Core.AudioManager.Instance.PlayBuildComplete();
            }
        }

        private void HandleXPGained(int amount)
        {
            UpdateDisplay();
        }

        private void ShowLevelUpNotification(int level)
        {
            if (levelUpNotification == null) return;

            if (levelUpText != null)
            {
                levelUpText.text = $"Level Up!\nNow Level {level}";
            }

            levelUpNotification.SetActive(true);
            CancelInvoke(nameof(HideLevelUpNotification));
            Invoke(nameof(HideLevelUpNotification), notificationDuration);
        }

        private void HideLevelUpNotification()
        {
            if (levelUpNotification != null)
            {
                levelUpNotification.SetActive(false);
            }
        }
    }
}
