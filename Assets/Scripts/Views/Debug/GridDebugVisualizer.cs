using UnityEngine;
using Zenject;
using Game.Services.Grid;

namespace Game.Tools.Debug
{
    public class GridDebugVisualizer : MonoBehaviour
    {
        [Inject] private IGridService _gridService;

        [field: SerializeField] private Color OccupiedColor { get; set; } = new(1f, 0f, 0f, 0.4f);
        [field: SerializeField] private Color EmptyFloorColor { get; set; } = new(0.2f, 0.2f, 0.2f, 0.4f);
        [field: SerializeField] private bool DrawWireGrid { get; set; } = true;

        private static readonly Vector3 CellOffset = new(0.5f, 0.5f, 0.5f);

        private void OnDrawGizmos()
        {
            if (_gridService is null) return;

            var gridSize = _gridService.GridSize;

            if (DrawWireGrid)
            {
                Gizmos.color = new Color(1f, 1f, 1f, 0.15f);
                for (var x = 0; x < gridSize; x++)
                    for (var y = 0; y < gridSize; y++)
                        for (var z = 0; z < gridSize; z++)
                            Gizmos.DrawWireCube(new Vector3Int(x, y, z) + CellOffset, Vector3.one);
            }

            for (var x = 0; x < gridSize; x++)
            {
                for (var y = 0; y < gridSize; y++)
                {
                    for (var z = 0; z < gridSize; z++)
                    {
                        var cell = new Vector3Int(x, y, z);
                        if (_gridService.IsCellOccupied(cell))
                        {
                            Gizmos.color = OccupiedColor;
                            Gizmos.DrawCube(cell + CellOffset, Vector3.one * 0.98f);
                        }
                    }
                }
            }

            for (var x = 0; x < gridSize; x++)
            {
                for (var z = 0; z < gridSize; z++)
                {
                    if (!_gridService.IsFloorExists(new Vector2Int(x, z)))
                    {
                        Gizmos.color = EmptyFloorColor;
                        Gizmos.DrawCube(new Vector3Int(x, -1, z) + CellOffset, Vector3.one * 0.98f);
                    }
                }
            }
        }
    }
}