using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;
using Game.Services.Grid;
using Game.Services.Pool;
using Game.Services.Registry;
using Game.Services.Shadow;
using Game.Views;
using Game.Data;

namespace Game.Services.Dev
{
    public class LevelGeneratorService : ILevelGeneratorService, IDisposable
    {
        public IObservable<(int Current, int Total)> OnProgress => _onProgress;
        public IObservable<bool> OnGenerationCompleted => _onGenerationCompleted;
        public IObservable<bool> OnValidationCompleted => _onValidationCompleted;

        private readonly Subject<(int Current, int Total)> _onProgress = new();
        private readonly Subject<bool> _onGenerationCompleted = new();
        private readonly Subject<bool> _onValidationCompleted = new();
        private readonly CompositeDisposable _disposables = new();

        private readonly IGridService _gridService;
        private readonly IBlockPoolService _poolService;
        private readonly IObjectRegistryService _registryService;
        private readonly IShadowCalculationService _calculationService;
        private readonly IShadowDensityService _densityService;
        private readonly ITargetDensityProjectionService _projectionService;
        private readonly IDevModeService _devModeService;

        private readonly List<BlockView> _spawnedBlocks = new();

        private const int GridSize = 5;
        private const int TotalCells = 125;

        public LevelGeneratorService(
            IGridService gridService,
            IBlockPoolService poolService,
            IObjectRegistryService registryService,
            IShadowCalculationService calculationService,
            IShadowDensityService densityService,
            ITargetDensityProjectionService projectionService,
            IDevModeService devModeService)
        {
            _gridService = gridService;
            _poolService = poolService;
            _registryService = registryService;
            _calculationService = calculationService;
            _densityService = densityService;
            _projectionService = projectionService;
            _devModeService = devModeService;
        }

        public void Generate(int difficulty, int strategy)
        {
            ClearLevel();
            var grid = strategy == 0 ? GenerateAdditive(difficulty) : GenerateSubtractive(difficulty);
            ApplyGridToServices(grid);
            _onGenerationCompleted.OnNext(true);
        }

        public void ValidateSolvability()
        {
            var grid = GetCurrentGrid();
            var isSolvable = CheckSolvability(grid, (current, total) => _onProgress.OnNext((current, total)));
            _onValidationCompleted.OnNext(isSolvable);
        }

        private void ClearLevel()
        {
            for (var x = 0; x < GridSize; x++)
                for (var y = 0; y < GridSize; y++)
                    for (var z = 0; z < GridSize; z++)
                    {
                        var cell = new Vector3Int(x, y, z);
                        if (_gridService.IsCellOccupied(cell))
                        {
                            _gridService.SetCellOccupied(cell, false);
                            _registryService.Unregister(cell, PlacedObjectType.Block);
                        }
                    }

            foreach (var block in _spawnedBlocks)
            {
                if (block is not null)
                    _poolService.Return(block);
            }
            _spawnedBlocks.Clear();
        }

        private bool[,,] GenerateAdditive(int difficulty)
        {
            var grid = new bool[GridSize, GridSize, GridSize];
            var budget = 10 + (int)(difficulty * 5f);
            var visited = new HashSet<Vector3Int>();
            var frontier = new List<Vector3Int>();

            for (var x = 0; x < GridSize; x++)
                for (var z = 0; z < GridSize; z++)
                {
                    if (_gridService.IsFloorExists(new Vector2Int(x, z)))
                    {
                        var cell = new Vector3Int(x, 0, z);
                        visited.Add(cell);
                        frontier.Add(cell);
                        grid[x, 0, z] = true;
                    }
                }

            if (frontier.Count == 0) return grid;

            while (visited.Count < budget && frontier.Count > 0)
            {
                var index = UnityEngine.Random.Range(0, frontier.Count);
                var current = frontier[index];
                frontier.RemoveAt(index);

                var verticalBias = difficulty / 10f;
                var neighbors = new List<Vector3Int>();

                if (current.y + 1 < GridSize && !visited.Contains(new Vector3Int(current.x, current.y + 1, current.z)))
                    neighbors.Add(new Vector3Int(current.x, current.y + 1, current.z));

                var horizontalNeighbors = new List<Vector3Int>();
                if (current.x + 1 < GridSize && !visited.Contains(new Vector3Int(current.x + 1, current.y, current.z)))
                    horizontalNeighbors.Add(new Vector3Int(current.x + 1, current.y, current.z));
                if (current.x - 1 >= 0 && !visited.Contains(new Vector3Int(current.x - 1, current.y, current.z)))
                    horizontalNeighbors.Add(new Vector3Int(current.x - 1, current.y, current.z));
                if (current.z + 1 < GridSize && !visited.Contains(new Vector3Int(current.x, current.y, current.z + 1)))
                    horizontalNeighbors.Add(new Vector3Int(current.x, current.y, current.z + 1));
                if (current.z - 1 >= 0 && !visited.Contains(new Vector3Int(current.x, current.y, current.z - 1)))
                    horizontalNeighbors.Add(new Vector3Int(current.x, current.y, current.z - 1));

                Vector3Int? nextCell = null;
                if (neighbors.Count > 0 && UnityEngine.Random.value < verticalBias)
                    nextCell = neighbors[0];
                else if (horizontalNeighbors.Count > 0)
                    nextCell = horizontalNeighbors[UnityEngine.Random.Range(0, horizontalNeighbors.Count)];
                else if (neighbors.Count > 0)
                    nextCell = neighbors[0];

                if (nextCell.HasValue)
                {
                    var n = nextCell.Value;
                    visited.Add(n);
                    frontier.Add(n);
                    grid[n.x, n.y, n.z] = true;
                }
            }

            return grid;
        }

