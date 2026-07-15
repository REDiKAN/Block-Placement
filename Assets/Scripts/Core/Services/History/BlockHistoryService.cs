using System.Collections.Generic;
using UnityEngine;

namespace Game.Services.History
{
    public interface IBlockHistoryService
    {
        void RecordPlacement(Vector3Int cell);
        bool TryPop(out Vector3Int cell);
        bool TryPeek(out Vector3Int cell);
        void Rotate(int angle, int gridSize);
        void Clear();
    }

    public class BlockHistoryService : IBlockHistoryService
    {
        private readonly Stack<Vector3Int> _history = new();

        public void RecordPlacement(Vector3Int cell) => _history.Push(cell);

        public bool TryPop(out Vector3Int cell)
        {
            if (_history.Count == 0)
            {
                cell = default;
                return false;
            }
            cell = _history.Pop();
            return true;
        }

        public bool TryPeek(out Vector3Int cell)
        {
            if (_history.Count == 0)
            {
                cell = default;
                return false;
            }
            cell = _history.Peek();
            return true;
        }

        public void Rotate(int angle, int gridSize)
        {
            if (_history.Count == 0) return;

            var items = _history.ToArray();
            _history.Clear();

            for (var i = 0; i < items.Length; i++)
            {
                var cell = items[i];
                items[i] = angle == 90
                    ? new Vector3Int(cell.z, cell.y, gridSize - 1 - cell.x)
                    : new Vector3Int(gridSize - 1 - cell.z, cell.y, cell.x);
            }

            for (var i = items.Length - 1; i >= 0; i--)
                _history.Push(items[i]);
        }

        public void Clear() => _history.Clear();
    }
}