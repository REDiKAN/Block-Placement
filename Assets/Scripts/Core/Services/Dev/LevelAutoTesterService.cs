using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using Zenject;
using Game.Data;
using Game.Services.Animation;
using Game.Services.Grid;
using Game.Services.Pool;
using Game.Services.Registry;
using Game.Services.Shadow;
using Game.Services.Time;
using Game.Views;

namespace Game.Services.Dev
{
    public class LevelAutoTesterService : ILevelAutoTesterService, IDisposable
    {
        public IReadOnlyReactiveProperty<AutoTesterState> State => _state;
        public IReadOnlyReactiveProperty<int> SpawnedCount => _spawnedCount;
        public IReadOnlyReactiveProperty<int> TotalRequired => _totalRequired;
        public IReadOnlyReactiveProperty<string> ObjectLabel => _objectLabel;
        public IReadOnlyReactiveProperty<int> ConfiguredLimit => _configuredLimit;
        public IReadOnlyReactiveProperty<bool> IsBlockLimitEnabled => _isBlockLimitEnabled;
        public IReadOnlyReactiveProperty<bool> IsTimeLimitEnabled => _isTimeLimitEnabled;
        public IReadOnlyReactiveProperty<float> RemainingTime => _remainingTime;

        private readonly ReactiveProperty<AutoTesterState> _state = new(AutoTesterState.Idle);
        private readonly ReactiveProperty<int> _spawnedCount = new();
        private readonly ReactiveProperty<int> _totalRequired = new();
        private readonly ReactiveProperty<string> _objectLabel = new();
        private readonly ReactiveProperty<int> _configuredLimit = new();
        private readonly ReactiveProperty<bool> _isBlockLimitEnabled = new();
        private readonly ReactiveProperty<bool> _isTimeLimitEnabled = new();
        private readonly ReactiveProperty<float> _remainingTime = new();

        private readonly CompositeDisposable _disposables = new();
        private readonly CompositeDisposable _testDisposables = new();

        private readonly LevelConfig _levelConfig;
        private readonly IBlockPoolService _blockPoolService;
        private readonly IStructurePoolService _structurePoolService;
        private readonly IGridService _gridService;
        private readonly IObjectRegistryService _objectRegistryService;
        private readonly IBlockAnimationService _blockAnimationService;
        private readonly IStructureAnimationService _structureAnimationService;
        private readonly IShadowValidationService _shadowValidationService;
        private readonly ITimeLimitService _timeLimitService;
        private readonly ITargetDensityProjectionService _projectionService;

        private float _delay;
        private List<Vector3Int> _blockQueue;

        private int _currentIndex;
        private bool _isWaitingForAnimation;

        private readonly Dictionary<StructureConfig, int> _availableStructureCounts = new();
        private readonly List<StructurePlacement> _placedStructures = new();
        private WallCellDensityData[] _targetWallYZDensities;
        private WallCellDensityData[] _targetWallXYDensities;
        private bool[] _targetWallYZ;
        private bool[] _targetWallXY;

        private const int GridSize = 5;
        private const int CellCount = 25;
        private const int MaxBacktrackDepth = 3;
        private const float ViolationPenalty = 10000f;
        private const float DensityReward = 100f;
        private const float BoolReward = 500f;
        private static readonly int[] ValidAngles = { 0, 90, 180, 270 };
        private static readonly Vector3Int[] Directions =
        {
            Vector3Int.up, Vector3Int.down,
            Vector3Int.left, Vector3Int.right,
            Vector3Int.forward, Vector3Int.back
        };

        public LevelAutoTesterService(
            LevelConfig levelConfig,
            IBlockPoolService blockPoolService,
            IStructurePoolService structurePoolService,
            IGridService gridService,
            IObjectRegistryService objectRegistryService,
            IBlockAnimationService blockAnimationService,
            IStructureAnimationService structureAnimationService,
            IShadowValidationService shadowValidationService,
            ITimeLimitService timeLimitService,
            ITargetDensityProjectionService projectionService)
        {
            _levelConfig = levelConfig;
            _blockPoolService = blockPoolService;
            _structurePoolService = structurePoolService;
            _gridService = gridService;
            _objectRegistryService = objectRegistryService;
            _blockAnimationService = blockAnimationService;
            _structureAnimationService = structureAnimationService;
            _shadowValidationService = shadowValidationService;
            _timeLimitService = timeLimitService;
            _projectionService = projectionService;

            _shadowValidationService.OnLevelCompleted
                .Subscribe(_ => StopTest())
                .AddTo(_disposables);

            _timeLimitService.OnTimeExpired
                .Subscribe(_ => StopTest())
                .AddTo(_disposables);

            _timeLimitService.RemainingTime
                .Subscribe(time => _remainingTime.Value = time)
                .AddTo(_disposables);
        }

