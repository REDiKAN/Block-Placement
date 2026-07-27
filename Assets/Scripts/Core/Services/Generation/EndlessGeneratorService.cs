using Game.Data;
using Game.Services.Dev;
using Game.Services.Grid;
using Game.Services.Placement;
using Game.Services.Rotation;
using Game.Services.Shadow;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using Zenject;

namespace Game.Services.Generation
{
    public class EndlessGeneratorService : IEndlessGeneratorService, IInitializable
    {
        public IObservable<Unit> OnLevelGenerated => _onLevelGenerated;
        private readonly Subject<Unit> _onLevelGenerated = new();

        private readonly IGenerationContext _context;
        private readonly IGridService _gridService;
        private readonly IShadowCalculationService _calculationService;
        private readonly ITargetDensityProjectionService _projectionService;
        private readonly IShadowDensityService _densityService;
        private readonly IRotationService _rotationService;
        private readonly IBlockPlacementService _placementService;
        private readonly IShadowValidationService _validationService;

        private const int GridSize = 5;
        private const int CellCount = 25;
        private const int TotalCells = 125;

        public EndlessGeneratorService(
            IGenerationContext context,
            IGridService gridService,
            IShadowCalculationService calculationService,
            ITargetDensityProjectionService projectionService,
            IShadowDensityService densityService,
            IRotationService rotationService,
            IBlockPlacementService placementService,
            IShadowValidationService validationService)
        {
            _context = context;
            _gridService = gridService;
            _calculationService = calculationService;
            _projectionService = projectionService;
            _densityService = densityService;
            _rotationService = rotationService;
            _placementService = placementService;
            _validationService = validationService;
        }

        public void Initialize()
        {
            if (_context.IsEndlessModeActive.Value)
                GenerateNext();
        }

        public void GenerateNext()
        {
            _placementService.ClearAll();
            var settings = _context.CurrentSettings.Value;
            var shape = GenerateValidShape(settings);
            var floor = GenerateFloor(settings, shape);

            ApplyFloor(floor);
            _rotationService.SetTargetBlocks(shape.ToArray());

            var projection = _calculationService.Calculate(shape.ToArray(), GridSize);
            var densities = CalculateDensities(shape, settings);

            _projectionService.SetDensities(densities.WallYZ, densities.WallXY);
            ApplyDensitiesToService(densities);

            _validationService.ForceRevalidate();

            _onLevelGenerated.OnNext(Unit.Default);
        }

        private List<Vector3Int> GenerateValidShape(CustomGenerationSettings settings)
        {
            for (var attempt = 0; attempt < 50; attempt++)
            {
                var shape = TryGenerateShape(settings);
                if (shape.Count == 0 || !IsConnected(shape)) continue;

                var projection = _calculationService.Calculate(shape.ToArray(), GridSize);
                var floor = GenerateFloor(settings, shape);

                if (!HasMultipleSolutions(projection.Wall1, projection.Wall2, floor, settings))
                    return shape;
            }
            return TryGenerateShape(settings);
        }

        private List<Vector3Int> TryGenerateShape(CustomGenerationSettings settings)
        {
            var shape = new HashSet<Vector3Int>();
            var targetBlocks = 5 + settings.Difficulty * 3;
            var symmetry = settings.IsSymmetrical;

            var available = new List<Vector3Int>();
            for (var x = 0; x < GridSize; x++)
                for (var z = 0; z < GridSize; z++)
                    available.Add(new Vector3Int(x, 0, z));

            while (shape.Count < targetBlocks && available.Count > 0)
            {
                var idx = UnityEngine.Random.Range(0, available.Count);
                var cell = available[idx];
                available.RemoveAt(idx);

                if (symmetry && cell.x > 2) continue;

                shape.Add(cell);
                if (symmetry && cell.x != 2)
                    shape.Add(new Vector3Int(GridSize - 1 - cell.x, cell.y, cell.z));

                if (cell.y + 1 < GridSize)
                {
                    var up = new Vector3Int(cell.x, cell.y + 1, cell.z);
                    if (!available.Contains(up) && !shape.Contains(up)) available.Add(up);
                }

                var dirs = new[] { Vector3Int.right, Vector3Int.left, Vector3Int.forward, Vector3Int.back };
                foreach (var dir in dirs)
                {
                    var next = cell + dir;
                    if (next.x >= 0 && next.x < GridSize && next.z >= 0 && next.z < GridSize && !available.Contains(next) && !shape.Contains(next))
                        available.Add(next);
                }
            }
            return shape.ToList();
        }

