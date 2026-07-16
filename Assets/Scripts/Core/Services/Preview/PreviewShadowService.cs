using System;
using UniRx;
using Game.Data;

namespace Game.Services.Preview
{
    public class PreviewShadowService : IPreviewShadowService
    {
        public IObservable<(int WallIndex, bool[] ShadowMask)> OnShadowUpdated => _onShadowUpdated;
        public IObservable<(int WallIndex, WallCellDensityData[] Densities)> OnDensitiesUpdated => _onDensitiesUpdated;
        public IObservable<Unit> OnShadowCleared => _onShadowCleared;

        private readonly Subject<(int WallIndex, bool[] ShadowMask)> _onShadowUpdated = new();
        private readonly Subject<(int WallIndex, WallCellDensityData[] Densities)> _onDensitiesUpdated = new();
        private readonly Subject<Unit> _onShadowCleared = new();

        public void UpdateShadows(bool[] wall1, bool[] wall2)
        {
            _onShadowUpdated.OnNext((0, wall1));
            _onShadowUpdated.OnNext((1, wall2));
        }

        public void UpdateDensities(WallCellDensityData[] wall1, WallCellDensityData[] wall2)
        {
            _onDensitiesUpdated.OnNext((0, wall1));
            _onDensitiesUpdated.OnNext((1, wall2));
        }

        public void ClearShadows() => _onShadowCleared.OnNext(Unit.Default);
    }
}