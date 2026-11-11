using Game.Data.Ranked;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Services.Ranked
{
    public class DecoyPlacementService : IDecoyPlacementService
    {
        private readonly RankedConfig _config;
        private readonly HashSet<Vector3Int> _mainStructureSet;
        private readonly List<Vector3Int> _validDecoyPositions;

        public DecoyPlacementService(RankedConfig config)
        {
            _config = config;
            _mainStructureSet = new HashSet<Vector3Int>();
            _validDecoyPositions = new List<Vector3Int>();
        }

        public List<Vector3Int> GenerateDecoys(List<Vector3Int> mainStructure, bool[] targetWallYZ, bool[] targetWallXY, int decoyCount)
        {
            _mainStructureSet.Clear();
            _validDecoyPositions.Clear();

            foreach (var block in mainStructure)
            {
                _mainStructureSet.Add(block);
            }

            for (var x = 0; x < _config.GridSize; x++)
            {
                for (var y = 0; y < _config.GridSize; y++)
                {
                    for (var z = 0; z < _config.GridSize; z++)
                    {
                        var cell = new Vector3Int(x, y, z);
                        if (_mainStructureSet.Contains(cell)) continue;

                        var yzIndex = y * _config.GridSize + z;
                        var xyIndex = x * _config.GridSize + y;

                        if (targetWallYZ[yzIndex] && targetWallXY[xyIndex])
                        {
                            _validDecoyPositions.Add(cell);
                        }
                    }
                }
            }

            var result = new List<Vector3Int>();
            var available = new List<Vector3Int>(_validDecoyPositions);

            for (var i = 0; i < decoyCount && available.Count > 0; i++)
            {
                var idx = Random.Range(0, available.Count);
                result.Add(available[idx]);
                available.RemoveAt(idx);
            }

            return result;
        }
    }
}