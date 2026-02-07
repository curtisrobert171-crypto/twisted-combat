using UnityEngine;

namespace EmpireOfGlass.Swarm
{
    /// <summary>
    /// Boss enemy with an HP wall that the swarm must melt through.
    /// Implements SwarmDamage = CurrentUnits - EnemyHP (TGDD Section 1).
    /// </summary>
    public class BossController : MonoBehaviour
    {
        [Header("Boss Stats")]
        [SerializeField] private int maxHP = 200;
        [SerializeField] private string bossName = "Obsidian Sentinel";

        [Header("VFX")]
        [SerializeField] private float shatterScale = 2f;

        private int currentHP;

        public int CurrentHP => currentHP;
        public int MaxHP => maxHP;
        public bool IsAlive => currentHP > 0;

        public event System.Action OnBossDefeated;
        public event System.Action<int, int> OnHPChanged;

        private void Awake()
        {
            currentHP = maxHP;
        }

        /// <summary>
        /// Take damage from the swarm. Calculates remaining shardlings after impact.
        /// </summary>
        public int TakeDamage(int swarmCount)
        {
            int damage = Mathf.Min(swarmCount, currentHP);
            currentHP -= damage;
            OnHPChanged?.Invoke(currentHP, maxHP);

            Debug.Log($"[BossController] {bossName} took {damage} damage. HP: {currentHP}/{maxHP}");

            if (currentHP <= 0)
            {
                OnBossDefeated?.Invoke();
                PlayShatterVFX();
            }

            // Return number of shardlings lost (equal to boss HP absorbed)
            return damage;
        }

        private void PlayShatterVFX()
        {
            Debug.Log($"[BossController] {bossName} shattered!");
            // Placeholder for physically-simulated glass shattering VFX
        }
    }
}
