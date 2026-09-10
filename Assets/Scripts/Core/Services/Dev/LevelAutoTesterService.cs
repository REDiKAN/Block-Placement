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

        private float _delay;
        private List<Vector3Int> _blockQueue;
        private List<StructureSpawnData> _structureQueue;
        private int _currentIndex;
        private bool _isWaitingForAnimation;

        public LevelAutoTesterService(
            LevelConfig levelConfig,
            IBlockPoolService blockPoolService,
            IStructurePoolService structurePoolService,
            IGridService gridService,
            IObjectRegistryService objectRegistryService,
            IBlockAnimationService blockAnimationService,
            IStructureAnimationService structureAnimationService,
            IShadowValidationService shadowValidationService,
            ITimeLimitService timeLimitService)
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
            ScheduleNext();
        }

        public void PauseTest()
        {
            if (_state.Value == AutoTesterState.Running)
            {
                _state.Value = AutoTesterState.Paused;
                _testDisposables.Clear();
            }
        }

        public void ResumeTest()
        {
            if (_state.Value == AutoTesterState.Paused)
            {
                _state.Value = AutoTesterState.Running;
                ScheduleNext();
            }
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
            _structureQueue?.Clear();
        }

        private void InitializeQueues()
        {
            _currentIndex = 0;
            _spawnedCount.Value = 0;
            _blockQueue?.Clear();
            _structureQueue?.Clear();

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

                _structureQueue = new List<StructureSpawnData>();
                var totalCount = 0;
                foreach (var data in structures)
                {
                    if (data is null || data.Config is null) continue;
                    var count = data.MaxCount > 0 ? data.MaxCount : 1;
                    totalCount += count;
                    for (var i = 0; i < count; i++)
                        _structureQueue.Add(data);
                }
                _totalRequired.Value = totalCount;
                _configuredLimit.Value = totalCount;
            }
        }

        private bool IsEmptyQueue() =>
            (_blockQueue is null || _blockQueue.Count == 0) && (_structureQueue is null || _structureQueue.Count == 0);

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
                if (_currentIndex >= _structureQueue.Count)
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
            else if (_levelConfig.Mode == GameMode.Structures)
            {
                var spawnData = _structureQueue[_currentIndex];
                _currentIndex++;

                if (spawnData is null || spawnData.Config is null)
                {
                    _isWaitingForAnimation = false;
                    if (_state.Value == AutoTesterState.Running) ScheduleNext();
                    return;
                }

                var structure = _structurePoolService.Get(spawnData.Config);
                if (structure is null)
                {
                    _isWaitingForAnimation = false;
                    if (_state.Value == AutoTesterState.Running) ScheduleNext();
                    return;
                }

                var originCell = FindFirstValidStructurePosition(spawnData.Config);
                structure.SetPosition(originCell);

                var localCoords = spawnData.Config.LocalCoordinates ?? Array.Empty<Vector3Int>();
                foreach (var local in localCoords)
                {
                    var worldCell = originCell + local;
                    if (_gridService.IsWithinBounds(worldCell))
                    {
                        _gridService.SetCellOccupied(worldCell, true);
                        _objectRegistryService.Register(new PlacedObjectData(PlacedObjectType.Block, worldCell, spawnData.Config.DisplayName));
                    }
                }

                _spawnedCount.Value++;

                _structureAnimationService.AnimateSpawn(structure, () =>
                {
                    _shadowValidationService.ForceRevalidate();
                    _isWaitingForAnimation = false;
                    if (_state.Value == AutoTesterState.Running) ScheduleNext();
                });
            }
        }

        private Vector3Int FindFirstValidStructurePosition(StructureConfig config)
        {
            var localCoords = config.LocalCoordinates ?? Array.Empty<Vector3Int>();
            var gridSize = _gridService.GridSize;

            for (var x = 0; x < gridSize; x++)
            {
                for (var z = 0; z < gridSize; z++)
                {
                    if (!_gridService.IsFloorExists(new Vector2Int(x, z))) continue;

                    var origin = new Vector3Int(x, 0, z);
                    var isValid = true;

                    foreach (var local in localCoords)
                    {
                        var worldCell = origin + local;
                        if (!_gridService.IsWithinBounds(worldCell) || _gridService.IsCellOccupied(worldCell))
                        {
                            isValid = false;
                            break;
                        }
                        if (worldCell.y == 0 && !_gridService.IsFloorExists(new Vector2Int(worldCell.x, worldCell.z)))
                        {
                            isValid = false;
                            break;
                        }
                    }

                    if (isValid) return origin;
                }
            }
            return Vector3Int.zero;
        }

        public void Dispose()
        {
            StopTest();
            _disposables?.Dispose();
            _testDisposables?.Dispose();
        }
    }
}