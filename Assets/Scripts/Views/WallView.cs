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

        [Inject] private LevelConfig _levelConfig;

        [Inject]
        private void Construct(IShadowValidationService validationService, ICellHoverService cellHoverService)
        {
            validationService.OnCellStateChanged
                .Where(update => update.WallIndex == WallIndex)
                .Subscribe(UpdateCell)
                .AddTo(this);

            cellHoverService.OnCellHovered
                .Where(data => data.WallIndex == WallIndex)
                .Subscribe(data => Cells[data.CellIndex].SetHover(true))
                .AddTo(this);

            cellHoverService.OnCellUnhovered
                .Subscribe(_ => ResetHovers())
                .AddTo(this);

            InitializeTargetDensities();
        }

        private void InitializeTargetDensities()
        {
            if (Cells is null || _levelConfig is null) return;

            var densities = WallIndex == 0 ? _levelConfig.WallYZ?.CellDensities : _levelConfig.WallXY?.CellDensities;

            if (densities is null) return;

            for (var i = 0; i < Cells.Length; i++)
            {
                if (Cells[i] is not null && i < densities.Length)
                    Cells[i].SetTargetDensity(densities[i].TargetDensity, densities[i].IsDensityEnabled);
            }
        }

        private void UpdateCell(ShadowCellUpdate update) => Cells[update.CellIndex].SetState(update.State);

        private void ResetHovers()
        {
            if (Cells is null) return;
            foreach (var cell in Cells)
            {
                if (cell is not null)
                    cell.SetHover(false);
            }
        }
    }
}