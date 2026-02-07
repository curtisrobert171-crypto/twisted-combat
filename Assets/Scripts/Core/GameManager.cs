using UnityEngine;

namespace EmpireOfGlass.Core
{
    /// <summary>
    /// Central game manager controlling session pacing and game state transitions.
    /// Implements the forced rotation loop: Run (Swarm) → Build (City) → Raid → Run.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public enum GameState
        {
            Splash,
            Login,
            City,
            Swarm,
            Raid,
            Shop,
            Paused
        }

        [Header("State")]
        [SerializeField] private GameState currentState = GameState.Splash;

        [Header("Session Pacing")]
        [SerializeField] private float swarmSessionDuration = 90f;
        [SerializeField] private float raidSessionDuration = 60f;

        public GameState CurrentState => currentState;

        public event System.Action<GameState, GameState> OnStateChanged;

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

        private void Start()
        {
            TransitionTo(GameState.Splash);
        }

        /// <summary>
        /// Transitions the game to a new state, enforcing the session rotation loop.
        /// </summary>
        public void TransitionTo(GameState newState)
        {
            if (newState == currentState) return;

            GameState previousState = currentState;
            currentState = newState;

            Debug.Log($"[GameManager] State transition: {previousState} → {newState}");
            OnStateChanged?.Invoke(previousState, newState);
        }

        /// <summary>
        /// Advances to the next state in the forced rotation loop: Swarm → City → Raid → Swarm.
        /// </summary>
        public void AdvanceLoop()
        {
            switch (currentState)
            {
                case GameState.Swarm:
                    TransitionTo(GameState.City);
                    break;
                case GameState.City:
                    TransitionTo(GameState.Raid);
                    break;
                case GameState.Raid:
                    TransitionTo(GameState.Swarm);
                    break;
                default:
                    TransitionTo(GameState.Swarm);
                    break;
            }
        }

        /// <summary>
        /// Called during the First-Time User Experience: start at max power, shatter a skyscraper,
        /// then strip to Level 1 (The Hook — Var 12).
        /// </summary>
        public void StartFTUE()
        {
            Debug.Log("[GameManager] Starting FTUE: Max power demo sequence");
            TransitionTo(GameState.Swarm);
        }

        public float GetSwarmSessionDuration() => swarmSessionDuration;
        public float GetRaidSessionDuration() => raidSessionDuration;
    }
}
