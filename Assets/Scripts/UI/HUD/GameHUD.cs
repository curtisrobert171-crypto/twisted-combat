using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EmpireOfGlass.Core;
using EmpireOfGlass.Swarm;

namespace EmpireOfGlass.UI
{
    /// <summary>
    /// HUD overlay displaying swarm count, hero health, and energy for raiding.
    /// Part of Phase 4: UI Polish - Sharpen overlays, tooltips, and juice.
    /// </summary>
    public class GameHUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI swarmCountText;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private Slider healthBar;
        [SerializeField] private TextMeshProUGUI energyText;
        [SerializeField] private TextMeshProUGUI gameStateText;

        [Header("Colors")]
        [SerializeField] private Color healthHighColor = Color.green;
        [SerializeField] private Color healthMidColor = Color.yellow;
        [SerializeField] private Color healthLowColor = Color.red;

        private HeroController hero;
        private SwarmController swarmController;

        private void Start()
        {
            hero = FindAnyObjectByType<HeroController>();
            swarmController = FindAnyObjectByType<SwarmController>();

            if (hero != null)
            {
                hero.OnHealthChanged += UpdateHealthDisplay;
                UpdateHealthDisplay(hero.CurrentHealth);
            }

            if (swarmController != null)
            {
                swarmController.OnSwarmSizeChanged += UpdateSwarmDisplay;
                UpdateSwarmDisplay(swarmController.ShardlingCount);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged += OnGameStateChanged;
                UpdateGameStateDisplay(GameManager.Instance.CurrentState);
            }
        }

        private void OnDestroy()
        {
            if (hero != null)
            {
                hero.OnHealthChanged -= UpdateHealthDisplay;
            }

            if (swarmController != null)
            {
                swarmController.OnSwarmSizeChanged -= UpdateSwarmDisplay;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged -= OnGameStateChanged;
            }
        }

        private void Update()
        {
            // Update energy display based on swarm size
            if (swarmController != null && energyText != null)
            {
                int energy = swarmController.GetRaidEnergy();
                energyText.text = $"Energy: {energy}";
            }
        }

        private void UpdateSwarmDisplay(int count)
        {
            if (swarmCountText != null)
            {
                swarmCountText.text = $"Swarm: {count}";
            }
        }

        private void UpdateHealthDisplay(int health)
        {
            if (hero == null) return;

            if (healthText != null)
            {
                healthText.text = $"HP: {health}";
            }

            if (healthBar != null)
            {
                float healthPercent = (float)health / 100f;
                healthBar.value = healthPercent;

                // Update health bar color based on percentage
                Image fillImage = healthBar.fillRect?.GetComponent<Image>();
                if (fillImage != null)
                {
                    if (healthPercent > 0.6f)
                        fillImage.color = healthHighColor;
                    else if (healthPercent > 0.3f)
                        fillImage.color = healthMidColor;
                    else
                        fillImage.color = healthLowColor;
                }
            }
        }

        private void OnGameStateChanged(GameManager.GameState previousState, GameManager.GameState newState)
        {
            UpdateGameStateDisplay(newState);
        }

        private void UpdateGameStateDisplay(GameManager.GameState state)
        {
            if (gameStateText != null)
            {
                gameStateText.text = state.ToString().ToUpper();
            }
        }
    }
}