        public void StartTest(float delay)
        {
            if (_state.Value != AutoTesterState.Idle) return;
            if (_levelConfig is null) return;

            _delay = delay;
            InitializeQueues();
            if (IsEmptyQueue()) return;

            _state.Value = AutoTesterState.Running;

            if (_levelConfig.Mode == GameMode.Structures)
            {
                InitializeStructureSolver();
            }

            ScheduleNext();
        }

        public void PauseTest()
        {
            if (_state.Value != AutoTesterState.Running) return;
            _state.Value = AutoTesterState.Paused;
            _testDisposables.Clear();
        }

        public void ResumeTest()
        {
            if (_state.Value != AutoTesterState.Paused) return;
            _state.Value = AutoTesterState.Running;
            ScheduleNext();
        }

        public void StepTest()
        {
            if (_state.Value == AutoTesterState.Idle)
            {
                if (_levelConfig is null) return;
                _delay = 0f;
                InitializeQueues();
                if (IsEmptyQueue()) return;

                _state.Value = AutoTesterState.Paused;
                if (_levelConfig.Mode == GameMode.Structures)
                {
                    InitializeStructureSolver();
                }
                ExecuteSpawn();
            }
            else if (_state.Value == AutoTesterState.Paused)
            {
                ExecuteSpawn();
            }
        }

        public void StopTest()
        {
            if (_state.Value == AutoTesterState.Idle) return;

            _state.Value = AutoTesterState.Idle;
            _testDisposables.Clear();
            _isWaitingForAnimation = false;
            _blockQueue?.Clear();
            _availableStructureCounts.Clear();
            _placedStructures.Clear();
            _targetWallYZ = null;
            _targetWallXY = null;
        }

        private void InitializeQueues()
        {
            _currentIndex = 0;
            _spawnedCount.Value = 0;
            _blockQueue?.Clear();
            _availableStructureCounts.Clear();
            _placedStructures.Clear();
            _isBlockLimitEnabled.Value = _levelConfig.IsBlockLimitEnabled;
            _isTimeLimitEnabled.Value = _levelConfig.IsTimeLimitEnabled;

            if (_levelConfig.Mode == GameMode.Blocks)
            {
                _objectLabel.Value = "Blocks";
                var blocks = _levelConfig.InitialBlocks;
                if (blocks is null || blocks.Length == 0) return;

                _totalRequired.Value = blocks.Length;
                _configuredLimit.Value = _levelConfig.IsBlockLimitEnabled ? _levelConfig.MaxBlocks : -1;

                var limit = _levelConfig.IsBlockLimitEnabled && _levelConfig.MaxBlocks > 0 ? _levelConfig.MaxBlocks : int.MaxValue;
                _blockQueue = blocks.OrderBy(b => b.y).ThenBy(b => b.x).ThenBy(b => b.z).Take(limit).ToList();
            }
            else if (_levelConfig.Mode == GameMode.Structures)
            {
                _objectLabel.Value = "Structures";
                var structures = _levelConfig.AvailableStructures;
                if (structures is null || structures.Length == 0) return;

                var totalCount = 0;
                foreach (var data in structures)
                {
                    if (data is null || data.Config is null) continue;
                    var count = data.MaxCount > 0 ? data.MaxCount : 1;
                    _availableStructureCounts[data.Config] = count;
                    totalCount += count;
                }

                _totalRequired.Value = totalCount;
                _configuredLimit.Value = totalCount;
            }
        }

        private void InitializeStructureSolver()
        {
            _targetWallYZDensities = _projectionService.GetCurrentDensities(0);
            _targetWallXYDensities = _projectionService.GetCurrentDensities(1);

            _targetWallYZ = new bool[CellCount];
            _targetWallXY = new bool[CellCount];

            if (_levelConfig?.InitialBlocks != null)
            {
                foreach (var block in _levelConfig.InitialBlocks)
                {
                    if (_gridService.IsWithinBounds(block))
                    {
                        _targetWallYZ[block.y * GridSize + block.z] = true;
                        _targetWallXY[block.x * GridSize + block.y] = true;
                    }
                }
            }
        }

