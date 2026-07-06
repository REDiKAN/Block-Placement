using UniRx;
using UnityEngine;
using Zenject;
using Game.Services.Shadow;

namespace Game.Views
{
    public class WallView : MonoBehaviour
    {
        [field: SerializeField] public int WallIndex { get; private set; }
        [field: SerializeField] public WallCellView[] Cells { get; private set; }

        [Inject]
        private void Construct(IShadowValidationService validationService)
        {
            validationService.OnCellStateChanged
                .Where(update => update.WallIndex == WallIndex)
                .Subscribe(UpdateCell)
                .AddTo(this);
        }

        private void UpdateCell(ShadowCellUpdate update) => Cells[update.CellIndex].SetState(update.State);
    }
}