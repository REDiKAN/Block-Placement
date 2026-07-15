using System;
using UniRx;

namespace Game.Services.Dev
{
    public class ShadowDensityService : IShadowDensityService
    {
        public IObservable<(int WallIndex, int CellIndex, bool IsEnabled)> OnDensityToggled => _onDensityToggled;

        private const int CellCount = 25;
        private readonly Subject<(int WallIndex, int CellIndex, bool IsEnabled)> _onDensityToggled = new();
        private readonly bool[] _wallYZDensities = new bool[CellCount];
        private readonly bool[] _wallXYDensities = new bool[CellCount];

        public bool IsDensityEnabled(int wallIndex, int cellIndex) =>
            wallIndex == 0 ? _wallYZDensities[cellIndex] : _wallXYDensities[cellIndex];

        public void SetDensityEnabled(int wallIndex, int cellIndex, bool enabled)
        {
            if (wallIndex == 0)
                _wallYZDensities[cellIndex] = enabled;
            else
                _wallXYDensities[cellIndex] = enabled;

            _onDensityToggled.OnNext((wallIndex, cellIndex, enabled));
        }

        public void ToggleDensity(int wallIndex, int cellIndex) =>
            SetDensityEnabled(wallIndex, cellIndex, !IsDensityEnabled(wallIndex, cellIndex));
    }
}