        private bool IsEmptyQueue() =>
            (_levelConfig.Mode == GameMode.Blocks && (_blockQueue is null || _blockQueue.Count == 0)) ||
            (_levelConfig.Mode == GameMode.Structures && _availableStructureCounts.Count == 0);

        private void ScheduleNext()
        {
            if (_state.Value != AutoTesterState.Running) return;
            if (_isWaitingForAnimation) return;

            if (_levelConfig.Mode == GameMode.Blocks)
            {
                if (_currentIndex >= _blockQueue.Count)
                {
                    StopTest();
                    return;
                }
            }
            else
            {
                if (_availableStructureCounts.All(kvp => kvp.Value <= 0))
                {
                    StopTest();
                    return;
                }
            }

            _testDisposables.Clear();
            Observable.Timer(TimeSpan.FromSeconds(_delay))
                .Subscribe(_ => ExecuteSpawn())
                .AddTo(_testDisposables);
        }

        private void ExecuteSpawn()
        {
            if (_state.Value == AutoTesterState.Idle) return;
            if (_isWaitingForAnimation) return;

            _isWaitingForAnimation = true;

            if (_levelConfig.Mode == GameMode.Blocks)
            {
                ExecuteBlockSpawn();
            }
            else if (_levelConfig.Mode == GameMode.Structures)
            {
                ExecuteStructureSpawn();
            }
        }

        private void ExecuteBlockSpawn()
        {
            var cell = _blockQueue[_currentIndex];
            _currentIndex++;

            if (_gridService.IsCellOccupied(cell))
            {
                _isWaitingForAnimation = false;
                if (_state.Value == AutoTesterState.Running) ScheduleNext();
                return;
            }

            var block = _blockPoolService.GetDefault();
            if (block is null)
            {
                _isWaitingForAnimation = false;
                if (_state.Value == AutoTesterState.Running) ScheduleNext();
                return;
            }

            _gridService.SetCellOccupied(cell, true);
            block.SetPosition(cell);
            _objectRegistryService.Register(new PlacedObjectData(PlacedObjectType.Block, cell, "AutoTestBlock"));
            _spawnedCount.Value++;

            _blockAnimationService.AnimateSpawn(block, () =>
            {
                _shadowValidationService.ForceRevalidate();
                _isWaitingForAnimation = false;
                if (_state.Value == AutoTesterState.Running) ScheduleNext();
            });
        }

        private void ExecuteStructureSpawn()
        {
            var bestPlacement = FindBestStructurePlacementWithBacktracking();

            if (bestPlacement == null)
            {
                Debug.LogWarning("[AutoTester] No valid structure placement found. Stopping test.");
                _isWaitingForAnimation = false;
                StopTest();
                return;
            }

            var structure = _structurePoolService.Get(bestPlacement.Config);
            if (structure is null)
            {
                _isWaitingForAnimation = false;
                if (_state.Value == AutoTesterState.Running) ScheduleNext();
                return;
            }

            structure.SetPosition(bestPlacement.OriginCell);
            structure.transform.rotation = Quaternion.Euler(0f, bestPlacement.Angle, 0f);

            var worldCells = new List<Vector3Int>();
            foreach (var local in bestPlacement.RotatedLocalCoords)
            {
                var worldCell = bestPlacement.OriginCell + local;
                if (_gridService.IsWithinBounds(worldCell))
                {
                    _gridService.SetCellOccupied(worldCell, true);
                    _objectRegistryService.Register(new PlacedObjectData(PlacedObjectType.Block, worldCell, bestPlacement.Config.DisplayName));
                    worldCells.Add(worldCell);
                }
            }

            _placedStructures.Add(new StructurePlacement
            {
                Config = bestPlacement.Config,
                OriginCell = bestPlacement.OriginCell,
                Angle = bestPlacement.Angle,
                RotatedLocalCoords = bestPlacement.RotatedLocalCoords,
                WorldCells = worldCells.ToArray(),
                View = structure
            });

            _availableStructureCounts[bestPlacement.Config]--;
            _spawnedCount.Value++;

            _structureAnimationService.AnimateSpawn(structure, () =>
            {
                _shadowValidationService.ForceRevalidate();
                _isWaitingForAnimation = false;
                if (_state.Value == AutoTesterState.Running) ScheduleNext();
            });
        }

