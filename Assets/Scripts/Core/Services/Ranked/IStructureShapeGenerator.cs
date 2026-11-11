using System.Collections.Generic;
using Game.Data.Ranked;
using UnityEngine;

namespace Game.Services.Ranked
{
    public interface IStructureShapeGenerator
    {
        List<Vector3Int> GenerateStructure(RankedDifficultyProfile profile);
    }
}