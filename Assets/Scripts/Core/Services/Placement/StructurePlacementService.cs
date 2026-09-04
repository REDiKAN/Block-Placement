using Game.Data;
using Game.Services.Audio;
using Game.Services.Grid;
using Game.Services.History;
using Game.Services.Input;
using Game.Services.Pool;
using Game.Services.Raycast;
using Game.Services.Registry;
using Game.Services.Rotation;
using Game.Services.Animation;
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
        private readonly IStructureAnimationService _animationService;
        private readonly ISfxService _sfxService;
        private readonly AudioConfig _audioConfig;
        private readonly LevelConfig _levelConfig;
        private readonly IRotationService _rotationService;

        private StructureConfig _selectedConfig;
        private StructureView _previewInstance;
        private readonly Dictionary<Vector3Int, StructureView> _activeStructures = new();
        private bool _isAnimating;
        private int _currentAngle;
        private int _localPreviewAngle;
        private Vector2 _lastMousePosition;

        private static readonly Vector3Int[] _directions =
        {
            Vector3Int.up, Vector3Int.down,
            Vector3Int.left, Vector3Int.right,
            Vector3Int.forward, Vector3Int.back
        };

        private int TotalPreviewAngle => (_currentAngle + _localPreviewAngle) % 360;

        public StructurePlacementService(
            IInputService inputService,
            IRaycastService raycastService,
            IGridService gridService,
            IStructurePoolService poolService,
            IPlacementHistoryService historyService,
            IInputContextService contextService,
            IObjectRegistryService registryService,
            IStructureAnimationService animationService,
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
            _animationService = animationService;
            _sfxService = sfxService;
            _audioConfig = audioConfig;
            _levelConfig = levelConfig;
            _rotationService = rotationService;
        }

        public void Initialize()
        {
            _currentAngle = 0;
            _localPreviewAngle = 0;

            _inputService.OnMouseMoved.Subscribe(UpdatePreview).AddTo(_disposables);
            _inputService.OnPrimaryClick.Subscribe(PlaceStructure).AddTo(_disposables);
            _inputService.OnSecondaryClick.Subscribe(_ => RemoveLastStructure()).AddTo(_disposables);
            _inputService.OnRotatePreviewLeft.Subscribe(_ => RotatePreview(-90)).AddTo(_disposables);
            _inputService.OnRotatePreviewRight.Subscribe(_ => RotatePreview(90)).AddTo(_disposables);

            _rotationService.OnRotationCompleted.Subscribe(angle =>
            {
                _currentAngle = (_currentAngle + angle + 360) % 360;
                RotateActiveStructures(angle);
                UpdatePreview(_lastMousePosition);
            }).AddTo(_disposables);
        }

        private void RotatePreview(int angleDelta)
        {
            if (_selectedConfig is null) return;
            if (_contextService.CurrentContext.Value != InputContext.PlaceBlock) return;

            _localPreviewAngle = (_localPreviewAngle + angleDelta + 360) % 360;

            if (_previewInstance is not null)
            {
                _previewInstance.transform.rotation = Quaternion.Euler(0f, TotalPreviewAngle, 0f);
            }

            UpdatePreview(_lastMousePosition);
        }

        private Vector3Int[] GetRotatedLocalCoordinates(Vector3Int[] localCoords)
        {
            if (localCoords is null || localCoords.Length == 0) return Array.Empty<Vector3Int>();

            var totalAngle = TotalPreviewAngle;
            if (totalAngle == 0) return localCoords;

            var rotated = new Vector3Int[localCoords.Length];
            for (var i = 0; i < localCoords.Length; i++)
            {
                var local = localCoords[i];
                rotated[i] = totalAngle switch
                {
                    90 => new Vector3Int(local.z, local.y, -local.x),
                    180 => new Vector3Int(-local.x, local.y, -local.z),
                    270 => new Vector3Int(-local.z, local.y, local.x),
                    _ => local
                };
            }
            return rotated;
        }

        public void SelectStructure(StructureConfig config)
        {
            _selectedConfig = config;
            _localPreviewAngle = 0;
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
            _lastMousePosition = mousePosition;

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

            _previewInstance.transform.rotation = Quaternion.Euler(0f, TotalPreviewAngle, 0f);

            if (_raycastService.TryGetTargetCell(mousePosition, out var cell, out _))
            {
                var rotatedCoords = GetRotatedLocalCoordinates(_selectedConfig.LocalCoordinates);
                var isValid = ValidatePlacement(cell, rotatedCoords);
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

            var rotatedCoords = GetRotatedLocalCoordinates(_selectedConfig.LocalCoordinates);
            if (!ValidatePlacement(originCell, rotatedCoords)) return;

            var structure = _previewInstance ?? _poolService.Get(_selectedConfig);
            if (structure is null) return;

            structure.transform.rotation = Quaternion.Euler(0f, TotalPreviewAngle, 0f);

            var worldCells = new Vector3Int[rotatedCoords.Length];
            for (var i = 0; i < rotatedCoords.Length; i++)
            {
                worldCells[i] = originCell + rotatedCoords[i];
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

            _animationService.AnimateSpawn(structure, OnStructureSpawned);
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

                var removeClip = config.RemoveClip ?? _audioConfig?.DefaultRemoveClip;
                if (removeClip is not null) _sfxService.Play(removeClip);

                _isAnimating = true;
                _contextService.SetContext(InputContext.Generating);

                _animationService.AnimateDespawn(structure, () => OnStructureDespawned(structure, config));
            }
        }

        private void OnStructureDespawned(StructureView structure, StructureConfig config)
        {
            _poolService.Return(structure);
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
                    foreach (var dir in _directions)
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

        public void Dispose() => _disposables?.Dispose();
    }
}