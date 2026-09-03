using System.Collections.Generic;
using UnityEngine;

namespace Game.Services.History
{
    public readonly struct PlacementRecord
    {
        public Vector3Int[] Cells { get; }
        public ScriptableObject Config { get; }
        public PlacementRecord(Vector3Int[] cells, ScriptableObject config) => (Cells, Config) = (cells, config);
    }

    public interface IPlacementHistoryService
    {
        bool CanUndo { get; }
        void RecordPlacement(PlacementRecord record);
        bool TryPop(out PlacementRecord record);
        void Rotate(int angle, int gridSize);
        void Clear();
    }

    public class PlacementHistoryService : IPlacementHistoryService
    {
        private readonly Stack<PlacementRecord> _history = new();

        public bool CanUndo => _history.Count > 0;

        public void RecordPlacement(PlacementRecord record) => _history.Push(record);

        public bool TryPop(out PlacementRecord record) => _history.TryPop(out record);

        public void Rotate(int angle, int gridSize)
        {
            if (_history.Count == 0) return;
            var items = _history.ToArray();
            _history.Clear();
            for (var i = 0; i < items.Length; i++)
            {
                var cells = items[i].Cells;
                var newCells = new Vector3Int[cells.Length];
                for (var j = 0; j < cells.Length; j++)
                {
                    var cell = cells[j];
                    newCells[j] = angle == 90
                        ? new Vector3Int(cell.z, cell.y, gridSize - 1 - cell.x)
                        : new Vector3Int(gridSize - 1 - cell.z, cell.y, cell.x);
                }
                items[i] = new PlacementRecord(newCells, items[i].Config);
            }
            for (var i = items.Length - 1; i >= 0; i--)
                _history.Push(items[i]);
        }

        public void Clear() => _history.Clear();
    }
}