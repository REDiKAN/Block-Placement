using System;
using UnityEngine;
using Zenject;

namespace Game.Services.Grid
{
    public interface IGridService
    {
        bool IsCellOccupied(Vector3Int cell);
        void SetCellOccupied(Vector3Int cell, bool isOccupied);
        bool IsWithinBounds(Vector3Int cell);
    }

    public class GridService : IGridService, IInitializable
    {
        private const int Size = 5;
        private readonly bool[,,] _cells = new bool[Size, Size, Size];

        public void Initialize() => Array.Clear(_cells, 0, _cells.Length);

        public bool IsCellOccupied(Vector3Int cell) =>
            IsWithinBounds(cell) && _cells[cell.x, cell.y, cell.z];

        public void SetCellOccupied(Vector3Int cell, bool isOccupied)
        {
            if (IsWithinBounds(cell)) _cells[cell.x, cell.y, cell.z] = isOccupied;
        }

        public bool IsWithinBounds(Vector3Int cell) =>
            cell.x >= 0 && cell.x < Size &&
            cell.y >= 0 && cell.y < Size &&
            cell.z >= 0 && cell.z < Size;
    }
}