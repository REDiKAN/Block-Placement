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
    }

    public class BlockPlacementService : IBlockPlacementService, IInitializable, IDisposable
    {
        public IObservable<Unit> OnGridChanged => _onGridChanged;

        private readonly Subject<Unit> _onGridChanged = new();
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
        private readonly bool _isDeveloperMode;

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
            _isDeveloperMode = isDeveloperMode;
            _previewBlock = previewBlock;
        }

        public void Initialize()
        {
            _inputService.OnMouseMoved.Subscribe(UpdatePreview).AddTo(_disposables);
            _inputService.OnPrimaryClick.Subscribe(PlaceBlock).AddTo(_disposables);
            _inputService.OnSecondaryClick.Subscribe(_ => RemoveLastBlock()).AddTo(_disposables);
            _rotationService.OnRotationCompleted.Subscribe(RotateActiveBlocks).AddTo(_disposables);

            if (_previewBlock is not null)
                _previewBlock.gameObject.SetActive(false);
        }

        private void UpdatePreview(Vector2 mousePosition)
        {
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
            }
            else
            {
                _previewBlock.gameObject.SetActive(false);
            }
        }

        private void PlaceBlock(Vector2 mousePosition)
        {
            if (_isDeveloperMode && _contextService.CurrentContext.Value != InputContext.PlaceBlock) return;
            if (!_raycastService.TryGetTargetCell(mousePosition, out var cell, out _)) return;

            BlockView block;
            var identifier = "DefaultBlock";

            if (_isDeveloperMode)
            {
                var config = _devModeService.ActiveBlockConfig.Value;
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
            _historyService.RecordPlacement(cell);

            if (_isDeveloperMode)
                _registryService.Register(new PlacedObjectData(PlacedObjectType.Block, cell, identifier));

            _onGridChanged.OnNext(Unit.Default);
        }

        private void RemoveLastBlock()
        {
            if (!_historyService.TryPop(out var targetCell)) return;
            if (_gridService.IsCellOccupied(targetCell + Vector3Int.up)) return;

            _gridService.SetCellOccupied(targetCell, false);
            if (_activeBlocks.TryGetValue(targetCell, out var block))
            {
                _poolService.Return(block);
                _activeBlocks.Remove(targetCell);
            }

            if (_isDeveloperMode)
                _registryService.Unregister(targetCell, PlacedObjectType.Block);

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
            _onGridChanged.OnNext(Unit.Default);
        }

        public void Dispose() => _disposables?.Dispose();
    }
}