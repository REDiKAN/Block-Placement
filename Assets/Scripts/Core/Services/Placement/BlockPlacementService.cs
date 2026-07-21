using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;
using Game.Data;
using Game.Services.Dev;
using Game.Services.Grid;
using Game.Services.History;
using Game.Services.Input;
using Game.Services.Pool;
using Game.Services.Raycast;
using Game.Services.Registry;
using Game.Services.Rotation;
using Game.Views;

namespace Game.Services.Placement
{
    public interface IBlockPlacementService
    {
        IObservable<Unit> OnGridChanged { get; }
        IReadOnlyReactiveProperty<(bool IsEnabled, int Remaining)> RemainingBlocks { get; }
    }

    public class BlockPlacementService : IBlockPlacementService, IInitializable, IDisposable
    {
        public IObservable<Unit> OnGridChanged => _onGridChanged;
        public IReadOnlyReactiveProperty<(bool IsEnabled, int Remaining)> RemainingBlocks => _remainingBlocks;

        private readonly Subject<Unit> _onGridChanged = new();
        private readonly ReactiveProperty<(bool IsEnabled, int Remaining)> _remainingBlocks = new();
        private readonly CompositeDisposable _disposables = new();
        private readonly Dictionary<Vector3Int, BlockView> _activeBlocks = new();

        private readonly IInputService _inputService;
        private readonly IRaycastService _raycastService;
        private readonly IGridService _gridService;
        private readonly IBlockPoolService _poolService;
        private readonly IBlockHistoryService _historyService;
        private readonly IRotationService _rotationService;
        private readonly IInputContextService _contextService;
        private readonly IDevModeService _devModeService;
        private readonly IObjectRegistryService _registryService;
        private readonly BlockView _previewBlock;
        private readonly LevelConfig _levelConfig;
        private readonly bool _isDeveloperMode;

        private Renderer _previewRenderer;
        private Color _previewDefaultColor;
        private MaterialPropertyBlock _materialPropertyBlock;
        private int _remainingBlocksCount;
        private bool _isLimitEnabled;

        private static readonly Color BlockedPreviewColor = new(1f, 0f, 0f, 0.4f);

        public BlockPlacementService(
            IInputService inputService,
            IRaycastService raycastService,
            IGridService gridService,
            IBlockPoolService poolService,
            IBlockHistoryService historyService,
            IRotationService rotationService,
            IInputContextService contextService,
            IDevModeService devModeService,
            IObjectRegistryService registryService,
            LevelConfig levelConfig,
            [Inject(Id = "IsDeveloperMode")] bool isDeveloperMode,
            [Inject(Id = "PreviewBlock")] BlockView previewBlock)
        {
            _inputService = inputService;
            _raycastService = raycastService;
            _gridService = gridService;
            _poolService = poolService;
            _historyService = historyService;
            _rotationService = rotationService;
            _contextService = contextService;
            _devModeService = devModeService;
            _registryService = registryService;
            _levelConfig = levelConfig;
            _isDeveloperMode = isDeveloperMode;
            _previewBlock = previewBlock;
        }

        public void Initialize()
        {
            _isLimitEnabled = !_isDeveloperMode && _levelConfig is not null && _levelConfig.IsBlockLimitEnabled;
            _remainingBlocksCount = _isLimitEnabled ? _levelConfig.MaxBlocks : -1;

            _materialPropertyBlock = new MaterialPropertyBlock();

            if (_previewBlock is not null)
            {
                _previewRenderer = _previewBlock.GetComponentInChildren<Renderer>();
                if (_previewRenderer is not null && _previewRenderer.sharedMaterial is not null)
                {
                    _previewDefaultColor = _previewRenderer.sharedMaterial.color;
                    _materialPropertyBlock.SetColor("_Color", _previewDefaultColor);
                    _previewRenderer.SetPropertyBlock(_materialPropertyBlock);
                }
                _previewBlock.gameObject.SetActive(false);
            }

            _inputService.OnMouseMoved.Subscribe(UpdatePreview).AddTo(_disposables);
            _inputService.OnPrimaryClick.Subscribe(PlaceBlock).AddTo(_disposables);
            _inputService.OnSecondaryClick.Subscribe(_ => RemoveLastBlock()).AddTo(_disposables);
            _rotationService.OnRotationCompleted.Subscribe(RotateActiveBlocks).AddTo(_disposables);

            PublishRemainingBlocks();
        }

