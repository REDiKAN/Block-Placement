using UnityEngine;
using Zenject;
using UniRx;
using Game.Data;
using Game.Services.Preview;
using Game.Services.Shadow;
using Game.Views;

namespace Game.Views.Preview
{
    public class PreviewWallView : MonoBehaviour
    {
        [field: SerializeField] public int WallIndex { get; private set; }
        [field: SerializeField] public WallCellView[] Cells { get; private set; }

        [Inject] private IPreviewShadowService _shadowService;
        private readonly CompositeDisposable _disposables = new();

        private void Start()
        {
            if (_shadowService is null) return;

            _shadowService.OnShadowUpdated
                .Where(update => update.WallIndex == WallIndex)
                .Subscribe(update => SetTargetShadow(update.ShadowMask))
                .AddTo(_disposables);

            _shadowService.OnDensitiesUpdated
                .Where(update => update.WallIndex == WallIndex)
                .Subscribe(update => SetTargetDensities(update.Densities))
                .AddTo(_disposables);

            _shadowService.OnShadowCleared
                .Subscribe(_ => Clear())
                .AddTo(_disposables);

            Clear();
        }

        private void SetTargetShadow(bool[] shadowMask)
        {
            if (Cells is null || shadowMask is null) return;

            for (var i = 0; i < Cells.Length; i++)
            {
                if (Cells[i] is null) continue;
                var state = i < shadowMask.Length && shadowMask[i] ? ShadowCellState.Correct : ShadowCellState.Empty;
                Cells[i].SetState(state);
            }
        }

        private void SetTargetDensities(WallCellDensityData[] densities)
        {
            if (Cells is null || densities is null) return;
            for (var i = 0; i < Cells.Length; i++)
            {
                if (Cells[i] is null) continue;
                var densityData = i < densities.Length ? densities[i] : default;
                Cells[i].SetTargetDensity(densityData.TargetDensity, densityData.IsDensityEnabled);
            }
        }

        private void Clear()
        {
            if (Cells is null) return;
            foreach (var cell in Cells)
            {
                if (cell is not null)
                {
                    cell.SetState(ShadowCellState.Empty);
                    cell.SetTargetDensity(0, false);
                }
            }
        }

        private void OnDestroy() => _disposables?.Dispose();
    }
}