using System.Collections.Generic;
using UnityEngine;

namespace EmpireOfGlass.Swarm
{
    /// <summary>
    /// Individual shardling behavior with physics-based flocking (separation, cohesion, alignment).
    /// Supports GPU instancing for rendering 500+ units efficiently.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class ShardlingBehavior : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float maxSpeed = 8f;

        [Header("VFX")]
        [SerializeField] private float shatterForce = 10f;

        private Rigidbody rb;
        private SwarmController swarmController;
        private float separationWeight;
        private float cohesionWeight;
        private float alignmentWeight;
        private float neighborRadius;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        /// <summary>
        /// Initialize this shardling with flocking parameters.
        /// </summary>
        public void Initialize(SwarmController controller, float separation, float cohesion, float alignment, float radius)
        {
            swarmController = controller;
            separationWeight = separation;
            cohesionWeight = cohesion;
            alignmentWeight = alignment;
            neighborRadius = radius;
        }

        private void FixedUpdate()
        {
            if (swarmController == null) return;

            List<ShardlingBehavior> neighbors = swarmController.GetNeighbors(transform.position, neighborRadius);
            if (neighbors.Count == 0) return;

            Vector3 separation = CalculateSeparation(neighbors) * separationWeight;
            Vector3 cohesion = CalculateCohesion(neighbors) * cohesionWeight;
            Vector3 alignment = CalculateAlignment(neighbors) * alignmentWeight;

            Vector3 flockingForce = separation + cohesion + alignment;
            rb.AddForce(flockingForce * moveSpeed, ForceMode.Acceleration);

            if (rb.linearVelocity.magnitude > maxSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
            }
        }

        /// <summary>
        /// Play shatter VFX and destroy this shardling.
        /// </summary>
        public void Shatter()
        {
            if (rb != null)
            {
                rb.AddExplosionForce(shatterForce, transform.position, 2f);
            }
            Destroy(gameObject, 0.3f);
        }

        private Vector3 CalculateSeparation(List<ShardlingBehavior> neighbors)
        {
            Vector3 force = Vector3.zero;
            foreach (var neighbor in neighbors)
            {
                if (neighbor == this) continue;
                Vector3 diff = transform.position - neighbor.transform.position;
                float dist = diff.magnitude;
                if (dist > 0)
                {
                    force += diff.normalized / dist;
                }
            }
            return force;
        }

        private Vector3 CalculateCohesion(List<ShardlingBehavior> neighbors)
        {
            Vector3 center = Vector3.zero;
            int count = 0;
            foreach (var neighbor in neighbors)
            {
                if (neighbor == this) continue;
                center += neighbor.transform.position;
                count++;
            }
            if (count == 0) return Vector3.zero;
            center /= count;
            return (center - transform.position).normalized;
        }

        private Vector3 CalculateAlignment(List<ShardlingBehavior> neighbors)
        {
            Vector3 avgVelocity = Vector3.zero;
            int count = 0;
            foreach (var neighbor in neighbors)
            {
                if (neighbor == this) continue;
                if (neighbor.rb != null)
                {
                    avgVelocity += neighbor.rb.linearVelocity;
                    count++;
                }
            }
            if (count == 0) return Vector3.zero;
            avgVelocity /= count;
            return avgVelocity.normalized;
        }
    }
}
