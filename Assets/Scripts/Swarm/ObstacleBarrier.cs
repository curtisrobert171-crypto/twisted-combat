using UnityEngine;

namespace EmpireOfGlass.Swarm
{
    /// <summary>
    /// Obsidian obstacle for the high-skill lane runner (Var 14).
    /// Obstacles reduce the swarm count on impact.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class ObstacleBarrier : MonoBehaviour
    {
        public enum ObstacleType
        {
            ObsidianWall,
            TrapBarrier,
            GlassFloor
        }

        [Header("Configuration")]
        [SerializeField] private ObstacleType obstacleType = ObstacleType.ObsidianWall;
        [SerializeField] private int damageToSwarm = 10;
        [SerializeField] private bool destroyOnImpact = true;

        private bool activated;
        private SwarmController cachedSwarm;

        private void Awake()
        {
            var collider = GetComponent<BoxCollider>();
            collider.isTrigger = true;
        }

        private void Start()
        {
            cachedSwarm = FindAnyObjectByType<SwarmController>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (activated) return;

            if (other.GetComponentInParent<Core.HeroController>() != null)
            {
                activated = true;
                if (cachedSwarm != null)
                {
                    cachedSwarm.ApplyMathGate(MathGate.GateOperation.Subtract, damageToSwarm);
                }

                Debug.Log($"[ObstacleBarrier] {obstacleType} hit! -{damageToSwarm} shardlings");

                if (destroyOnImpact)
                {
                    PlayShatterVFX();
                    Destroy(gameObject, 0.5f);
                }
            }
        }

        private void PlayShatterVFX()
        {
            // Placeholder for obsidian/glass shattering particle effects
            Debug.Log($"[ObstacleBarrier] {obstacleType} shattered");
        }
    }
}
