using System;
using UniRx;
using UnityEngine;
using Zenject;
using Game.Data;

namespace Game.Services.Grid
{
    public interface IGridService
    {
        int GridSize { get; }
        IObservable<Vector2Int> OnFloorCellChanged { get; }
        bool IsCellOccupied(Vector3Int cell);
        void SetCellOccupied(Vector3Int cell, bool isOccupied);
        bool IsWithinBounds(Vector3Int cell);
        bool IsFloorExists(Vector2Int cell);
        void SetFloorExists(Vector2Int cell, bool exists);
        void Rotate(int angle);
    }

    public class GridService : IGridService, IInitializable
    {
        public int GridSize => Size;
        public IObservable<Vector2Int> OnFloorCellChanged => _onFloorCellChanged;

        private const int Size = 5;
        private const int CellCount = 25;
        private readonly bool[,,] _cells = new bool[Size, Size, Size];
        private readonly bool[] _floor = new bool[CellCount];
        private readonly Subject<Vector2Int> _onFloorCellChanged = new();
        private readonly LevelConfig _config;
        private readonly bool _isDeveloperMode;

        public GridService(LevelConfig config, [Inject(Id = "IsDeveloperMode")] bool isDeveloperMode)
        {
            _config = config;
            _isDeveloperMode = isDeveloperMode;
        }

        public void Initialize()
        {
            Array.Clear(_cells, 0, _cells.Length);
            if (_isDeveloperMode)
                Array.Fill(_floor, true);
            else if (_config?.FloorMatrix?.Length == CellCount)
                Array.Copy(_config.FloorMatrix, _floor, CellCount);
            else
                Array.Fill(_floor, true);
        }

        public bool IsCellOccupied(Vector3Int cell) =>
            IsWithinBounds(cell) && _cells[cell.x, cell.y, cell.z];

        public void SetCellOccupied(Vector3Int cell, bool isOccupied)
        {
            if (IsWithinBounds(cell))
                _cells[cell.x, cell.y, cell.z] = isOccupied;
        }

        public bool IsWithinBounds(Vector3Int cell) =>
            cell.x >= 0 && cell.x < Size &&
            cell.y >= 0 && cell.y < Size &&
            cell.z >= 0 && cell.z < Size;

        public bool IsFloorExists(Vector2Int cell) =>
            cell.x >= 0 && cell.x < Size &&
            cell.y >= 0 && cell.y < Size &&
            _floor[cell.x * Size + cell.y];

        public void SetFloorExists(Vector2Int cell, bool exists)
        {
            if (cell.x < 0 || cell.x >= Size || cell.y < 0 || cell.y >= Size) return;
            var index = cell.x * Size + cell.y;
            if (_floor[index] != exists)
            {
                _floor[index] = exists;
                _onFloorCellChanged.OnNext(cell);
            }
        }

        public void Rotate(int angle)
        {
            var newCells = new bool[Size, Size, Size];
            for (var x = 0; x < Size; x++)
                for (var y = 0; y < Size; y++)
                    for (var z = 0; z < Size; z++)
                    {
                        if (!_cells[x, y, z]) continue;
                        var (nx, nz) = angle == 90 ? (z, Size - 1 - x) : (Size - 1 - z, x);
                        newCells[nx, y, nz] = true;
                    }
            Array.Copy(newCells, _cells, _cells.Length);
        }
    }
}