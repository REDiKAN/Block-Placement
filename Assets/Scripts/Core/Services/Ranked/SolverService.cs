using System;
using Game.Data.Ranked;
using UnityEngine;

namespace Game.Services.Ranked
{
    public class SolverService : ISolverService
    {
        private readonly RankedCoreConfig _config;
        private readonly bool[,,] _grid;
        private readonly int[] _w1Count;
        private readonly int[] _w2Count;
        private const int GridSize = 5;
        private const int CellCount = 25;
        private const int TotalCells = 125;

        public SolverService(RankedCoreConfig config)
        {
            _config = config;
            _grid = new bool[GridSize, GridSize, GridSize];
            _w1Count = new int[CellCount];
            _w2Count = new int[CellCount];
        }

        public SolverResult Solve(bool[] wall1, bool[] wall2, bool[] floor)
        {
            var solutionsFound = 0;
            var minBlocks = int.MaxValue;
            var solverSteps = 0;
            Array.Clear(_grid, 0, _grid.Length);
            Array.Clear(_w1Count, 0, _w1Count.Length);
            Array.Clear(_w2Count, 0, _w2Count.Length);
            SolveRecursive(wall1, wall2, floor, 0, 0, ref solutionsFound, ref minBlocks, ref solverSteps, 2);
            return new SolverResult(solutionsFound, solverSteps, minBlocks == int.MaxValue ? 0 : minBlocks);
        }

        public SolverResult SolveWithoutRotation(bool[] wall1, bool[] wall2, bool[] floor) =>
            Solve(wall1, wall2, floor);

        public int CountSolutions(bool[] wall1, bool[] wall2, bool[] floor, int maxSolutions)
        {
            var solutionsFound = 0;
            var minBlocks = int.MaxValue;
            var solverSteps = 0;
            Array.Clear(_grid, 0, _grid.Length);
            Array.Clear(_w1Count, 0, _w1Count.Length);
            Array.Clear(_w2Count, 0, _w2Count.Length);
            SolveRecursive(wall1, wall2, floor, 0, 0, ref solutionsFound, ref minBlocks, ref solverSteps, maxSolutions);
            return solutionsFound;
        }

        private void SolveRecursive(
            bool[] wall1,
            bool[] wall2,
            bool[] floor,
            int index,
            int currentBlocks,
            ref int solutionsFound,
            ref int minBlocks,
            ref int solverSteps,
            int maxSolutions)
        {
            if (solutionsFound >= maxSolutions) return;
            if (solverSteps >= _config.MaxSolverSteps) return;
            solverSteps++;
            if (index == TotalCells)
            {
                for (var i = 0; i < CellCount; i++)
                {
                    if ((_w1Count[i] > 0) != wall1[i]) return;
                    if ((_w2Count[i] > 0) != wall2[i]) return;
                }
                solutionsFound++;
                if (currentBlocks < minBlocks) minBlocks = currentBlocks;
                return;
            }
            var x = index / GridSize / GridSize;
            var rem = index % (GridSize * GridSize);
            var y = rem / GridSize;
            var z = rem % GridSize;
            var canPlace = floor[x * GridSize + z] && wall1[y * GridSize + z] && wall2[x * GridSize + y];
            if (y > 0 && !_grid[x, y - 1, z]) canPlace = false;
            if (canPlace)
            {
                _grid[x, y, z] = true;
                _w1Count[y * GridSize + z]++;
                _w2Count[x * GridSize + y]++;
                SolveRecursive(wall1, wall2, floor, index + 1, currentBlocks + 1, ref solutionsFound, ref minBlocks, ref solverSteps, maxSolutions);
                _grid[x, y, z] = false;
                _w1Count[y * GridSize + z]--;
                _w2Count[x * GridSize + y]--;
            }
            var isLastX = x == GridSize - 1;
            if (isLastX && wall1[y * GridSize + z] && _w1Count[y * GridSize + z] == 0) return;
            var isLastZ = z == GridSize - 1;
            if (isLastZ && wall2[x * GridSize + y] && _w2Count[x * GridSize + y] == 0) return;
            SolveRecursive(wall1, wall2, floor, index + 1, currentBlocks, ref solutionsFound, ref minBlocks, ref solverSteps, maxSolutions);
        }
    }
}