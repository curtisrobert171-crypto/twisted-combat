using EmpireOfGlass.Core;
using EmpireOfGlass.Swarm;
using TMPro;
using UnityEngine;

namespace EmpireOfGlass.Raid
{
    /// <summary>
    /// App-facing controller that wires Swarm energy into RaidManager and updates raid HUD text.
    /// This reconstructs the raid feature into a playable flow: Enter Raid -> Spin -> Beam -> Reward.
    /// </summary>
    public class RaidAppController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RaidManager raidManager;
        [SerializeField] private SwarmController swarmController;

        [Header("Optional UI")]
        [SerializeField] private TextMeshProUGUI spinsText;
        [SerializeField] private TextMeshProUGUI energyText;
        [SerializeField] private TextMeshProUGUI targetFrequencyText;
        [SerializeField] private TextMeshProUGUI spinResultText;
        [SerializeField] private TextMeshProUGUI raidResultText;

        private void Awake()
        {
            if (raidManager == null)
            {
                raidManager = FindAnyObjectByType<RaidManager>();
            }

            if (swarmController == null)
            {
                swarmController = FindAnyObjectByType<SwarmController>();
            }
        }

        private void OnEnable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged += OnStateChanged;
            }

            if (raidManager != null)
            {
                raidManager.OnSpinResolved += OnSpinResolved;
                raidManager.OnRaidComplete += OnRaidComplete;
            }

            RefreshHUD();
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged -= OnStateChanged;
            }

            if (raidManager != null)
            {
                raidManager.OnSpinResolved -= OnSpinResolved;
                raidManager.OnRaidComplete -= OnRaidComplete;
            }
        }

        private void OnStateChanged(GameManager.GameState previous, GameManager.GameState next)
        {
            if (next == GameManager.GameState.Raid)
            {
                StartRaidFromSwarmEnergy();
            }
        }

        public void StartRaidFromSwarmEnergy()
        {
            if (raidManager == null) return;

            int energy = swarmController != null ? swarmController.GetRaidEnergy() : 0;
            raidManager.StartRaid(energy);
            RefreshHUD();
        }

        /// <summary>
        /// Hook this to a Spin button.
        /// </summary>
        public void SpinOnce()
        {
            if (raidManager == null) return;
            raidManager.Spin();
            RefreshHUD();
        }

        /// <summary>
        /// Hook this to a slider/button value representing player shot frequency [0..1].
        /// </summary>
        public void FireBeam(float playerFrequency)
        {
            if (raidManager == null) return;
            raidManager.FireFrequencyBeam(playerFrequency);
            RefreshHUD();
        }

        private void OnSpinResolved(SpinResult result)
        {
            if (spinResultText != null)
            {
                spinResultText.text =
                    $"Spin: {result.First} | {result.Second} | {result.Third}\n" +
                    $"Coins +{result.CoinsAwarded}  Shields +{result.ShieldsAwarded}  Attacks +{result.AttacksAwarded}" +
                    (result.TriggeredRaid ? "\nRAID TRIGGERED" : string.Empty);
            }
        }

        private void OnRaidComplete(RaidResult result)
        {
            if (raidResultText != null)
            {
                raidResultText.text =
                    $"Raid Complete\nTier {result.LootTier} | Gold +{result.Gold} | Shards +{result.Shards} | Precision {result.Precision:F2}";
            }
        }

        private void RefreshHUD()
        {
            if (raidManager == null) return;

            if (spinsText != null)
            {
                spinsText.text = $"Spins: {raidManager.CurrentSpins}";
            }

            if (energyText != null)
            {
                energyText.text = $"Energy: {raidManager.CurrentEnergy}";
            }

            if (targetFrequencyText != null)
            {
                targetFrequencyText.text = $"Target Freq: {raidManager.TargetFrequency:F2}";
            }
        }
    }
}
