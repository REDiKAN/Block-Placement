using System.Collections.Generic;
using UnityEngine;

namespace Game.Services.History
{
    public interface IBlockHistoryService
    {
        void RecordPlacement(Vector3Int cell);
        bool TryPop(out Vector3Int cell);
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

        public void Clear() => _history.Clear();
    }
}