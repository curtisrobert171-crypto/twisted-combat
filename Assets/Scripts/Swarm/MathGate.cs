using UnityEngine;

namespace EmpireOfGlass.Swarm
{
    /// <summary>
    /// Math gate that multiplies/adds to the hero's shardling count when passed through.
    /// Implements the core mechanic: x2, x5, +10 gates that turn 1 hero into 500+ shardlings (Var 13).
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class MathGate : MonoBehaviour
    {
        public enum GateOperation
        {
            Multiply,
            Add,
            Subtract
        }

        [Header("Gate Configuration")]
        [SerializeField] private GateOperation operation = GateOperation.Multiply;
        [SerializeField] private int value = 2;

        [Header("Visuals")]
        [SerializeField] private Color gateColor = Color.cyan;
        [SerializeField] private string displayText;

        private bool activated;

        public GateOperation Operation => operation;
        public int Value => value;

        private void Awake()
        {
            var collider = GetComponent<BoxCollider>();
            collider.isTrigger = true;

            if (string.IsNullOrEmpty(displayText))
            {
                displayText = operation switch
                {
                    GateOperation.Multiply => $"x{value}",
                    GateOperation.Add => $"+{value}",
                    GateOperation.Subtract => $"-{value}",
                    _ => value.ToString()
                };
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (activated) return;

            if (other.GetComponentInParent<Core.HeroController>() != null)
            {
                activated = true;
                var swarm = FindAnyObjectByType<SwarmController>();
                if (swarm != null)
                {
                    swarm.ApplyMathGate(operation, value);
                }
                PlayRefractionVFX();
            }
        }

        private void PlayRefractionVFX()
        {
            Debug.Log($"[MathGate] Activated: {displayText} — Refraction gate VFX");
            // VFX: Light beam hits prism and refracts (placeholder for particle system)
        }

        /// <summary>
        /// Returns the display string for UI overlay on the gate.
        /// </summary>
        public string GetDisplayText() => displayText;
    }
}
