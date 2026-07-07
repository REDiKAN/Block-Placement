using System;
using UnityEngine;
using Zenject;
using Game.Data;

namespace Game.Services.Grid
{
    public interface IGridService
    {
        bool IsCellOccupied(Vector3Int cell);
        void SetCellOccupied(Vector3Int cell, bool isOccupied);
        bool IsWithinBounds(Vector3Int cell);
        bool IsFloorExists(Vector3Int cell);
    }

    public class GridService : IGridService, IInitializable
    {
        private const int Size = 5;
        private const int CellCount = 25;

        private readonly bool[,,] _cells = new bool[Size, Size, Size];
        private readonly bool[] _floor = new bool[CellCount];
        private readonly ShadowLevelConfig _config;
        private readonly bool _isDeveloperMode;

        public GridService(ShadowLevelConfig config, [Inject(Id = "IsDeveloperMode")] bool isDeveloperMode)
        {
            _config = config;
            _isDeveloperMode = isDeveloperMode;
        }

        public void Initialize()
        {
            Array.Clear(_cells, 0, _cells.Length);

            if (_isDeveloperMode)
            {
                Array.Fill(_floor, true);
                Debug.Log("[GridService] Developer mode active. Floor fully initialized without holes.");
            }
            else
            {
                if (_config?.FloorMatrix?.Length == CellCount)
                    Array.Copy(_config.FloorMatrix, _floor, CellCount);
                else
                    Array.Fill(_floor, true);

                Debug.Log("[GridService] Normal mode active. Floor initialized from configuration.");
            }
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

        public bool IsFloorExists(Vector3Int cell) =>
            cell.x >= 0 && cell.x < Size &&
            cell.z >= 0 && cell.z < Size &&
            _floor[cell.x * Size + cell.z];
    }
}