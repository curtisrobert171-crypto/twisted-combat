using UnityEngine;

namespace EmpireOfGlass.Core
{
    /// <summary>
    /// Level generator for the Swarm runner track.
    /// Procedurally places math gates, obstacles, and boss encounters along the lane.
    /// </summary>
    public class LevelGenerator : MonoBehaviour
    {
        [Header("Track Settings")]
        [SerializeField] private float trackLength = 200f;
        [SerializeField] private float segmentLength = 20f;
        [SerializeField] private int laneCount = 3;
        [SerializeField] private float laneWidth = 3f;

        [Header("Prefabs")]
        [SerializeField] private GameObject mathGateMultiplyPrefab;
        [SerializeField] private GameObject mathGateAddPrefab;
        [SerializeField] private GameObject obsidianWallPrefab;
        [SerializeField] private GameObject trapBarrierPrefab;
        [SerializeField] private GameObject bossPrefab;

        [Header("Generation")]
        [SerializeField] private float gateSpacing = 15f;
        [SerializeField] private float obstacleSpacing = 10f;
        [SerializeField] [Range(0f, 1f)] private float obstacleDensity = 0.4f;

        /// <summary>
        /// Generate a full run track with gates, obstacles, and a boss at the end.
        /// </summary>
        public void GenerateTrack()
        {
            ClearTrack();

            float currentZ = segmentLength;

            while (currentZ < trackLength - segmentLength)
            {
                // Place math gates in pairs (player chooses a lane)
                if (currentZ % gateSpacing < segmentLength)
                {
                    PlaceMathGatePair(currentZ);
                }

                // Place obstacles randomly based on density
                if (Random.value < obstacleDensity)
                {
                    PlaceObstacle(currentZ + Random.Range(0f, segmentLength));
                }

                currentZ += segmentLength;
            }

            // Place boss at the end of the track
            if (bossPrefab != null)
            {
                Instantiate(bossPrefab, new Vector3(0f, 0f, trackLength), Quaternion.identity, transform);
            }

            Debug.Log($"[LevelGenerator] Track generated: {trackLength}m, {transform.childCount} objects");
        }

        private void PlaceMathGatePair(float zPos)
        {
            // Place two gates side by side — player picks one lane
            int lane1 = Random.Range(0, laneCount);
            int lane2 = (lane1 + 1) % laneCount;

            float x1 = (lane1 - laneCount / 2) * laneWidth;
            float x2 = (lane2 - laneCount / 2) * laneWidth;

            if (mathGateMultiplyPrefab != null)
            {
                Instantiate(mathGateMultiplyPrefab, new Vector3(x1, 0f, zPos), Quaternion.identity, transform);
            }
            if (mathGateAddPrefab != null)
            {
                Instantiate(mathGateAddPrefab, new Vector3(x2, 0f, zPos), Quaternion.identity, transform);
            }
        }

        private void PlaceObstacle(float zPos)
        {
            int lane = Random.Range(0, laneCount);
            float x = (lane - laneCount / 2) * laneWidth;
            Vector3 pos = new Vector3(x, 0f, zPos);

            GameObject prefab = Random.value > 0.5f ? obsidianWallPrefab : trapBarrierPrefab;
            if (prefab != null)
            {
                Instantiate(prefab, pos, Quaternion.identity, transform);
            }
        }

        private void ClearTrack()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
    }
}
