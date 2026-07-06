using UnityEngine;

namespace Game.Editor.Tool
{
    public static class ShadowCalculator
    {
        public static void CalculateShadows(bool[,,] grid, int size, bool[] wall1, bool[] wall2)
        {
            for (var i = 0; i < wall1.Length; i++)
            {
                wall1[i] = false;
                wall2[i] = false;
            }

            for (var x = 0; x < size; x++)
                for (var y = 0; y < size; y++)
                    for (var z = 0; z < size; z++)
                    {
                        if (grid[x, y, z])
                        {
                            wall1[y * size + z] = true;
                            wall2[x * size + y] = true;
                        }
                    }
        }
    }
}