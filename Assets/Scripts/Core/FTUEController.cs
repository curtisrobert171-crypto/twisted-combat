using UnityEngine;

namespace EmpireOfGlass.Core
{
    /// <summary>
    /// Controls the First-Time User Experience (FTUE) scripted sequence (Var 12).
    /// The Hook: Start at max power, shatter a skyscraper, then strip to Level 1.
    /// Guides the player through each game loop for the first time.
    /// </summary>
    public class FTUEController : MonoBehaviour
    {
        public static FTUEController Instance { get; private set; }

        public enum FTUEStep
        {
            NotStarted,
            MaxPowerDemo,       // Start at max power
            SkyscraperShatter,  // Shatter a skyscraper with full swarm
            PowerStripped,      // Strip to Level 1
            FirstSwarmRun,      // Tutorial swarm run with gates
            FirstCityBuild,     // Tutorial city placement
            FirstRaid,          // Tutorial raid
            ScriptedDefeat,     // Scripted loss → triggers Starter Pack offer (Var 30)
            StarterPackOffer,   // $0.99 conversion offer
            Complete
        }

        [Header("FTUE Settings")]
        [SerializeField] private int maxPowerShardlings = 500;
        [SerializeField] private int postStripShardlings = 1;

        private FTUEStep currentStep = FTUEStep.NotStarted;

        public FTUEStep CurrentStep => currentStep;
        public bool IsComplete => currentStep == FTUEStep.Complete;
        public bool IsActive => currentStep != FTUEStep.NotStarted && currentStep != FTUEStep.Complete;

        public event System.Action<FTUEStep> OnStepChanged;
        public event System.Action OnFTUEComplete;

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
        /// Check if the player needs the FTUE (first session).
        /// </summary>
        public bool ShouldStartFTUE()
        {
            var player = Data.SaveManager.Instance?.CurrentPlayer;
            if (player == null) return false;
            return !player.FTUECompleted && player.Level <= 1;
        }

        /// <summary>
        /// Begin the FTUE scripted sequence.
        /// </summary>
        public void StartFTUE()
        {
            if (!ShouldStartFTUE()) return;

            Debug.Log("[FTUEController] Starting First-Time User Experience");
            AdvanceToStep(FTUEStep.MaxPowerDemo);
        }

        /// <summary>
        /// Advance to the next step in the FTUE sequence.
        /// </summary>
        public void AdvanceStep()
        {
            FTUEStep nextStep = currentStep switch
            {
                FTUEStep.MaxPowerDemo => FTUEStep.SkyscraperShatter,
                FTUEStep.SkyscraperShatter => FTUEStep.PowerStripped,
                FTUEStep.PowerStripped => FTUEStep.FirstSwarmRun,
                FTUEStep.FirstSwarmRun => FTUEStep.FirstCityBuild,
                FTUEStep.FirstCityBuild => FTUEStep.FirstRaid,
                FTUEStep.FirstRaid => FTUEStep.ScriptedDefeat,
                FTUEStep.ScriptedDefeat => FTUEStep.StarterPackOffer,
                FTUEStep.StarterPackOffer => FTUEStep.Complete,
                _ => FTUEStep.Complete
            };

            AdvanceToStep(nextStep);
        }

        private void AdvanceToStep(FTUEStep step)
        {
            currentStep = step;
            Debug.Log($"[FTUEController] FTUE step: {step}");
            OnStepChanged?.Invoke(step);

            switch (step)
            {
                case FTUEStep.MaxPowerDemo:
                    HandleMaxPowerDemo();
                    break;
                case FTUEStep.SkyscraperShatter:
                    HandleSkyscraperShatter();
                    break;
                case FTUEStep.PowerStripped:
                    HandlePowerStripped();
                    break;
                case FTUEStep.FirstSwarmRun:
                    HandleFirstSwarmRun();
                    break;
                case FTUEStep.FirstCityBuild:
                    HandleFirstCityBuild();
                    break;
                case FTUEStep.FirstRaid:
                    HandleFirstRaid();
                    break;
                case FTUEStep.ScriptedDefeat:
                    HandleScriptedDefeat();
                    break;
                case FTUEStep.StarterPackOffer:
                    HandleStarterPackOffer();
                    break;
                case FTUEStep.Complete:
                    HandleComplete();
                    break;
            }
        }

        private void HandleMaxPowerDemo()
        {
            // Give the player a full 500-unit swarm to feel powerful
            Debug.Log($"[FTUEController] Max power demo: {maxPowerShardlings} shardlings");
            GameManager.Instance?.TransitionTo(GameManager.GameState.Swarm);
        }

        private void HandleSkyscraperShatter()
        {
            // Player destroys a skyscraper with the massive swarm
            Debug.Log("[FTUEController] Skyscraper shatter sequence");
        }

        private void HandlePowerStripped()
        {
            // Strip power back to 1 shardling — the rebuild begins
            Debug.Log($"[FTUEController] Power stripped to {postStripShardlings} shardling(s)");
        }

        private void HandleFirstSwarmRun()
        {
            // Guided tutorial: run through gates to rebuild swarm
            Debug.Log("[FTUEController] First swarm run tutorial");
            GameManager.Instance?.TransitionTo(GameManager.GameState.Swarm);
        }

        private void HandleFirstCityBuild()
        {
            // Tutorial: place first building in the city
            Debug.Log("[FTUEController] First city build tutorial");
            GameManager.Instance?.TransitionTo(GameManager.GameState.City);
        }

        private void HandleFirstRaid()
        {
            // Tutorial: raid a weak NPC base
            Debug.Log("[FTUEController] First raid tutorial");
            GameManager.Instance?.TransitionTo(GameManager.GameState.Raid);
        }

        private void HandleScriptedDefeat()
        {
            // Intentionally lose to trigger loss aversion → Starter Pack offer (Var 24, 30)
            Debug.Log("[FTUEController] Scripted defeat — triggers starter pack offer");
        }

        private void HandleStarterPackOffer()
        {
            // Show the $0.99 starter pack conversion offer (Var 30)
            Debug.Log("[FTUEController] Starter pack offer displayed ($0.99)");
            GameManager.Instance?.TransitionTo(GameManager.GameState.Shop);
        }

        private void HandleComplete()
        {
            var player = Data.SaveManager.Instance?.CurrentPlayer;
            if (player != null)
            {
                player.FTUECompleted = true;
            }

            Debug.Log("[FTUEController] FTUE complete — entering main game loop");
            OnFTUEComplete?.Invoke();
            GameManager.Instance?.TransitionTo(GameManager.GameState.City);
        }
    }
}
