using UnityEngine;
using Zenject;
using Game.Data;

namespace Game.Views
{
    public class FloorGridView : MonoBehaviour
    {
        [field: SerializeField] public FloorCellView[] Cells { get; private set; }

        [Inject]
        private void Construct(ShadowLevelConfig config)
        {
            AssignIndices();
            ApplyConfig(config);
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

        private void ApplyConfig(ShadowLevelConfig config)
        {
            if (config?.FloorMatrix is null || Cells is null) return;

            foreach (var cell in Cells)
            {
                if (cell is null || cell.Index < 0 || cell.Index >= config.FloorMatrix.Length) continue;

                cell.SetVisible(config.FloorMatrix[cell.Index]);
            }
        }
    }
}