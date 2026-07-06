using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;
using Game.Services.Grid;
using Game.Services.History;
using Game.Services.Input;
using Game.Services.Pool;
using Game.Services.Raycast;
using Game.Views;

namespace Game.Services.Placement
{
    public interface IBlockPlacementService
    {
        IObservable<Unit> OnGridChanged { get; }
    }

    public class BlockPlacementService : IBlockPlacementService, IInitializable, IDisposable
    {
        private readonly Subject<Unit> _onGridChanged = new();
        private readonly CompositeDisposable _disposables = new();
        private readonly Dictionary<Vector3Int, BlockView> _activeBlocks = new();
        private readonly IInputService _inputService;
        private readonly IRaycastService _raycastService;
        private readonly IGridService _gridService;
        private readonly IBlockPoolService _poolService;
        private readonly IBlockHistoryService _historyService;
        private readonly BlockView _previewBlock;

        public IObservable<Unit> OnGridChanged => _onGridChanged;

        public BlockPlacementService(
            IInputService inputService,
            IRaycastService raycastService,
            IGridService gridService,
            IBlockPoolService poolService,
            IBlockHistoryService historyService,
            [Inject(Id = "PreviewBlock")] BlockView previewBlock)
        {
            _inputService = inputService;
            _raycastService = raycastService;
            _gridService = gridService;
            _poolService = poolService;
            _historyService = historyService;
            _previewBlock = previewBlock;
        }

        public void Initialize()
        {
            _inputService.OnMouseMoved
                .Subscribe(UpdatePreview)
                .AddTo(_disposables);

            _inputService.OnPrimaryClick
                .Subscribe(PlaceBlock)
                .AddTo(_disposables);

            _inputService.OnSecondaryClick
                .Subscribe(_ => RemoveLastBlock())
                .AddTo(_disposables);

            if (_previewBlock is not null)
                _previewBlock.gameObject.SetActive(false);
        }

        private void UpdatePreview(Vector2 mousePosition)
        {
            if (_previewBlock is null) return;

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
            if (!_raycastService.TryGetTargetCell(mousePosition, out var cell, out _)) return;

            _gridService.SetCellOccupied(cell, true);
            var block = _poolService.Get();

            if (block is null) return;

            block.SetPosition(cell);
            _activeBlocks[cell] = block;
            _historyService.RecordPlacement(cell);
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

            _onGridChanged.OnNext(Unit.Default);
        }

        public void Dispose() => _disposables?.Dispose();
    }
}