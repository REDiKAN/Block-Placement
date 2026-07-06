using UnityEngine;
using Zenject;
using Game.Services.Grid;

namespace Game.Services.Raycast
{
    public interface IRaycastService
    {
        bool TryGetTargetCell(Vector2 screenPosition, out Vector3Int cell, out Vector3Int normal);
    }

    public class RaycastService : IRaycastService
    {
        private const int GridSize = 5;

        private readonly Camera _camera;
        private readonly IGridService _gridService;
        private readonly Plane _floorPlane;

        public RaycastService(Camera camera, IGridService gridService)
        {
            _camera = camera;
            _gridService = gridService;
            _floorPlane = new Plane(Vector3.up, Vector3.zero);
        }

        public bool TryGetTargetCell(Vector2 screenPosition, out Vector3Int cell, out Vector3Int normal)
        {
            cell = default;
            normal = default;

            var ray = _camera.ScreenPointToRay(screenPosition);

            if (_floorPlane.Raycast(ray, out var floorDistance))
            {
                var hitPoint = ray.GetPoint(floorDistance);
                var floorCell = new Vector3Int(Mathf.FloorToInt(hitPoint.x), 0, Mathf.FloorToInt(hitPoint.z));

                if (IsInBounds(floorCell) && !_gridService.IsCellOccupied(floorCell) && _gridService.IsFloorExists(floorCell))
                {
                    cell = floorCell;
                    normal = Vector3Int.up;
                    return true;
                }
            }

            return TryRaycastBlocks(ray, out cell, out normal);
        }

        private bool TryRaycastBlocks(Ray ray, out Vector3Int cell, out Vector3Int normal)
        {
            cell = default;
            normal = default;

            var closestDistance = float.MaxValue;
            var found = false;

            for (var x = 0; x < GridSize; x++)
                for (var y = 0; y < GridSize; y++)
                    for (var z = 0; z < GridSize; z++)
                    {
                        var currentCell = new Vector3Int(x, y, z);
                        if (!_gridService.IsCellOccupied(currentCell)) continue;

                        var center = new Vector3(x + 0.5f, y + 0.5f, z + 0.5f);

                        if (IntersectRayAABB(ray, center, out var distance, out var hitNormal) && distance < closestDistance)
                        {
                            closestDistance = distance;
                            normal = Vector3Int.RoundToInt(hitNormal);
                            cell = currentCell + normal;
                            found = true;
                        }
                    }

            return found && IsInBounds(cell) && !_gridService.IsCellOccupied(cell);
        }

        private bool IntersectRayAABB(Ray ray, Vector3 center, out float distance, out Vector3 normal)
        {
            distance = 0;
            normal = default;

            var tMin = new Vector3(
                (center.x - 0.5f - ray.origin.x) / ray.direction.x,
                (center.y - 0.5f - ray.origin.y) / ray.direction.y,
                (center.z - 0.5f - ray.origin.z) / ray.direction.z
            );

            var tMax = new Vector3(
                (center.x + 0.5f - ray.origin.x) / ray.direction.x,
                (center.y + 0.5f - ray.origin.y) / ray.direction.y,
                (center.z + 0.5f - ray.origin.z) / ray.direction.z
            );

            var t1 = new Vector3(Mathf.Min(tMin.x, tMax.x), Mathf.Min(tMin.y, tMax.y), Mathf.Min(tMin.z, tMax.z));
            var t2 = new Vector3(Mathf.Max(tMin.x, tMax.x), Mathf.Max(tMin.y, tMax.y), Mathf.Max(tMin.z, tMax.z));

            var tNear = Mathf.Max(Mathf.Max(t1.x, t1.y), t1.z);
            var tFar = Mathf.Min(Mathf.Min(t2.x, t2.y), t2.z);

            if (tNear > tFar || tFar < 0) return false;

            distance = tNear;

            if (t1.x > t1.y && t1.x > t1.z) normal = ray.direction.x > 0 ? Vector3.left : Vector3.right;
            else if (t1.y > t1.z) normal = ray.direction.y > 0 ? Vector3.down : Vector3.up;
            else normal = ray.direction.z > 0 ? Vector3.back : Vector3.forward;

            return true;
        }

        private bool IsInBounds(Vector3Int cell) =>
            cell.x >= 0 && cell.x < GridSize &&
            cell.y >= 0 && cell.y < GridSize &&
            cell.z >= 0 && cell.z < GridSize;
    }
}