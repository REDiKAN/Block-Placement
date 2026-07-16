using System;
using UniRx;
using Game.Data;

namespace Game.Services.Preview
{
    public interface IPreviewShadowService
    {
        IObservable<(int WallIndex, bool[] ShadowMask)> OnShadowUpdated { get; }
        IObservable<(int WallIndex, WallCellDensityData[] Densities)> OnDensitiesUpdated { get; }
        IObservable<Unit> OnShadowCleared { get; }
        void UpdateShadows(bool[] wall1, bool[] wall2);
        void UpdateDensities(WallCellDensityData[] wall1, WallCellDensityData[] wall2);
        void ClearShadows();
    }
}