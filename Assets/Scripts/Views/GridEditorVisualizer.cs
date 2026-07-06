#if UNITY_EDITOR
using UnityEngine;

namespace Game.Views
{
    [ExecuteInEditMode]
    public class GridEditorVisualizer : MonoBehaviour
    {
        private const int GridSize = 5;
        private const float CellSize = 1f;
        private static readonly Vector3 CellOffset = new(0.5f, 0.5f, 0.5f);

        private void OnDrawGizmos()
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
    }
}
#endif
