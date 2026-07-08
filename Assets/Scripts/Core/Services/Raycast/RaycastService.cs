using UnityEngine;
using Zenject;
using Game.Data;
using Game.Services.Grid;

namespace Game.Services.Raycast
{
    public interface IRaycastService
    {
        bool TryGetTargetCell(Vector2 screenPosition, out Vector3Int cell, out Vector3Int normal);
    }

    public class RaycastService : IRaycastService
    {
        private readonly Camera _camera;
        private readonly IGridService _gridService;
        private readonly RaycastConfig _config;

        public RaycastService(Camera camera, IGridService gridService, RaycastConfig config)
        {
            _camera = camera;
            _gridService = gridService;
            _config = config;
        }

        public bool TryGetTargetCell(Vector2 screenPosition, out Vector3Int cell, out Vector3Int normal)
        {
            cell = default;
            normal = default;
            var ray = _camera.ScreenPointToRay(screenPosition);

            if (Physics.Raycast(ray, out var blockHit, _config.MaxDistance, _config.BlockMask))
            {
                var targetPos = blockHit.point + blockHit.normal * 0.5f;
                cell = new Vector3Int(
                    Mathf.FloorToInt(targetPos.x),
                    Mathf.FloorToInt(targetPos.y),
                    Mathf.FloorToInt(targetPos.z)
                );
                normal = Vector3Int.RoundToInt(blockHit.normal);
                return _gridService.IsWithinBounds(cell) && !_gridService.IsCellOccupied(cell);
            }

            if (Physics.Raycast(ray, out var floorHit, _config.MaxDistance, _config.FloorMask))
            {
                var cell2D = new Vector2Int(
                    Mathf.FloorToInt(floorHit.point.x),
                    Mathf.FloorToInt(floorHit.point.z)
                );
                cell = new Vector3Int(cell2D.x, 0, cell2D.y);
                normal = Vector3Int.up;
                return _gridService.IsWithinBounds(cell)
                       && !_gridService.IsCellOccupied(cell)
                       && _gridService.IsFloorExists(cell2D);
            }

            return false;
        }
    }
}