#if UNITY_EDITOR
using UnityEngine;
using Game.Data;

namespace Game.Views
{
    [ExecuteInEditMode]
    public class GridEditorVisualizer : MonoBehaviour
    {
        private const int GridSize = 5;
        private const float CellSize = 1f;
        private static readonly Vector3 CellOffset = new(0.5f, 0.5f, 0.5f);

        [field: SerializeField] private LevelConfig Config { get; set; }
        [field: SerializeField] private bool ShowGrid { get; set; } = true;
        [field: SerializeField] private bool ShowInitialBlocks { get; set; } = true;

        private void OnDrawGizmos()
        {
            if (ShowGrid) DrawGrid();
            if (ShowInitialBlocks) DrawInitialBlocks();
        }

        private void DrawGrid()
        {
            Gizmos.color = new Color(1f, 1f, 1f, 0.2f);
            for (var x = 0; x < GridSize; x++)
                for (var y = 0; y < GridSize; y++)
                    for (var z = 0; z < GridSize; z++)
                    {
                        var center = new Vector3Int(x, y, z) + CellOffset;
                        Gizmos.DrawWireCube(center, Vector3.one * CellSize);
                    }
        }

        private void DrawInitialBlocks()
        {
            if (Config?.InitialBlocks is null) return;
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            foreach (var block in Config.InitialBlocks)
            {
                var center = block + CellOffset;
                Gizmos.DrawCube(center, Vector3.one * CellSize);
            }
        }
    }
}
#endif