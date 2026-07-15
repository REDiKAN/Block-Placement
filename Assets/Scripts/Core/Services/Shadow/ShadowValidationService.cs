using System;
using UniRx;
using UnityEngine;
using Zenject;
using Game.Data;
using Game.Services.Grid;
using Game.Services.Placement;
using Game.Services.Rotation;

namespace Game.Services.Shadow
{
    public enum ShadowCellState
    {
        Empty,
        Missing,
        Correct,
        Extra
    }

    public readonly struct ShadowCellUpdate
    {
        public int WallIndex { get; }
        public int CellIndex { get; }
        public ShadowCellState State { get; }

        public ShadowCellUpdate(int wallIndex, int cellIndex, ShadowCellState state)
        {
            WallIndex = wallIndex;
            CellIndex = cellIndex;
            State = state;
        }
    }

    public interface IShadowValidationService
    {
        IObservable<ShadowCellUpdate> OnCellStateChanged { get; }
        IObservable<Unit> OnLevelCompleted { get; }
        void ForceRevalidate();
    }

    public class ShadowValidationService : IShadowValidationService, IInitializable, IDisposable
    {
        public IObservable<ShadowCellUpdate> OnCellStateChanged => _onCellStateChanged;
        public IObservable<Unit> OnLevelCompleted => _onLevelCompleted;

        private const int GridSize = 5;
        private const int CellCount = 25;

        private readonly Subject<ShadowCellUpdate> _onCellStateChanged = new();
        private readonly Subject<Unit> _onLevelCompleted = new();
        private readonly CompositeDisposable _disposables = new();

        private readonly IGridService _gridService;
        private readonly IShadowCalculationService _calculationService;
        private readonly IRotationService _rotationService;
        private readonly LevelConfig _levelConfig;

        private readonly bool[] _hasShadow1 = new bool[CellCount];
        private readonly bool[] _hasShadow2 = new bool[CellCount];
        private readonly ShadowCellState[] _wall1States = new ShadowCellState[CellCount];
        private readonly ShadowCellState[] _wall2States = new ShadowCellState[CellCount];

        private bool[] _targetWall1;
        private bool[] _targetWall2;
        private Vector3Int _cellCoord;

        public ShadowValidationService(
            IGridService gridService,
            IBlockPlacementService placementService,
            IShadowCalculationService calculationService,
            IRotationService rotationService,
            LevelConfig levelConfig)
        {
            _gridService = gridService;
            _calculationService = calculationService;
            _rotationService = rotationService;
            _levelConfig = levelConfig;

            placementService.OnGridChanged
                .Subscribe(_ => ValidateAndPublish())
                .AddTo(_disposables);
        }

        public void Initialize()
        {
            ValidateAndPublish();
        }

        private void ValidateAndPublish()
        {
            var projection = _calculationService.Calculate(_rotationService.CurrentInitialBlocks, GridSize);
            _targetWall1 = projection.Wall1;
            _targetWall2 = projection.Wall2;

            Array.Clear(_hasShadow1, 0, CellCount);
            Array.Clear(_hasShadow2, 0, CellCount);

            for (var x = 0; x < GridSize; x++)
            {
                _cellCoord.x = x;
                for (var y = 0; y < GridSize; y++)
                {
                    _cellCoord.y = y;
                    for (var z = 0; z < GridSize; z++)
                    {
                        _cellCoord.z = z;
                        if (!_gridService.IsCellOccupied(_cellCoord)) continue;
                        _hasShadow1[y * GridSize + z] = true;
                        _hasShadow2[x * GridSize + y] = true;
                    }
                }
            }

            var isLevelCompleted = true;
            for (var i = 0; i < CellCount; i++)
            {
                var yzData = GetDensityData(_levelConfig.WallYZ, i);
                var xyData = GetDensityData(_levelConfig.WallXY, i);

                isLevelCompleted &= EvaluateAndPublish(0, i, _hasShadow1[i], _targetWall1[i], yzData, ref _wall1States[i]);
                isLevelCompleted &= EvaluateAndPublish(1, i, _hasShadow2[i], _targetWall2[i], xyData, ref _wall2States[i]);
            }

            if (isLevelCompleted)
                _onLevelCompleted.OnNext(Unit.Default);
        }

        private WallCellDensityData GetDensityData(WallData wallData, int index)
        {
            if (wallData?.CellDensities is null || index >= wallData.CellDensities.Length)
                return default;

            return wallData.CellDensities[index];
        }

        private bool EvaluateAndPublish(int wallIndex, int cellIndex, bool hasShadow, bool isTarget, WallCellDensityData densityData, ref ShadowCellState currentState)
        {
            ShadowCellState newState;

            if (densityData.IsDensityEnabled)
            {
                var actualDensity = _calculationService.CalculateDensity(wallIndex, cellIndex, _gridService, GridSize);
                var targetDensity = densityData.TargetDensity;

                newState = targetDensity switch
                {
                    0 => ShadowCellState.Empty,
                    _ when actualDensity == targetDensity => ShadowCellState.Correct,
                    _ when actualDensity < targetDensity => ShadowCellState.Missing,
                    _ => ShadowCellState.Extra
                };
            }
            else
            {
                newState = (hasShadow, isTarget) switch
                {
                    (false, false) => ShadowCellState.Empty,
                    (false, true) => ShadowCellState.Missing,
                    (true, true) => ShadowCellState.Correct,
                    _ => ShadowCellState.Extra
                };
            }

            if (currentState != newState)
            {
                currentState = newState;
                _onCellStateChanged.OnNext(new ShadowCellUpdate(wallIndex, cellIndex, newState));
            }

            if (densityData.IsDensityEnabled)
            {
                var targetDensity = densityData.TargetDensity;
                return targetDensity == 0 ? newState == ShadowCellState.Empty : newState == ShadowCellState.Correct;
            }

            return isTarget ? newState == ShadowCellState.Correct : newState == ShadowCellState.Empty;
        }

        public void Dispose() => _disposables?.Dispose();
        public void ForceRevalidate() => ValidateAndPublish();
    }
}