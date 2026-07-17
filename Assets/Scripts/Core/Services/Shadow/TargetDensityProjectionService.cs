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
        void SetDensities(WallCellDensityData[] wallYZ, WallCellDensityData[] wallXY);
    }

    public class TargetDensityProjectionService : ITargetDensityProjectionService, IInitializable, IDisposable
    {
        public IObservable<(int WallIndex, WallCellDensityData[] Densities)> OnDensitiesProjected => _onDensitiesProjected;

        private const int GridSize = 5;
        private const int CellCount = 25;

        private readonly Subject<(int WallIndex, WallCellDensityData[] Densities)> _onDensitiesProjected = new();
        private readonly CompositeDisposable _disposables = new();

        private WallCellDensityData[] _baseWallYZ;
        private WallCellDensityData[] _baseWallXY;

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

        public void SetDensities(WallCellDensityData[] wallYZ, WallCellDensityData[] wallXY)
        {
            _baseWallYZ = wallYZ ?? Array.Empty<WallCellDensityData>();
            _baseWallXY = wallXY ?? Array.Empty<WallCellDensityData>();
            ProjectDensities();
        }

        private void Rotate(int angle)
        {
            _currentAngle = (_currentAngle + angle + 360) % 360;
            ProjectDensities();
        }

        private void ProjectDensities()
        {
            var yzSource = _currentAngle is 90 or 270 ? _baseWallXY : _baseWallYZ;
            var xySource = _currentAngle is 90 or 270 ? _baseWallYZ : _baseWallXY;

            ApplyRotation(yzSource, _currentWallYZ, _currentAngle, 0);
            ApplyRotation(xySource, _currentWallXY, _currentAngle, 1);

            _onDensitiesProjected.OnNext((0, _currentWallYZ));
            _onDensitiesProjected.OnNext((1, _currentWallXY));
        }

        private void ApplyRotation(WallCellDensityData[] source, WallCellDensityData[] target, int angle, int wallIndex)
        {
            if (source.Length != CellCount) return;

            Array.Clear(target, 0, CellCount);
            var normalizedAngle = (angle % 360 + 360) % 360;

            for (var i = 0; i < CellCount; i++)
            {
                var row = i / GridSize;
                var col = i % GridSize;

                var (newRow, newCol) = wallIndex switch
                {
                    0 => normalizedAngle switch
                    {
                        90 => (col, GridSize - 1 - row),
                        180 => (row, GridSize - 1 - col),
                        270 => (col, row),
                        _ => (row, col)
                    },
                    1 => normalizedAngle switch
                    {
                        90 => (col, row),
                        180 => (GridSize - 1 - row, col),
                        270 => (GridSize - 1 - col, row),
                        _ => (row, col)
                    },
                    _ => (row, col)
                };

                target[newRow * GridSize + newCol] = source[i];
            }
        }

        public void Dispose() => _disposables?.Dispose();
    }
}