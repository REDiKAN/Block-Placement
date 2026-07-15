using System;
using UniRx;
using Zenject;

namespace Game.Services.Dev
{
    public interface ICellHoverService
    {
        IObservable<(int WallIndex, int CellIndex)> OnCellHovered { get; }
        IObservable<Unit> OnCellUnhovered { get; }
        void NotifyHovered(int wallIndex, int cellIndex);
        void NotifyUnhovered();

        IObservable<int> OnFloorCellHovered { get; }
        IObservable<Unit> OnFloorCellUnhovered { get; }
        void NotifyFloorHovered(int cellIndex);
        void NotifyFloorUnhovered();
    }

    public class CellHoverService : ICellHoverService
    {
        public IObservable<(int WallIndex, int CellIndex)> OnCellHovered => _onCellHovered;
        public IObservable<Unit> OnCellUnhovered => _onCellUnhovered;
        public IObservable<int> OnFloorCellHovered => _onFloorCellHovered;
        public IObservable<Unit> OnFloorCellUnhovered => _onFloorCellUnhovered;

        private readonly Subject<(int WallIndex, int CellIndex)> _onCellHovered = new();
        private readonly Subject<Unit> _onCellUnhovered = new();
        private readonly Subject<int> _onFloorCellHovered = new();
        private readonly Subject<Unit> _onFloorCellUnhovered = new();
        private readonly bool _isDeveloperMode;

        public CellHoverService([Inject(Id = "IsDeveloperMode")] bool isDeveloperMode)
        {
            _isDeveloperMode = isDeveloperMode;
        }

        public void NotifyHovered(int wallIndex, int cellIndex)
        {
            if (!_isDeveloperMode) return;
            _onCellHovered.OnNext((wallIndex, cellIndex));
        }

        public void NotifyUnhovered()
        {
            if (!_isDeveloperMode) return;
            _onCellUnhovered.OnNext(Unit.Default);
        }

        public void NotifyFloorHovered(int cellIndex)
        {
            if (!_isDeveloperMode) return;
            _onFloorCellHovered.OnNext(cellIndex);
        }

        public void NotifyFloorUnhovered()
        {
            if (!_isDeveloperMode) return;
            _onFloorCellUnhovered.OnNext(Unit.Default);
        }
    }
}