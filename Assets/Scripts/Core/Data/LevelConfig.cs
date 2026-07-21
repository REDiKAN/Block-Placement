using UnityEngine;
using Game.Attributes;

namespace Game.Data
{
    [System.Serializable]
    public struct WallCellDensityData
    {
        [field: SerializeField] public bool IsDensityEnabled { get; private set; }
        [field: SerializeField, Range(0, 5)] public int TargetDensity { get; private set; }

        public WallCellDensityData(bool isDensityEnabled, int targetDensity)
        {
            IsDensityEnabled = isDensityEnabled;
            TargetDensity = targetDensity;
        }
    }

    [System.Serializable]
    public class WallData
    {
        [field: SerializeField] public WallCellDensityData[] CellDensities { get; private set; }

        public void SetDensities(WallCellDensityData[] densities) => CellDensities = densities;
    }

    [CreateAssetMenu(fileName = nameof(LevelConfig), menuName = "Game/" + nameof(LevelConfig))]
    public class LevelConfig : ScriptableObject
    {
        [field: Title("Level", CustomColor.Cyan, CustomColor.Blue)]
        [field: SerializeField] public Vector3Int[] InitialBlocks { get; private set; }
        [field: SerializeField] public bool[] FloorMatrix { get; private set; }

        [field: Title("Walls", CustomColor.Green, CustomColor.DarkGreen)]
        [field: SerializeField] public WallData WallYZ { get; private set; }
        [field: SerializeField] public WallData WallXY { get; private set; }

        [field: Title("Block Limit", CustomColor.Yellow, CustomColor.Orange)]
        [field: SerializeField] public bool IsBlockLimitEnabled { get; private set; }
        [field: SerializeField] public int MaxBlocks { get; private set; } = -1;

        public void SetData(Vector3Int[] initialBlocks, bool[] floorMatrix, WallData wallYZ, WallData wallXY)
        {
            InitialBlocks = initialBlocks;
            FloorMatrix = floorMatrix;
            WallYZ = wallYZ;
            WallXY = wallXY;
        }

        public void SetBlocksAndFloor(Vector3Int[] initialBlocks, bool[] floorMatrix)
        {
            InitialBlocks = initialBlocks;
            FloorMatrix = floorMatrix;
        }

        public void SetBlockLimit(bool isEnabled, int maxBlocks)
        {
            IsBlockLimitEnabled = isEnabled;
            MaxBlocks = maxBlocks;
        }
    }
}