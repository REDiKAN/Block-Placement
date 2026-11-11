using System.Collections.Generic;
using Game.Data.Ranked;
using UnityEngine;

namespace Game.Services.Ranked
{
    public class StructureShapeGenerator : IStructureShapeGenerator
    {
        private readonly RankedConfig _config;
        private readonly HashSet<Vector3Int> _shape;
        private readonly List<Vector3Int> _available;

        public StructureShapeGenerator(RankedConfig config)
        {
            _config = config;
            _shape = new HashSet<Vector3Int>();
            _available = new List<Vector3Int>();
        }

        public List<Vector3Int> GenerateStructure(RankedDifficultyProfile profile)
        {
            _shape.Clear();
            _available.Clear();

            var targetCount = Mathf.Max(3, Mathf.RoundToInt(profile.TargetBlockCount * 0.7f));
            var seedX = Random.Range(0, _config.GridSize);
            var seedZ = Random.Range(0, _config.GridSize);
            var seed = new Vector3Int(seedX, 0, seedZ);

            _shape.Add(seed);
            AddNeighbors(seed);

            while (_shape.Count < targetCount && _available.Count > 0)
            {
                var idx = Random.Range(0, _available.Count);
                var candidate = _available[idx];
                _available.RemoveAt(idx);

                if (EvaluateCandidate(candidate, profile))
                {
                    _shape.Add(candidate);
                    AddNeighbors(candidate);
                }
            }

            return new List<Vector3Int>(_shape);
        }

        private void AddNeighbors(Vector3Int cell)
        {
            var directions = new[]
            {
                Vector3Int.up, Vector3Int.down,
                Vector3Int.left, Vector3Int.right,
                Vector3Int.forward, Vector3Int.back
            };

            foreach (var dir in directions)
            {
                var next = cell + dir;
                if (next.x >= 0 && next.x < _config.GridSize &&
                    next.y >= 0 && next.y < _config.GridSize &&
                    next.z >= 0 && next.z < _config.GridSize)
                {
                    if (!_shape.Contains(next) && !_available.Contains(next))
                    {
                        _available.Add(next);
                    }
                }
            }
        }

        private bool EvaluateCandidate(Vector3Int candidate, RankedDifficultyProfile profile)
        {
            if (candidate.y > 0 && Random.value > profile.Verticality * 0.5f + 0.5f)
                return false;

            var centerDistance = Mathf.Abs(candidate.x - 2) + Mathf.Abs(candidate.z - 2);
            if (centerDistance > 2 && Random.value > profile.Asymmetry)
                return false;

            return true;
        }
    }
}