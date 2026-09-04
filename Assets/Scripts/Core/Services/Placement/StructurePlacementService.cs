using Game.Data;
using Game.Services.Audio;
using Game.Services.Grid;
using Game.Services.History;
using Game.Services.Input;
using Game.Services.Pool;
using Game.Services.Raycast;
using Game.Services.Registry;
using Game.Services.Rotation;
using Game.Views;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

namespace Game.Services.Placement
{
    public interface IStructurePlacementService
    {
        IObservable<Unit> OnGridChanged { get; }
        void SelectStructure(StructureConfig config);
        void ClearSelection();
    }

    public class StructurePlacementService : IStructurePlacementService, IInitializable, IDisposable
    {
        public IObservable<Unit> OnGridChanged => _onGridChanged;

        private readonly Subject<Unit> _onGridChanged = new();
        private readonly CompositeDisposable _disposables = new();

        private readonly IInputService _inputService;
        private readonly IRaycastService _raycastService;
        private readonly IGridService _gridService;
        private readonly IStructurePoolService _poolService;
        private readonly IPlacementHistoryService _historyService;
        private readonly IInputContextService _contextService;
        private readonly IObjectRegistryService _registryService;
        private readonly ISfxService _sfxService;
        private readonly AudioConfig _audioConfig;
        private readonly LevelConfig _levelConfig;
        private readonly IRotationService _rotationService;

        private StructureConfig _selectedConfig;
        private StructureView _previewInstance;
        private readonly Dictionary<Vector3Int, StructureView> _activeStructures = new();
        private bool _isAnimating;

        public StructurePlacementService(
            IInputService inputService,
            IRaycastService raycastService,
            IGridService gridService,
            IStructurePoolService poolService,
            IPlacementHistoryService historyService,
            IInputContextService contextService,
            IObjectRegistryService registryService,
            ISfxService sfxService,
            AudioConfig audioConfig,
            LevelConfig levelConfig,
            IRotationService rotationService)
        {
            _inputService = inputService;
            _raycastService = raycastService;
            _gridService = gridService;
            _poolService = poolService;
            _historyService = historyService;
            _contextService = contextService;
            _registryService = registryService;
            _sfxService = sfxService;
            _audioConfig = audioConfig;
            _levelConfig = levelConfig;
            _rotationService = rotationService;
        }

        public void Initialize()
        {
            _inputService.OnMouseMoved.Subscribe(UpdatePreview).AddTo(_disposables);
            _inputService.OnPrimaryClick.Subscribe(PlaceStructure).AddTo(_disposables);
            _inputService.OnSecondaryClick.Subscribe(_ => RemoveLastStructure()).AddTo(_disposables);

            _rotationService.OnRotationCompleted.Subscribe(RotateActiveStructures).AddTo(_disposables);
        }

        public void SelectStructure(StructureConfig config)
        {
            _selectedConfig = config;
            ReturnPreviewToPool();
        }

        public void ClearSelection()
        {
            _selectedConfig = null;
            ReturnPreviewToPool();
        }

        private void ReturnPreviewToPool()
        {
            if (_previewInstance is null) return;
            _previewInstance.SetInteractionEnabled(true);
            _poolService.Return(_previewInstance);
            _previewInstance = null;
        }

        private void UpdatePreview(Vector2 mousePosition)
        {
            if (_levelConfig is null || _levelConfig.Mode != GameMode.Structures)
            {
                ReturnPreviewToPool();
                return;
            }

            if (_contextService.CurrentContext.Value is InputContext.LevelCompleted or InputContext.Paused or InputContext.TimeExpired)
            {
                if (_previewInstance is not null) _previewInstance.gameObject.SetActive(false);
                return;
            }

            if (_selectedConfig is null)
            {
                ReturnPreviewToPool();
                return;
            }

            if (_previewInstance is null)
            {
                _previewInstance = _poolService.Get(_selectedConfig);
                if (_previewInstance is null) return;
                _previewInstance.SetInteractionEnabled(false);
            }

            if (_raycastService.TryGetTargetCell(mousePosition, out var cell, out _))
            {
                var isValid = ValidatePlacement(cell, _selectedConfig.LocalCoordinates);
                _previewInstance.gameObject.SetActive(isValid);
                if (isValid) _previewInstance.SetPosition(cell);
            }
            else
            {
                _previewInstance.gameObject.SetActive(false);
            }
        }

