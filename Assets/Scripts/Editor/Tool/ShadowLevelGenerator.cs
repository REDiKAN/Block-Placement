using System.Collections.Generic;
using UnityEngine;

namespace Game.Editor.Tool
{
    public class ShadowLevelGenerator
    {
        private const int _gridSize = 5;
        private const int _totalCells = 125;
        private const float _cellsPerDifficulty = 12.5f;

        public struct GenerationResult
        {
            public bool[,,] Grid;
            public bool[] Wall1Target;
            public bool[] Wall2Target;
            public bool[] FloorMatrix;
        }

        public GenerationResult Generate(int difficulty)
        {
            var floorMatrix = new bool[_gridSize * _gridSize];
            for (var i = 0; i < floorMatrix.Length; i++) floorMatrix[i] = true;

            if (difficulty >= 6)
            {
                var missingCount = Mathf.RoundToInt((difficulty - 5) * 2.5f);
                var availableFloors = new List<int>();

                for (var i = 0; i < floorMatrix.Length; i++) availableFloors.Add(i);

                for (var i = 0; i < missingCount && availableFloors.Count > 0; i++)
                {
                    var index = Random.Range(0, availableFloors.Count);
                    floorMatrix[availableFloors[index]] = false;
                    availableFloors.RemoveAt(index);
                }
            }

            var targetBlockCount = Mathf.RoundToInt(difficulty * _cellsPerDifficulty);
            var grid = new bool[_gridSize, _gridSize, _gridSize];
            var availablePositions = new List<Vector3Int>(_totalCells);

            for (var x = 0; x < _gridSize; x++)
                for (var z = 0; z < _gridSize; z++)
                    if (floorMatrix[x * _gridSize + z])
                        availablePositions.Add(new Vector3Int(x, 0, z));

            var placedCount = 0;

            while (placedCount < targetBlockCount && availablePositions.Count > 0)
            {
                var index = Random.Range(0, availablePositions.Count);
                var pos = availablePositions[index];

                availablePositions.RemoveAt(index);
                grid[pos.x, pos.y, pos.z] = true;
                placedCount++;

                if (pos.y + 1 < _gridSize)
                {
                    var upperPos = new Vector3Int(pos.x, pos.y + 1, pos.z);
                    if (!availablePositions.Contains(upperPos))
                        availablePositions.Add(upperPos);
                }
            }

            var wall1 = new bool[_gridSize * _gridSize];
            var wall2 = new bool[_gridSize * _gridSize];
            CalculateShadows(grid, _gridSize, wall1, wall2);

            return new GenerationResult
            {
                Grid = grid,
                Wall1Target = wall1,
                Wall2Target = wall2,
                FloorMatrix = floorMatrix
            };
        }

        private static void CalculateShadows(bool[,,] grid, int size, bool[] wall1, bool[] wall2)
        {
            for (var i = 0; i < wall1.Length; i++) wall1[i] = false;
            for (var i = 0; i < wall2.Length; i++) wall2[i] = false;

            for (var x = 0; x < size; x++)
                for (var y = 0; y < size; y++)
                    for (var z = 0; z < size; z++)
                        if (grid[x, y, z])
                        {
                            wall1[y * size + z] = true;
                            wall2[x * size + y] = true;
                        }
        }
    }
}