        private StructurePlacementCandidate FindBestStructurePlacementWithBacktracking()
        {
            var candidates = GenerateAllValidPlacements();

            if (candidates.Count == 0)
            {
                return TryBacktrackAndFindPlacement();
            }

            candidates.Sort((a, b) => a.Score.CompareTo(b.Score));

            foreach (var candidate in candidates)
            {
                if (candidate.Score < 5000f && CanPlaceRemainingStructures(candidate))
                {
                    return candidate;
                }
            }

            return candidates[0];
        }

        private StructurePlacementCandidate TryBacktrackAndFindPlacement()
        {
            if (_placedStructures.Count == 0) return null;

            var backtrackCount = Mathf.Min(_placedStructures.Count, MaxBacktrackDepth);
            var undonePlacements = new List<StructurePlacement>();

            for (var i = 0; i < backtrackCount; i++)
            {
                var lastPlacement = _placedStructures[_placedStructures.Count - 1];
                UndoPlacement(lastPlacement);
                undonePlacements.Add(lastPlacement);

                var candidates = GenerateAllValidPlacements();
                if (candidates.Count > 0)
                {
                    candidates.Sort((a, b) => a.Score.CompareTo(b.Score));
                    return candidates[0];
                }
            }

            foreach (var placement in undonePlacements.AsEnumerable().Reverse())
            {
                RedoPlacement(placement);
            }

            return null;
        }

        private void UndoPlacement(StructurePlacement placement)
        {
            foreach (var cell in placement.WorldCells)
            {
                _gridService.SetCellOccupied(cell, false);
                _objectRegistryService.Unregister(cell, PlacedObjectType.Block);
            }
            if (placement.View != null)
            {
                _structurePoolService.Return(placement.View);
            }
            _availableStructureCounts[placement.Config]++;
            _placedStructures.Remove(placement);
        }

        private void RedoPlacement(StructurePlacement placement)
        {
            foreach (var cell in placement.WorldCells)
            {
                _gridService.SetCellOccupied(cell, true);
                _objectRegistryService.Register(new PlacedObjectData(PlacedObjectType.Block, cell, placement.Config.DisplayName));
            }
            var structure = _structurePoolService.Get(placement.Config);
            if (structure != null)
            {
                structure.SetPosition(placement.OriginCell);
                structure.transform.rotation = Quaternion.Euler(0f, placement.Angle, 0f);
                placement.View = structure;
            }
            _availableStructureCounts[placement.Config]--;
            _placedStructures.Add(placement);
        }

        private List<StructurePlacementCandidate> GenerateAllValidPlacements()
        {
            var candidates = new List<StructurePlacementCandidate>();

            var availableConfigs = _availableStructureCounts
                .Where(kvp => kvp.Value > 0)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var config in availableConfigs)
            {
                foreach (var angle in GetUniqueAngles(config))
                {
                    var rotatedCoords = GetRotatedLocalCoordinates(config.LocalCoordinates, angle);

                    for (var x = 0; x < GridSize; x++)
                    {
                        for (var z = 0; z < GridSize; z++)
                        {
                            var origin = new Vector3Int(x, 0, z);

                            if (!ValidatePlacement(origin, rotatedCoords)) continue;

                            var score = EvaluatePlacement(origin, rotatedCoords);

                            candidates.Add(new StructurePlacementCandidate
                            {
                                Config = config,
                                OriginCell = origin,
                                Angle = angle,
                                RotatedLocalCoords = rotatedCoords,
                                Score = score
                            });
                        }
                    }
                }
            }

            return candidates;
        }

        private int[] GetUniqueAngles(StructureConfig config)
        {
            if (config.LocalCoordinates is null || config.LocalCoordinates.Length == 0)
                return Array.Empty<int>();

            var uniqueAngles = new List<int>();
            var uniqueSignatures = new HashSet<string>();

            foreach (var angle in ValidAngles)
            {
                var rotated = GetRotatedLocalCoordinates(config.LocalCoordinates, angle);
                var signature = string.Join(",", rotated.Select(c => $"{c.x},{c.y},{c.z}").OrderBy(s => s));

                if (uniqueSignatures.Add(signature))
                {
                    uniqueAngles.Add(angle);
                }
            }

            return uniqueAngles.ToArray();
        }