        private void PlaceStructure(Vector2 mousePosition)
        {
            if (_levelConfig is null || _levelConfig.Mode != GameMode.Structures) return;
            if (_contextService.CurrentContext.Value is InputContext.LevelCompleted or InputContext.Paused or InputContext.TimeExpired) return;
            if (_isAnimating || _selectedConfig is null) return;
            if (!_raycastService.TryGetTargetCell(mousePosition, out var originCell, out _)) return;
            if (!ValidatePlacement(originCell, _selectedConfig.LocalCoordinates)) return;

            var structure = _previewInstance ?? _poolService.Get(_selectedConfig);
            if (structure is null) return;

            var worldCells = new Vector3Int[_selectedConfig.LocalCoordinates.Length];
            for (var i = 0; i < _selectedConfig.LocalCoordinates.Length; i++)
            {
                var local = _selectedConfig.LocalCoordinates[i];
                worldCells[i] = originCell + local;
                _gridService.SetCellOccupied(worldCells[i], true);
                _registryService.Register(new PlacedObjectData(PlacedObjectType.Block, worldCells[i], _selectedConfig.DisplayName));
            }

            structure.SetPosition(originCell);
            structure.SetInteractionEnabled(true);
            _activeStructures[worldCells[0]] = structure;
            _previewInstance = null;

            _historyService.RecordPlacement(new PlacementRecord(worldCells, _selectedConfig));

            var placeClip = _selectedConfig.PlaceClip ?? _audioConfig?.DefaultPlaceClip;
            if (placeClip is not null) _sfxService.Play(placeClip);

            _isAnimating = true;
            _contextService.SetContext(InputContext.Generating);
            Observable.Timer(TimeSpan.FromSeconds(0.1f))
                .Subscribe(_ => OnStructureSpawned())
                .AddTo(_disposables);
        }

        private void OnStructureSpawned()
        {
            _isAnimating = false;
            _contextService.SetContext(InputContext.PlaceBlock);
            _onGridChanged.OnNext(Unit.Default);
        }

        private void RemoveLastStructure()
        {
            if (_levelConfig is null || _levelConfig.Mode != GameMode.Structures) return;
            if (_contextService.CurrentContext.Value is InputContext.LevelCompleted or InputContext.Paused or InputContext.TimeExpired) return;
            if (_isAnimating) return;
            if (!_historyService.TryPop(out var record)) return;
            if (record.Config is not StructureConfig config) return;

            foreach (var cell in record.Cells)
            {
                _gridService.SetCellOccupied(cell, false);
                _registryService.Unregister(cell, PlacedObjectType.Block);
            }

            if (_activeStructures.TryGetValue(record.Cells[0], out var structure))
            {
                _activeStructures.Remove(record.Cells[0]);
                structure.gameObject.SetActive(false);
                _isAnimating = true;
                _contextService.SetContext(InputContext.Generating);
                Observable.Timer(TimeSpan.FromSeconds(0.1f))
                    .Subscribe(_ => OnStructureDespawned(structure, config))
                    .AddTo(_disposables);
            }
        }

        private void OnStructureDespawned(StructureView structure, StructureConfig config)
        {
            _poolService.Return(structure);
            var removeClip = config.RemoveClip ?? _audioConfig?.DefaultRemoveClip;
            if (removeClip is not null) _sfxService.Play(removeClip);
            _isAnimating = false;
            _contextService.SetContext(InputContext.PlaceBlock);
            _onGridChanged.OnNext(Unit.Default);
        }

        private void RotateActiveStructures(int angle)
        {
            var gridSize = _gridService.GridSize;
            var newActiveStructures = new Dictionary<Vector3Int, StructureView>();

            foreach (var kvp in _activeStructures)
            {
                var origin = kvp.Key;
                var structure = kvp.Value;

                var newOrigin = angle == 90
                    ? new Vector3Int(origin.z, origin.y, gridSize - 1 - origin.x)
                    : new Vector3Int(gridSize - 1 - origin.z, origin.y, origin.x);

                structure.SetPosition(newOrigin);
                newActiveStructures[newOrigin] = structure;
            }

            _activeStructures.Clear();
            foreach (var kvp in newActiveStructures)
                _activeStructures.Add(kvp.Key, kvp.Value);

            _onGridChanged.OnNext(Unit.Default);
        }

        private bool ValidatePlacement(Vector3Int origin, Vector3Int[] localCoords)
        {
            if (localCoords is null || localCoords.Length == 0) return false;
            var hasSupport = false;

            foreach (var local in localCoords)
            {
                var worldCell = origin + local;
                if (!_gridService.IsWithinBounds(worldCell)) return false;
                if (_gridService.IsCellOccupied(worldCell)) return false;

                if (worldCell.y == 0)
                {
                    var floorCoord = new Vector2Int(worldCell.x, worldCell.z);
                    if (_gridService.IsFloorExists(floorCoord)) hasSupport = true;
                }
                else
                {
                    var belowCell = new Vector3Int(worldCell.x, worldCell.y - 1, worldCell.z);
                    if (_gridService.IsCellOccupied(belowCell)) hasSupport = true;
                }
            }

            return hasSupport;
        }

        public void Dispose() => _disposables?.Dispose();
    }
}