        private void UpdatePreview(Vector2 mousePosition)
        {
            if (_contextService.CurrentContext.Value is InputContext.LevelCompleted or InputContext.Paused)
            {
                if (_previewBlock is not null) _previewBlock.gameObject.SetActive(false);
                return;
            }

            if (_previewBlock is null) return;

            if (_isDeveloperMode && _contextService.CurrentContext.Value != InputContext.PlaceBlock)
            {
                _previewBlock.gameObject.SetActive(false);
                return;
            }

            if (_raycastService.TryGetTargetCell(mousePosition, out var cell, out _))
            {
                _previewBlock.gameObject.SetActive(true);
                _previewBlock.SetPosition(cell);
                UpdatePreviewColor();
            }
            else
            {
                _previewBlock.gameObject.SetActive(false);
            }
        }

        private void UpdatePreviewColor()
        {
            if (_previewRenderer is null || _materialPropertyBlock is null) return;

            var isBlocked = _isLimitEnabled && _remainingBlocksCount <= 0;
            var targetColor = isBlocked ? BlockedPreviewColor : _previewDefaultColor;

            _materialPropertyBlock.SetColor("_Color", targetColor);
            _previewRenderer.SetPropertyBlock(_materialPropertyBlock);
        }

        private void PlaceBlock(Vector2 mousePosition)
        {
            if (_contextService.CurrentContext.Value is InputContext.LevelCompleted or InputContext.Paused)
            {
                if (_previewBlock is not null) _previewBlock.gameObject.SetActive(false);
                return;
            }

            if (_isDeveloperMode && _contextService.CurrentContext.Value != InputContext.PlaceBlock) return;
            if (_isLimitEnabled && _remainingBlocksCount <= 0) return;
            if (!_raycastService.TryGetTargetCell(mousePosition, out var cell, out _)) return;

            BlockView block;
            var identifier = "DefaultBlock";
            BlockConfig config = null;

            if (_isDeveloperMode)
            {
                config = _devModeService.ActiveBlockConfig.Value;
                if (config is null) return;
                block = _poolService.Get(config);
                identifier = config.DisplayName;
            }
            else
            {
                block = _poolService.GetDefault();
            }

            if (block is null) return;

            _gridService.SetCellOccupied(cell, true);
            block.SetPosition(cell);
            _activeBlocks[cell] = block;
            _historyService.RecordPlacement(new PlacementRecord(cell, config));

            if (_isDeveloperMode)
                _registryService.Register(new PlacedObjectData(PlacedObjectType.Block, cell, identifier));

            if (_isLimitEnabled)
            {
                _remainingBlocksCount--;
                PublishRemainingBlocks();
                UpdatePreviewColor();
            }

            _onGridChanged.OnNext(Unit.Default);
        }

        private void RemoveLastBlock()
        {
            if (_contextService.CurrentContext.Value is InputContext.LevelCompleted or InputContext.Paused)
            {
                if (_previewBlock is not null) _previewBlock.gameObject.SetActive(false);
                return;
            }

            if (!_historyService.TryPop(out var record)) return;

            _gridService.SetCellOccupied(record.Cell, false);

            if (_activeBlocks.TryGetValue(record.Cell, out var block))
            {
                _poolService.Return(block);
                _activeBlocks.Remove(record.Cell);
            }

            if (_isDeveloperMode)
                _registryService.Unregister(record.Cell, PlacedObjectType.Block);

            if (_isLimitEnabled)
            {
                _remainingBlocksCount++;
                PublishRemainingBlocks();
                UpdatePreviewColor();
            }

            _onGridChanged.OnNext(Unit.Default);
        }

        private void RotateActiveBlocks(int angle)
        {
            var gridSize = _gridService.GridSize;
            var newActiveBlocks = new Dictionary<Vector3Int, BlockView>();

            foreach (var (cell, block) in _activeBlocks)
            {
                var newCell = angle == 90
                    ? new Vector3Int(cell.z, cell.y, gridSize - 1 - cell.x)
                    : new Vector3Int(gridSize - 1 - cell.z, cell.y, cell.x);

                block.SetPosition(newCell);
                newActiveBlocks[newCell] = block;
            }

            _activeBlocks.Clear();
            foreach (var kvp in newActiveBlocks)
                _activeBlocks.Add(kvp.Key, kvp.Value);

            _gridService.Rotate(angle);
            _historyService.Rotate(angle, gridSize);
            _onGridChanged.OnNext(Unit.Default);
        }

        private void PublishRemainingBlocks() =>
            _remainingBlocks.Value = (_isLimitEnabled, _remainingBlocksCount);

        public void Dispose() => _disposables?.Dispose();
    }
}