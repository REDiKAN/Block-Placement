using System;
using UniRx;
using Zenject;
using Game.Data;
using Game.Services.Rotation;

namespace Game.Services.Shadow
{
    public interface ITargetDensityProjectionService
    {
        IObservable<(int WallIndex, WallCellDensityData[] Densities)> OnDensitiesProjected { get; }
        WallCellDensityData[] GetCurrentDensities(int wallIndex);
    }

    public class TargetDensityProjectionService : ITargetDensityProjectionService, IInitializable, IDisposable
    {
        public IObservable<(int WallIndex, WallCellDensityData[] Densities)> OnDensitiesProjected => _onDensitiesProjected;

        private const int GridSize = 5;
        private const int CellCount = 25;

        private readonly Subject<(int WallIndex, WallCellDensityData[] Densities)> _onDensitiesProjected = new();
        private readonly CompositeDisposable _disposables = new();

        private readonly WallCellDensityData[] _baseWallYZ;
        private readonly WallCellDensityData[] _baseWallXY;

        private readonly WallCellDensityData[] _currentWallYZ = new WallCellDensityData[CellCount];
        private readonly WallCellDensityData[] _currentWallXY = new WallCellDensityData[CellCount];

        private int _currentAngle;

        public TargetDensityProjectionService(LevelConfig levelConfig, IRotationService rotationService)
        {
            _baseWallYZ = levelConfig.WallYZ?.CellDensities ?? Array.Empty<WallCellDensityData>();
            _baseWallXY = levelConfig.WallXY?.CellDensities ?? Array.Empty<WallCellDensityData>();

            rotationService.OnRotationCompleted
                .Subscribe(Rotate)
                .AddTo(_disposables);
        }

        public void Initialize()
        {
            _currentAngle = 0;
            ProjectDensities();
        }

        public WallCellDensityData[] GetCurrentDensities(int wallIndex) =>
            wallIndex == 0 ? _currentWallYZ : _currentWallXY;

        private void Rotate(int angle)
        {
            _currentAngle = (_currentAngle + angle + 360) % 360;
            ProjectDensities();
        }

        private void ProjectDensities()
        {
            var yzSource = _currentAngle is 90 or 270 ? _baseWallXY : _baseWallYZ;
            var xySource = _currentAngle is 90 or 270 ? _baseWallYZ : _baseWallXY;

            ApplyRotation(yzSource, _currentWallYZ, _currentAngle);
            ApplyRotation(xySource, _currentWallXY, _currentAngle);

            _onDensitiesProjected.OnNext((0, _currentWallYZ));
            _onDensitiesProjected.OnNext((1, _currentWallXY));
        }

        private void ApplyRotation(WallCellDensityData[] source, WallCellDensityData[] target, int angle)
        {
            if (source.Length != CellCount) return;

            Array.Clear(target, 0, CellCount);

            var normalizedAngle = (angle % 360 + 360) % 360;

            for (var i = 0; i < CellCount; i++)
            {
                var row = i / GridSize;
                var col = i % GridSize;

                var (newRow, newCol) = normalizedAngle switch
                {
                    90 => (col, GridSize - 1 - row),
                    180 => (GridSize - 1 - row, GridSize - 1 - col),
                    270 => (GridSize - 1 - col, row),
                    _ => (row, col)
                };

                target[newRow * GridSize + newCol] = source[i];
            }
        }

        public void Dispose() => _disposables?.Dispose();
    }
}