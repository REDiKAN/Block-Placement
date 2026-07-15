using System;
using UniRx;

namespace Game.Services.Dev
{
    public interface IShadowDensityService
    {
        bool IsDensityEnabled(int wallIndex, int cellIndex);
        void SetDensityEnabled(int wallIndex, int cellIndex, bool enabled);
        void ToggleDensity(int wallIndex, int cellIndex);
        IObservable<(int WallIndex, int CellIndex, bool IsEnabled)> OnDensityToggled { get; }
    }
}