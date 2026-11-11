using UnityEngine;
using Game.Attributes;

namespace Game.Data.Ranked
{
    [CreateAssetMenu(fileName = nameof(RankedCoreConfig), menuName = "Game/Ranked/" + nameof(RankedCoreConfig))]
    public class RankedCoreConfig : ScriptableObject
    {
        [field: Title("Match Settings", CustomColor.Green, CustomColor.DarkGreen)]
        [field: SerializeField] public int LevelsPerMatch { get; private set; } = 5;
        [field: SerializeField] public int Level5Bonus { get; private set; } = 20;
        [field: SerializeField] public int PerfectRunBonus { get; private set; } = 50;

        [field: Title("Level 5 Multipliers", CustomColor.Yellow, CustomColor.Orange)]
        [field: SerializeField] public float Level5PointsDivisor { get; private set; } = 200f;
        [field: SerializeField] public float Level5MetricBonus { get; private set; } = 0.2f;

        [field: Title("Core Generation", CustomColor.Cyan, CustomColor.Blue)]
        [field: SerializeField] public int GridSize { get; private set; } = 5;
        [field: SerializeField] public int MaxGenerationAttempts { get; private set; } = 50;
        [field: SerializeField] public int MaxSolverSteps { get; private set; } = 1000;
    }
}