        private float EvaluatePlacement(Vector3Int origin, Vector3Int[] rotatedCoords)
        {
            var baseGrid = GetCurrentGrid();
            var simulatedGrid = (bool[,,])baseGrid.Clone();

            foreach (var local in rotatedCoords)
            {
                var worldCell = origin + local;
                if (_gridService.IsWithinBounds(worldCell))
                {
                    simulatedGrid[worldCell.x, worldCell.y, worldCell.z] = true;
                }
            }

            var score = 0f;

            for (var i = 0; i < CellCount; i++)
            {
                var yzData = GetDensityData(_targetWallYZDensities, i);
                var xyData = GetDensityData(_targetWallXYDensities, i);

                if (yzData.IsDensityEnabled)
                {
                    var currentDensity = CalculateDensityFromGrid(0, i, baseGrid);
                    var newDensity = CalculateDensityFromGrid(0, i, simulatedGrid);
                    var targetDensity = yzData.TargetDensity;

                    if (newDensity > targetDensity)
                    {
                        score += ViolationPenalty * (newDensity - targetDensity);
                    }
                    else if (newDensity > currentDensity)
                    {
                        score -= (newDensity - currentDensity) * DensityReward;
                    }
                }
                else
                {
                    var isTarget = _targetWallYZ[i];
                    var currentDensity = CalculateDensityFromGrid(0, i, baseGrid);
                    var newDensity = CalculateDensityFromGrid(0, i, simulatedGrid);
                    var currentHasShadow = currentDensity > 0;
                    var newHasShadow = newDensity > 0;

                    if (newHasShadow && !currentHasShadow)
                    {
                        if (!isTarget)
                        {
                            score += ViolationPenalty;
                        }
                        else
                        {
                            score -= BoolReward;
                        }
                    }
                }

                if (xyData.IsDensityEnabled)
                {
                    var currentDensity = CalculateDensityFromGrid(1, i, baseGrid);
                    var newDensity = CalculateDensityFromGrid(1, i, simulatedGrid);
                    var targetDensity = xyData.TargetDensity;

                    if (newDensity > targetDensity)
                    {
                        score += ViolationPenalty * (newDensity - targetDensity);
                    }
                    else if (newDensity > currentDensity)
                    {
                        score -= (newDensity - currentDensity) * DensityReward;
                    }
                }
                else
                {
                    var isTarget = _targetWallXY[i];
                    var currentDensity = CalculateDensityFromGrid(1, i, baseGrid);
                    var newDensity = CalculateDensityFromGrid(1, i, simulatedGrid);
                    var currentHasShadow = currentDensity > 0;
                    var newHasShadow = newDensity > 0;

                    if (newHasShadow && !currentHasShadow)
                    {
                        if (!isTarget)
                        {
                            score += ViolationPenalty;
                        }
                        else
                        {
                            score -= BoolReward;
                        }
                    }
                }
            }

            return score;
        }

        private int CalculateDensityFromGrid(int wallIndex, int cellIndex, bool[,,] grid)
        {
            var density = 0;

            if (wallIndex == 0)
            {
                var y = cellIndex / GridSize;
                var z = cellIndex % GridSize;
                for (var x = 0; x < GridSize; x++)
                {
                    if (grid[x, y, z])
                        density++;
                }
            }
            else
            {
                var x = cellIndex / GridSize;
                var y = cellIndex % GridSize;
                for (var z = 0; z < GridSize; z++)
                {
                    if (grid[x, y, z])
                        density++;
                }
            }

            return density;
        }

        private WallCellDensityData GetDensityData(WallCellDensityData[] densities, int index)
        {
            if (densities is null || index >= densities.Length)
                return default;
            return densities[index];
        }

        private bool CanPlaceRemainingStructures(StructurePlacementCandidate candidate)
        {
            var simulatedGrid = GetCurrentGrid();

            foreach (var local in candidate.RotatedLocalCoords)
            {
                var worldCell = candidate.OriginCell + local;
                if (_gridService.IsWithinBounds(worldCell))
                {
                    simulatedGrid[worldCell.x, worldCell.y, worldCell.z] = true;
                }
            }

            var remainingCounts = new Dictionary<StructureConfig, int>(_availableStructureCounts);
            remainingCounts[candidate.Config]--;

            foreach (var kvp in remainingCounts)
            {
                if (kvp.Value <= 0) continue;

                var config = kvp.Key;
                var countNeeded = kvp.Value;

                var tempGrid = (bool[,,])simulatedGrid.Clone();

                for (var placed = 0; placed < countNeeded; placed++)
                {
                    var canPlace = false;
                    foreach (var angle in GetUniqueAngles(config))
                    {
                        var rotatedCoords = GetRotatedLocalCoordinates(config.LocalCoordinates, angle);

                        for (var x = 0; x < GridSize && !canPlace; x++)
                        {
                            for (var z = 0; z < GridSize && !canPlace; z++)
                            {
                                var origin = new Vector3Int(x, 0, z);
                                if (ValidatePlacementInGrid(origin, rotatedCoords, tempGrid))
                                {
                                    foreach (var local in rotatedCoords)
                                    {
                                        var worldCell = origin + local;
                                        if (_gridService.IsWithinBounds(worldCell))
                                        {
                                            tempGrid[worldCell.x, worldCell.y, worldCell.z] = true;
                                        }
                                    }
                                    canPlace = true;
                                }
                            }
                        }
                        if (canPlace) break;
                    }
                    if (!canPlace) return false;
                }
            }
            return true;
        }

