using UnityEngine;

namespace EmpireOfGlass.Core
{
    /// <summary>
    /// Manages camera transitions between the three perspectives (Var 5):
    /// - God-View (City): Top-down view of the player's base
    /// - Orbiting Raid Cam: Orbit around rival base during raids
    /// - Over-Shoulder Runner: Behind-hero view during Swarm runs
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        public enum CameraMode
        {
            GodView,
            OrbitRaid,
            OverShoulderRunner
        }

        [Header("God-View (City)")]
        [SerializeField] private Vector3 godViewOffset = new Vector3(0f, 30f, -10f);
        [SerializeField] private float godViewAngle = 60f;

        [Header("Over-Shoulder Runner (Swarm)")]
        [SerializeField] private Vector3 shoulderOffset = new Vector3(0f, 5f, -8f);
        [SerializeField] private float shoulderAngle = 15f;

        [Header("Orbit Raid Cam")]
        [SerializeField] private float orbitRadius = 15f;
        [SerializeField] private float orbitSpeed = 30f;
        [SerializeField] private float orbitHeight = 10f;

        [Header("Transitions")]
        [SerializeField] private float transitionDuration = 1.5f;

        private CameraMode currentMode = CameraMode.GodView;
        private Transform followTarget;
        private Transform orbitCenter;
        private float orbitAngle;

        public CameraMode CurrentMode => currentMode;

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged += HandleGameStateChanged;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged -= HandleGameStateChanged;
            }
        }

        private void LateUpdate()
        {
            switch (currentMode)
            {
                case CameraMode.GodView:
                    UpdateGodView();
                    break;
                case CameraMode.OverShoulderRunner:
                    UpdateOverShoulder();
                    break;
                case CameraMode.OrbitRaid:
                    UpdateOrbitRaid();
                    break;
            }
        }

        /// <summary>
        /// Set the target for the camera to follow.
        /// </summary>
        public void SetFollowTarget(Transform target)
        {
            followTarget = target;
        }

        /// <summary>
        /// Set the center point for orbit cam during raids.
        /// </summary>
        public void SetOrbitCenter(Transform center)
        {
            orbitCenter = center;
        }

        /// <summary>
        /// Transition to a new camera mode with smooth interpolation.
        /// </summary>
        public void TransitionToMode(CameraMode mode)
        {
            currentMode = mode;
            Debug.Log($"[CameraController] Transitioning to {mode}");
        }

        private void HandleGameStateChanged(GameManager.GameState from, GameManager.GameState to)
        {
            switch (to)
            {
                case GameManager.GameState.City:
                    TransitionToMode(CameraMode.GodView);
                    break;
                case GameManager.GameState.Swarm:
                    TransitionToMode(CameraMode.OverShoulderRunner);
                    break;
                case GameManager.GameState.Raid:
                    TransitionToMode(CameraMode.OrbitRaid);
                    break;
            }
        }

        private void UpdateGodView()
        {
            if (followTarget == null) return;
            Vector3 targetPos = followTarget.position + godViewOffset;
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 3f);
            transform.rotation = Quaternion.Euler(godViewAngle, 0f, 0f);
        }

        private void UpdateOverShoulder()
        {
            if (followTarget == null) return;
            Vector3 targetPos = followTarget.position + shoulderOffset;
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 5f);
            transform.rotation = Quaternion.Euler(shoulderAngle, 0f, 0f);
        }

        private void UpdateOrbitRaid()
        {
            if (orbitCenter == null) return;
            orbitAngle += orbitSpeed * Time.deltaTime;
            float rad = orbitAngle * Mathf.Deg2Rad;
            Vector3 pos = orbitCenter.position + new Vector3(
                Mathf.Cos(rad) * orbitRadius,
                orbitHeight,
                Mathf.Sin(rad) * orbitRadius
            );
            transform.position = pos;
            transform.LookAt(orbitCenter.position);
        }
    }
}
