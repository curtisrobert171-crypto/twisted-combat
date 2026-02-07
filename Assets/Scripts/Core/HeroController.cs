using UnityEngine;

namespace EmpireOfGlass.Core
{
    /// <summary>
    /// Hero character controller for the fractured-light hero.
    /// Handles movement across lanes in the Swarm runner mode and transitions
    /// between God-View City, Orbiting Raid Cam, and Over-Shoulder Runner perspectives.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class HeroController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float forwardSpeed = 10f;
        [SerializeField] private float laneWidth = 3f;
        [SerializeField] private float laneSwitchSpeed = 10f;
        [SerializeField] private int laneCount = 3;

        [Header("Combat")]
        [SerializeField] private int baseHealth = 100;
        [SerializeField] private float glowIntensity = 1.5f;

        private Rigidbody rb;
        private int currentLane;
        private int targetLane;
        private bool isRunning;

        public int CurrentHealth { get; private set; }
        public bool IsAlive => CurrentHealth > 0;

        public event System.Action OnHeroDeath;
        public event System.Action<int> OnHealthChanged;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            CurrentHealth = baseHealth;
            currentLane = laneCount / 2;
            targetLane = currentLane;
        }

        private void Update()
        {
            if (!isRunning || !IsAlive) return;

            HandleLaneInput();
            UpdateLanePosition();
        }

        private void FixedUpdate()
        {
            if (!isRunning || !IsAlive) return;

            rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, forwardSpeed);
        }

        /// <summary>
        /// Start running in the Swarm lane runner mode.
        /// </summary>
        public void StartRunning()
        {
            isRunning = true;
        }

        /// <summary>
        /// Stop the runner.
        /// </summary>
        public void StopRunning()
        {
            isRunning = false;
            rb.linearVelocity = Vector3.zero;
        }

        /// <summary>
        /// Apply damage to the hero. Triggers death and revive offer at 0 HP.
        /// </summary>
        public void TakeDamage(int damage)
        {
            CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
            OnHealthChanged?.Invoke(CurrentHealth);

            if (CurrentHealth <= 0)
            {
                isRunning = false;
                OnHeroDeath?.Invoke();
            }
        }

        /// <summary>
        /// Revive the hero (e.g., after ad watch or $0.99 purchase — Loss Aversion, Var 24).
        /// </summary>
        public void Revive(int healthPercent = 50)
        {
            CurrentHealth = Mathf.CeilToInt(baseHealth * (healthPercent / 100f));
            OnHealthChanged?.Invoke(CurrentHealth);
        }

        private void HandleLaneInput()
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                targetLane = Mathf.Max(0, targetLane - 1);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                targetLane = Mathf.Min(laneCount - 1, targetLane + 1);
            }
        }

        private void UpdateLanePosition()
        {
            float targetX = (targetLane - laneCount / 2) * laneWidth;
            Vector3 pos = transform.position;
            pos.x = Mathf.MoveTowards(pos.x, targetX, laneSwitchSpeed * Time.deltaTime);
            transform.position = pos;
            currentLane = targetLane;
        }
    }
}
