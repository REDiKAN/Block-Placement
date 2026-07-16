using UniRx;
using UnityEngine;
using Zenject;
using Game.Data;
using Game.Services.Dev;
using Game.Services.Shadow;

namespace Game.Views
{
    public class WallView : MonoBehaviour
    {
        [field: SerializeField] public int WallIndex { get; private set; }
        [field: SerializeField] public WallCellView[] Cells { get; private set; }

        private ITargetDensityProjectionService _projectionService;
        private IShadowValidationService _validationService;

        [Inject]
        private void Construct(
            ITargetDensityProjectionService projectionService,
            IShadowValidationService validationService,
            ICellHoverService cellHoverService)
        {
            _projectionService = projectionService;
            _validationService = validationService;

            _validationService.OnCellStateChanged
                .Where(update => update.WallIndex == WallIndex)
                .Subscribe(UpdateCell)
                .AddTo(this);

            _projectionService.OnDensitiesProjected
                .Where(update => update.WallIndex == WallIndex)
                .Subscribe(UpdateDensities)
                .AddTo(this);

            cellHoverService.OnCellHovered
                .Where(data => data.WallIndex == WallIndex)
                .Subscribe(data => Cells[data.CellIndex].SetHover(true))
                .AddTo(this);

            cellHoverService.OnCellUnhovered
                .Subscribe(_ => ResetHovers())
                .AddTo(this);
        }

        private void Start()
        {
            var densities = _projectionService.GetCurrentDensities(WallIndex);
            if (densities is not null)
                UpdateDensities((WallIndex, densities));
        }

        private void UpdateCell(ShadowCellUpdate update)
        {
            if (Cells is null || update.CellIndex < 0 || update.CellIndex >= Cells.Length || Cells[update.CellIndex] is null)
                return;

            Cells[update.CellIndex].SetState(update.State);
        }

        private void UpdateDensities((int WallIndex, WallCellDensityData[] Densities) update)
        {
            if (Cells is null || update.Densities is null) return;

            for (var i = 0; i < Cells.Length; i++)
            {
                if (Cells[i] is not null && i < update.Densities.Length)
                    Cells[i].SetTargetDensity(update.Densities[i].TargetDensity, update.Densities[i].IsDensityEnabled);
            }
        }

        private void ResetHovers()
        {
            if (Cells is null) return;

            foreach (var cell in Cells)
                if (cell is not null)
                    cell.SetHover(false);
        }
    }
}