        private bool ValidatePlacementInGrid(Vector3Int origin, Vector3Int[] localCoords, bool[,,] grid)
        {
            if (localCoords is null || localCoords.Length == 0) return false;

            var hasConnection = false;
            foreach (var local in localCoords)
            {
                var worldCell = origin + local;
                if (!_gridService.IsWithinBounds(worldCell)) return false;
                if (grid[worldCell.x, worldCell.y, worldCell.z]) return false;

                if (worldCell.y == 0)
                {
                    if (!_gridService.IsFloorExists(new Vector2Int(worldCell.x, worldCell.z))) return false;
                    hasConnection = true;
                }

                if (!hasConnection)
                {
                    foreach (var dir in Directions)
                    {
                        var neighbor = worldCell + dir;
                        if (_gridService.IsWithinBounds(neighbor) && grid[neighbor.x, neighbor.y, neighbor.z])
                        {
                            hasConnection = true;
                            break;
                        }
                    }
                }
            }
            return hasConnection;
        }

        private bool[,,] GetCurrentGrid()
        {
            var grid = new bool[GridSize, GridSize, GridSize];
            for (var x = 0; x < GridSize; x++)
                for (var y = 0; y < GridSize; y++)
                    for (var z = 0; z < GridSize; z++)
                    {
                        grid[x, y, z] = _gridService.IsCellOccupied(new Vector3Int(x, y, z));
                    }
            return grid;
        }

        private bool ValidatePlacement(Vector3Int origin, Vector3Int[] localCoords)
        {
            if (localCoords is null || localCoords.Length == 0) return false;

            var hasConnection = false;
            foreach (var local in localCoords)
            {
                var worldCell = origin + local;
                if (!_gridService.IsWithinBounds(worldCell)) return false;
                if (_gridService.IsCellOccupied(worldCell)) return false;

                if (worldCell.y == 0)
                {
                    if (!_gridService.IsFloorExists(new Vector2Int(worldCell.x, worldCell.z))) return false;
                    hasConnection = true;
                }

                if (!hasConnection)
                {
                    foreach (var dir in Directions)
                    {
                        var neighbor = worldCell + dir;
                        if (_gridService.IsWithinBounds(neighbor) && _gridService.IsCellOccupied(neighbor))
                        {
                            hasConnection = true;
                            break;
                        }
                    }
                }
            }
            return hasConnection;
        }

        private Vector3Int[] GetRotatedLocalCoordinates(Vector3Int[] localCoords, int angle)
        {
            if (localCoords is null || localCoords.Length == 0) return Array.Empty<Vector3Int>();
            if (angle == 0) return localCoords;

            var rotated = new Vector3Int[localCoords.Length];
            for (var i = 0; i < localCoords.Length; i++)
            {
                var local = localCoords[i];
                rotated[i] = angle switch
                {
                    90 => new Vector3Int(local.z, local.y, -local.x),
                    180 => new Vector3Int(-local.x, local.y, -local.z),
                    270 => new Vector3Int(-local.z, local.y, local.x),
                    _ => local
                };
            }
            return rotated;
        }

        public void Dispose()
        {
            StopTest();
            _disposables?.Dispose();
            _testDisposables?.Dispose();
        }

        private class StructurePlacement
        {
            public StructureConfig Config;
            public Vector3Int OriginCell;
            public int Angle;
            public Vector3Int[] RotatedLocalCoords;
            public Vector3Int[] WorldCells;
            public StructureView View;
        }

        private class StructurePlacementCandidate
        {
            public StructureConfig Config;
            public Vector3Int OriginCell;
            public int Angle;
            public Vector3Int[] RotatedLocalCoords;
            public float Score;
        }
    }
}