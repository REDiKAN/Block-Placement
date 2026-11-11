using UnityEngine;
using Game.Attributes;
using Game.Data;

namespace Game.Data.Ranked
{
    [CreateAssetMenu(fileName = nameof(RankedConfig), menuName = "Game/Ranked/" + nameof(RankedConfig))]
    public class RankedConfig : ScriptableObject
    {
        [field: Title("MMR Settings", CustomColor.Cyan, CustomColor.Blue)]
        [field: SerializeField] public int StartingMmr { get; private set; } = 1000;
        [field: SerializeField] public int MinimumMmr { get; private set; } = 0;
        [field: SerializeField] public string MmrPlayerPrefsKey { get; private set; } = "ranked_mmr";
        [field: SerializeField] public string MmrStructuresPlayerPrefsKey { get; private set; } = "ranked_mmr_structures";

        [field: Title("Match Settings", CustomColor.Green, CustomColor.DarkGreen)]
        [field: SerializeField] public int LevelsPerMatch { get; private set; } = 5;
        [field: SerializeField] public int Level5Bonus { get; private set; } = 20;
        [field: SerializeField] public int PerfectRunBonus { get; private set; } = 50;

        [field: Title("Scoring", CustomColor.Yellow, CustomColor.Orange)]
        [field: SerializeField] public int BaseReward { get; private set; } = 15;
        [field: SerializeField] public float RewardPerDifficulty { get; private set; } = 3f;
        [field: SerializeField] public float TimeBonusMultiplier { get; private set; } = 20f;

        [field: Title("Penalty Settings", CustomColor.Red, CustomColor.Orange)]
        [field: SerializeField] public int BasePenalty { get; private set; } = 20;
        [field: SerializeField] public float PenaltyPerDifficulty { get; private set; } = 5f;

        [field: Title("Timer", CustomColor.Red, CustomColor.Orange)]
        [field: SerializeField] public float BaseTimerSeconds { get; private set; } = 120f;
        [field: SerializeField] public float TimerDecayFactor { get; private set; } = 0.8f;
        [field: SerializeField] public float MinimumTimerSeconds { get; private set; } = 40f;

        [field: Title("Generation", CustomColor.Cyan, CustomColor.Blue)]
        [field: SerializeField] public int MaxGenerationAttempts { get; private set; } = 50;
        [field: SerializeField] public int MaxSolverSteps { get; private set; } = 1000;
        [field: SerializeField] public float MetricMatchThreshold { get; private set; } = 0.7f;
        [field: SerializeField] public float BlockEfficiencyThreshold { get; private set; } = 0.9f;
        [field: SerializeField] public int GridSize { get; private set; } = 5;
        [field: SerializeField] public StructureConfig[] AvailableStructures { get; private set; }

        [field: Title("Level 5 Bonus Multipliers", CustomColor.Yellow, CustomColor.Orange)]
        [field: SerializeField] public float Level5PointsDivisor { get; private set; } = 200f;
        [field: SerializeField] public float Level5MetricBonus { get; private set; } = 0.2f;

        [field: Title("Archetype Profiles", CustomColor.Green, CustomColor.DarkGreen)]
        [field: SerializeField] public ArchetypeProfileData[] ArchetypeProfiles { get; private set; }
    }

    [System.Serializable]
    public class ArchetypeProfileData
    {
        [field: SerializeField] public Archetype Archetype { get; private set; }
        [field: SerializeField] public int MinMmr { get; private set; }
        [field: SerializeField] public int MaxMmr { get; private set; }
        [field: SerializeField] public int BaseDifficulty { get; private set; }
        [field: SerializeField] public float Verticality { get; private set; }
        [field: SerializeField] public float OverhangRatio { get; private set; }
        [field: SerializeField] public float Ambiguity { get; private set; }
        [field: SerializeField] public float DensityComplexity { get; private set; }
        [field: SerializeField] public float Asymmetry { get; private set; }
        [field: SerializeField] public float Compactness { get; private set; }
        [field: SerializeField] public int MinBlockCount { get; private set; }
        [field: SerializeField] public int MaxBlockCount { get; private set; }
        [field: SerializeField] public bool IsBlockLimitEnabled { get; private set; }
        [field: SerializeField] public bool IsRotationRequired { get; private set; }
        [field: SerializeField] public int BlockLimitPadding { get; private set; }
        [field: SerializeField] public bool UseDecoyStructures { get; private set; }
        [field: SerializeField] public int MinDecoyCount { get; private set; }
        [field: SerializeField] public int MaxDecoyCount { get; private set; }

        public RankedDifficultyProfile ToProfile() =>
            new(
                Verticality,
                OverhangRatio,
                Ambiguity,
                DensityComplexity,
                Asymmetry,
                Compactness,
                UnityEngine.Random.Range(MinBlockCount, MaxBlockCount + 1));
    }
}