        private bool[,,] GenerateSubtractive(int difficulty)
        {
            var removalPercent = Mathf.Lerp(0.1f, 0.5f, difficulty / 10f);
            var blocksToRemove = (int)(TotalCells * removalPercent);

            for (var attempt = 1; attempt <= 10; attempt++)
            {
                var grid = new bool[GridSize, GridSize, GridSize];
                for (var x = 0; x < GridSize; x++)
                    for (var y = 0; y < GridSize; y++)
                        for (var z = 0; z < GridSize; z++)
                            grid[x, y, z] = true;

                var removed = 0;
                while (removed < blocksToRemove)
                {
                    var x = UnityEngine.Random.Range(0, GridSize);
                    var y = UnityEngine.Random.Range(0, GridSize);
                    var z = UnityEngine.Random.Range(0, GridSize);
                    if (grid[x, y, z])
                    {
                        grid[x, y, z] = false;
                        removed++;
                    }
                }

                if (CheckSolvability(grid, null))
                {
                    Debug.Log($"[Generator] Success on attempt {attempt}");
                    return grid;
                }

                Debug.Log($"[Generator] Attempt {attempt} failed. Retrying...");
            }

            Debug.LogError("[Generator] Failed to generate solvable level.");
            return new bool[GridSize, GridSize, GridSize];
        }

        private bool CheckSolvability(bool[,,] grid, Action<int, int> progressCallback)
        {
            var visited = new HashSet<Vector3Int>();
            var queue = new Queue<Vector3Int>();
            var totalBlocks = 0;

            for (var x = 0; x < GridSize; x++)
                for (var y = 0; y < GridSize; y++)
                    for (var z = 0; z < GridSize; z++)
                    {
                        if (grid[x, y, z])
                        {
                            totalBlocks++;
                            if (y == 0 && _gridService.IsFloorExists(new Vector2Int(x, z)))
                            {
                                var cell = new Vector3Int(x, y, z);
                                visited.Add(cell);
                                queue.Enqueue(cell);
                            }
                        }
                    }

            if (totalBlocks > 0 && visited.Count == 0) return false;

            var processed = 0;
            var directions = new[]
            {
                Vector3Int.up, Vector3Int.down,
                Vector3Int.left, Vector3Int.right,
                Vector3Int.forward, Vector3Int.back
            };

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                processed++;
                progressCallback?.Invoke(processed, totalBlocks);

                foreach (var dir in directions)
                {
                    var next = current + dir;
                    if (next.x >= 0 && next.x < GridSize &&
                        next.y >= 0 && next.y < GridSize &&
                        next.z >= 0 && next.z < GridSize &&
                        grid[next.x, next.y, next.z] &&
                        visited.Add(next))
                    {
                        queue.Enqueue(next);
                    }
                }
            }

            return processed == totalBlocks;
        }

        private bool[,,] GetCurrentGrid()
        {
            var grid = new bool[GridSize, GridSize, GridSize];
            for (var x = 0; x < GridSize; x++)
                for (var y = 0; y < GridSize; y++)
                    for (var z = 0; z < GridSize; z++)
                        grid[x, y, z] = _gridService.IsCellOccupied(new Vector3Int(x, y, z));
            return grid;
        }

        private void ApplyGridToServices(bool[,,] grid)
        {
            var activeConfig = _devModeService.ActiveBlockConfig.Value;

            for (var x = 0; x < GridSize; x++)
                for (var y = 0; y < GridSize; y++)
                    for (var z = 0; z < GridSize; z++)
                    {
                        if (grid[x, y, z])
                        {
                            var cell = new Vector3Int(x, y, z);
                            _gridService.SetCellOccupied(cell, true);

                            var block = activeConfig is not null ? _poolService.Get(activeConfig) : _poolService.GetDefault();
                            if (block is not null)
                            {
                                block.SetPosition(cell);
                                _spawnedBlocks.Add(block);
                                var identifier = activeConfig is not null ? activeConfig.DisplayName : "DefaultBlock";
                                _registryService.Register(new PlacedObjectData(PlacedObjectType.Block, cell, identifier));
                            }
                        }
                    }

            var yzDensities = new WallCellDensityData[25];
            var xyDensities = new WallCellDensityData[25];

            for (var i = 0; i < 25; i++)
            {
                var yzDensity = _calculationService.CalculateDensity(0, i, _gridService, GridSize);
                var xyDensity = _calculationService.CalculateDensity(1, i, _gridService, GridSize);

                var isYZEnabled = yzDensity > 0;
                var isXYEnabled = xyDensity > 0;

                yzDensities[i] = new WallCellDensityData(isYZEnabled, yzDensity);
                xyDensities[i] = new WallCellDensityData(isXYEnabled, xyDensity);

                _densityService.SetDensityEnabled(0, i, isYZEnabled);
                _densityService.SetDensityEnabled(1, i, isXYEnabled);
            }

            _projectionService.SetDensities(yzDensities, xyDensities);
        }

        public void Dispose() => _disposables?.Dispose();
    }
}