using UnityEngine;

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
    }
}