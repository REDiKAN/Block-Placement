using System.Collections.Generic;
using Game.Data.Ranked;
using UnityEngine;

namespace Game.Services.Ranked
{
    public interface IShapeGenerator
    {
        List<Vector3Int> Generate(RankedDifficultyProfile profile);
    }
}