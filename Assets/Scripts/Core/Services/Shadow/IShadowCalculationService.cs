using UnityEngine;
using Game.Services.Grid;

namespace Game.Services.Shadow
{
    public readonly struct ShadowProjection
    {
        public bool[] Wall1 { get; }
        public bool[] Wall2 { get; }

        public ShadowProjection(bool[] wall1, bool[] wall2)
        {
            Wall1 = wall1;
            Wall2 = wall2;
        }
    }

    public interface IShadowCalculationService
    {
        ShadowProjection Calculate(Vector3Int[] blocks, int gridSize);
        int CalculateDensity(int wallIndex, int cellIndex, IGridService gridService, int gridSize);
    }

    public class ShadowCalculationService : IShadowCalculationService
    {
        public ShadowProjection Calculate(Vector3Int[] blocks, int gridSize)
        {
            var cellCount = gridSize * gridSize;
            var wall1 = new bool[cellCount];
            var wall2 = new bool[cellCount];

            if (blocks is null)
                return new ShadowProjection(wall1, wall2);

            foreach (var block in blocks)
            {
                wall1[block.y * gridSize + block.z] = true;
                wall2[block.x * gridSize + block.y] = true;
            }

            return new ShadowProjection(wall1, wall2);
        }

        public int CalculateDensity(int wallIndex, int cellIndex, IGridService gridService, int gridSize)
        {
            var density = 0;
            if (wallIndex == 0)
            {
                var y = cellIndex / gridSize;
                var z = cellIndex % gridSize;
                for (var x = 0; x < gridSize; x++)
                    if (gridService.IsCellOccupied(new Vector3Int(x, y, z)))
                        density++;
            }
            else
            {
                var x = cellIndex / gridSize;
                var y = cellIndex % gridSize;
                for (var z = 0; z < gridSize; z++)
                    if (gridService.IsCellOccupied(new Vector3Int(x, y, z)))
                        density++;
            }
            return density;
        }
    }
}