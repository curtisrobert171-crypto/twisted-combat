using System.Collections.Generic;
using UnityEngine;

namespace EmpireOfGlass.City
{
    /// <summary>
    /// Manages the City meta-game loop: rebuilding a shattered 3D city using loot.
    /// Buildings reassemble from shards in reverse slow-motion (Var 16).
    /// Players design defense layouts for raids (Var 22 — UGC).
    /// </summary>
    public class CityBuilder : MonoBehaviour
    {
        [Header("City Grid")]
        [SerializeField] private int gridWidth = 10;
        [SerializeField] private int gridHeight = 10;
        [SerializeField] private float cellSize = 5f;

        [Header("Building Settings")]
        [SerializeField] private float rebuildAnimationDuration = 3f;
        [SerializeField] private AnimationCurve rebuildCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        private BuildingState[,] cityGrid;
        private readonly List<CityBuilding> buildings = new List<CityBuilding>();
        private readonly Dictionary<(int, int), CityBuilding> buildingLookup = new Dictionary<(int, int), CityBuilding>();

        public int TotalBuildings => buildings.Count;

        public event System.Action<CityBuilding> OnBuildingPlaced;
        public event System.Action<CityBuilding> OnBuildingUpgraded;

        private void Awake()
        {
            cityGrid = new BuildingState[gridWidth, gridHeight];
        }

        /// <summary>
        /// Place a building at a grid position using loot resources.
        /// </summary>
        public bool PlaceBuilding(int gridX, int gridY, BuildingType type, int goldCost)
        {
            if (gridX < 0 || gridX >= gridWidth || gridY < 0 || gridY >= gridHeight)
                return false;

            if (cityGrid[gridX, gridY] != BuildingState.Empty)
                return false;

            cityGrid[gridX, gridY] = BuildingState.Construction;

            var building = new CityBuilding
            {
                GridX = gridX,
                GridY = gridY,
                Type = type,
                State = BuildingState.Construction,
                Level = 1,
                WorldPosition = GridToWorld(gridX, gridY)
            };

            buildings.Add(building);
            buildingLookup[(gridX, gridY)] = building;
            OnBuildingPlaced?.Invoke(building);

            Debug.Log($"[CityBuilder] Building placed: {type} at ({gridX}, {gridY})");
            return true;
        }

        /// <summary>
        /// Complete construction of a building, triggering reverse-time shard assembly animation.
        /// </summary>
        public void CompleteBuilding(int gridX, int gridY)
        {
            if (gridX < 0 || gridX >= gridWidth || gridY < 0 || gridY >= gridHeight)
                return;

            cityGrid[gridX, gridY] = BuildingState.Completed;

            if (buildingLookup.TryGetValue((gridX, gridY), out var building))
            {
                building.State = BuildingState.Completed;
                OnBuildingUpgraded?.Invoke(building);
                Debug.Log($"[CityBuilder] Building completed with reverse-time animation: ({gridX}, {gridY})");
            }
        }

        /// <summary>
        /// Damage a building during a raid (reduces it to ruin state).
        /// </summary>
        public void DamageBuilding(int gridX, int gridY)
        {
            if (gridX < 0 || gridX >= gridWidth || gridY < 0 || gridY >= gridHeight)
                return;

            if (cityGrid[gridX, gridY] == BuildingState.Empty) return;

            cityGrid[gridX, gridY] = BuildingState.Ruin;

            if (buildingLookup.TryGetValue((gridX, gridY), out var building))
            {
                building.State = BuildingState.Ruin;
            }
        }

        /// <summary>
        /// Get the state of a grid cell.
        /// </summary>
        public BuildingState GetCellState(int gridX, int gridY)
        {
            if (gridX < 0 || gridX >= gridWidth || gridY < 0 || gridY >= gridHeight)
                return BuildingState.Empty;
            return cityGrid[gridX, gridY];
        }

        private Vector3 GridToWorld(int gridX, int gridY)
        {
            return new Vector3(gridX * cellSize, 0f, gridY * cellSize);
        }

        public float GetRebuildAnimationDuration() => rebuildAnimationDuration;
        public AnimationCurve GetRebuildCurve() => rebuildCurve;
    }

    /// <summary>
    /// Three states for city buildings as defined in the Prefab Manifest.
    /// </summary>
    public enum BuildingState
    {
        Empty,
        Ruin,
        Construction,
        Completed
    }

    /// <summary>
    /// Building types available in the city.
    /// </summary>
    public enum BuildingType
    {
        Residential,
        Defense,
        Resource,
        Vault,
        MegaStructure
    }

    /// <summary>
    /// Runtime data for a placed city building.
    /// </summary>
    [System.Serializable]
    public class CityBuilding
    {
        public int GridX;
        public int GridY;
        public BuildingType Type;
        public BuildingState State;
        public int Level;
        public Vector3 WorldPosition;
    }
}
