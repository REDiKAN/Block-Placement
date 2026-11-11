using System.Collections.Generic;
using UnityEngine;

namespace Game.Services.Ranked
{
    public interface IDecoyPlacementService
    {
        List<Vector3Int> GenerateDecoys(List<Vector3Int> mainStructure, bool[] targetWallYZ, bool[] targetWallXY, int decoyCount);
    }
}