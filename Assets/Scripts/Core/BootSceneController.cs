using UnityEngine;
using UnityEngine.SceneManagement;

namespace EmpireOfGlass.Core
{
    /// <summary>
    /// Controls the Boot scene, providing simple navigation to prototype scenes.
    /// Initializes GameBootstrap and offers UI buttons or key input to jump to Swarm/City/Raid scenes.
    /// </summary>
    public class BootSceneController : MonoBehaviour
    {
        [Header("Bootstrap")]
        [SerializeField] private GameBootstrap gameBootstrap;

        private void Start()
        {
            Debug.Log("[BootSceneController] Boot scene loaded. Press 1 for Swarm, 2 for City, 3 for Raid");
        }

        private void Update()
        {
            // Simple key input for scene navigation
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                LoadSwarmScene();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                LoadCityScene();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            {
                LoadRaidScene();
            }
        }

        /// <summary>
        /// Load the SwarmPrototype scene.
        /// </summary>
        public void LoadSwarmScene()
        {
            Debug.Log("[BootSceneController] Loading SwarmPrototype scene");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TransitionTo(GameManager.GameState.Swarm);
            }
            SceneManager.LoadScene("SwarmPrototype");
        }

        /// <summary>
        /// Load the CityPrototype scene.
        /// </summary>
        public void LoadCityScene()
        {
            Debug.Log("[BootSceneController] Loading CityPrototype scene");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TransitionTo(GameManager.GameState.City);
            }
            SceneManager.LoadScene("CityPrototype");
        }

        /// <summary>
        /// Load the RaidPrototype scene.
        /// </summary>
        public void LoadRaidScene()
        {
            Debug.Log("[BootSceneController] Loading RaidPrototype scene");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TransitionTo(GameManager.GameState.Raid);
            }
            SceneManager.LoadScene("RaidPrototype");
        }
    }
}
