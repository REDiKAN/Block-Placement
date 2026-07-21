using System.Collections.Generic;
using UnityEngine;
using Game.Data;

namespace Game.Services.History
{
    public readonly struct PlacementRecord
    {
        public Vector3Int Cell { get; }
        public BlockConfig Config { get; }

        public PlacementRecord(Vector3Int cell, BlockConfig config) => (Cell, Config) = (cell, config);
    }

    public interface IBlockHistoryService
    {
        bool CanUndo { get; }
        void RecordPlacement(PlacementRecord record);
        bool TryPop(out PlacementRecord record);
        void Rotate(int angle, int gridSize);
        void Clear();
    }

    public class BlockHistoryService : IBlockHistoryService
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
                var cell = items[i].Cell;
                var newCell = angle == 90
                    ? new Vector3Int(cell.z, cell.y, gridSize - 1 - cell.x)
                    : new Vector3Int(gridSize - 1 - cell.z, cell.y, cell.x);

                items[i] = new PlacementRecord(newCell, items[i].Config);
            }

            for (var i = items.Length - 1; i >= 0; i--)
                _history.Push(items[i]);
        }

        public void Clear() => _history.Clear();
    }
}