        private bool[] GenerateFloor(CustomGenerationSettings settings, List<Vector3Int> shape)
        {
            var floor = new bool[CellCount];
            Array.Fill(floor, true);

            if (settings.HasFloorHoles)
            {
                var holesCount = UnityEngine.Random.Range(3, 8);
                var available = new List<int>();
                for (var i = 0; i < CellCount; i++)
                {
                    var x = i / GridSize;
                    var z = i % GridSize;
                    var hasBlockAbove = false;
                    for (var y = 0; y < GridSize; y++)
                    {
                        if (shape.Contains(new Vector3Int(x, y, z)))
                        {
                            hasBlockAbove = true;
                            break;
                        }
                    }
                    if (!hasBlockAbove) available.Add(i);
                }

                for (var i = 0; i < holesCount && available.Count > 0; i++)
                {
                    var idx = UnityEngine.Random.Range(0, available.Count);
                    floor[available[idx]] = false;
                    available.RemoveAt(idx);
                }
            }
            return floor;
        }

        private bool IsConnected(List<Vector3Int> shape)
        {
            if (shape.Count == 0) return false;
            var visited = new HashSet<Vector3Int>();
            var queue = new Queue<Vector3Int>();
            queue.Enqueue(shape[0]);
            visited.Add(shape[0]);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var dirs = new[] { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right, Vector3Int.forward, Vector3Int.back };
                foreach (var dir in dirs)
                {
                    var next = current + dir;
                    if (shape.Contains(next) && visited.Add(next))
                        queue.Enqueue(next);
                }
            }
            return visited.Count == shape.Count;
        }

        private bool HasMultipleSolutions(bool[] wall1, bool[] wall2, bool[] floor, CustomGenerationSettings settings)
        {
            if (settings.UseDensity) return false;

            var w1Count = new int[CellCount];
            var w2Count = new int[CellCount];
            var grid = new bool[GridSize, GridSize, GridSize];
            var solutionsFound = 0;

            Solve(wall1, wall2, floor, grid, w1Count, w2Count, 0, ref solutionsFound);
            return solutionsFound > 1;
        }

        private void Solve(bool[] wall1, bool[] wall2, bool[] floor, bool[,,] grid, int[] w1Count, int[] w2Count, int index, ref int solutionsFound)
        {
            if (solutionsFound > 1) return;
            if (index == TotalCells)
            {
                for (var i = 0; i < CellCount; i++)
                {
                    if ((w1Count[i] > 0) != wall1[i]) return;
                    if ((w2Count[i] > 0) != wall2[i]) return;
                }
                solutionsFound++;
                return;
            }

            var x = index / (GridSize * GridSize);
            var rem = index % (GridSize * GridSize);
            var y = rem / GridSize;
            var z = rem % GridSize;

            var canPlace = floor[x * GridSize + z] && wall1[y * GridSize + z] && wall2[x * GridSize + y];
            if (y > 0 && !grid[x, y - 1, z]) canPlace = false;

            if (canPlace)
            {
                grid[x, y, z] = true;
                w1Count[y * GridSize + z]++;
                w2Count[x * GridSize + y]++;
                Solve(wall1, wall2, floor, grid, w1Count, w2Count, index + 1, ref solutionsFound);
                grid[x, y, z] = false;
                w1Count[y * GridSize + z]--;
                w2Count[x * GridSize + y]--;
            }

            var isLastX = (x == GridSize - 1);
            if (isLastX && wall1[y * GridSize + z] && w1Count[y * GridSize + z] == 0) return;

            var isLastZ = (z == GridSize - 1);
            if (isLastZ && wall2[x * GridSize + y] && w2Count[x * GridSize + y] == 0) return;

            Solve(wall1, wall2, floor, grid, w1Count, w2Count, index + 1, ref solutionsFound);
        }

        private (WallCellDensityData[] WallYZ, WallCellDensityData[] WallXY) CalculateDensities(List<Vector3Int> shape, CustomGenerationSettings settings)
        {
            var yz = new WallCellDensityData[CellCount];
            var xy = new WallCellDensityData[CellCount];

            if (!settings.UseDensity) return (yz, xy);

            for (var i = 0; i < CellCount; i++)
            {
                var y = i / GridSize;
                var z = i % GridSize;
                var countYZ = 0;
                for (var x = 0; x < GridSize; x++) if (shape.Contains(new Vector3Int(x, y, z))) countYZ++;
                yz[i] = new WallCellDensityData(countYZ > 0, countYZ);

                var x2 = i / GridSize;
                var y2 = i % GridSize;
                var countXY = 0;
                for (var z2 = 0; z2 < GridSize; z2++) if (shape.Contains(new Vector3Int(x2, y2, z2))) countXY++;
                xy[i] = new WallCellDensityData(countXY > 0, countXY);
            }
            return (yz, xy);
        }

        private void ApplyFloor(bool[] floor)
        {
            for (var i = 0; i < CellCount; i++)
            {
                var x = i / GridSize;
                var z = i % GridSize;
                _gridService.SetFloorExists(new Vector2Int(x, z), floor[i]);
            }
        }

        private void ApplyDensitiesToService((WallCellDensityData[] WallYZ, WallCellDensityData[] WallXY) densities)
        {
            for (var i = 0; i < CellCount; i++)
            {
                _densityService.SetDensityEnabled(0, i, densities.WallYZ[i].IsDensityEnabled);
                _densityService.SetDensityEnabled(1, i, densities.WallXY[i].IsDensityEnabled);
            }
        }
    }
}