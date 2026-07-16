using UnityEngine;
using Zenject;
using UniRx;
using Game.Services.Grid;
using Game.Views;

namespace Game.Views.Preview
{
    public class PreviewFloorGridView : MonoBehaviour
    {
        [field: SerializeField] public FloorCellView[] Cells { get; private set; }

        [Inject] private IGridService _gridService;
        private readonly CompositeDisposable _disposables = new();

        private void Start()
        {
            AssignIndices();
            SyncAllCells();

            if (_gridService is not null)
            {
                _gridService.OnFloorCellChanged
                    .Subscribe(UpdateCell)
                    .AddTo(_disposables);
            }
        }

        private void SyncAllCells()
        {
            if (Cells is null || _gridService is null) return;
            foreach (var cell in Cells)
            {
                if (cell is null) continue;
                var x = cell.Index / 5;
                var z = cell.Index % 5;
                cell.SetVisible(_gridService.IsFloorExists(new Vector2Int(x, z)));
            }
        }

        private void UpdateCell(Vector2Int cellCoord)
        {
            if (_gridService is null) return;
            var index = cellCoord.x * 5 + cellCoord.y;
            if (index >= 0 && index < Cells.Length && Cells[index] is not null)
                Cells[index].SetVisible(_gridService.IsFloorExists(cellCoord));
        }

        private void AssignIndices()
        {
            if (Cells is null) return;
            foreach (var cell in Cells)
            {
                if (cell is null) continue;
                var x = Mathf.FloorToInt(cell.transform.localPosition.x);
                var z = Mathf.FloorToInt(cell.transform.localPosition.z);
                cell.Index = x * 5 + z;
            }
        }

        private void OnDestroy() => _disposables?.Dispose();
    }
}