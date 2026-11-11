using UnityEngine;
using Game.Attributes;
using Game.Data;

namespace Game.Data.Ranked
{
    [CreateAssetMenu(fileName = nameof(RankedStructuresConfig), menuName = "Game/Ranked/" + nameof(RankedStructuresConfig))]
    public class RankedStructuresConfig : ScriptableObject
    {
        [field: Title("MMR Settings", CustomColor.Cyan, CustomColor.Blue)]
        [field: SerializeField] public int StartingMmr { get; private set; } = 1000;
        [field: SerializeField] public int MinimumMmr { get; private set; } = 0;
        [field: SerializeField] public string MmrPlayerPrefsKey { get; private set; } = "ranked_mmr_structures";

        [field: Title("Scoring", CustomColor.Yellow, CustomColor.Orange)]
        [field: SerializeField] public int BaseReward { get; private set; } = 20;
        [field: SerializeField] public float RewardPerDifficulty { get; private set; } = 4f;
        [field: SerializeField] public float TimeBonusMultiplier { get; private set; } = 25f;

        [field: Title("Penalty Settings", CustomColor.Red, CustomColor.Orange)]
        [field: SerializeField] public int BasePenalty { get; private set; } = 25;
        [field: SerializeField] public float PenaltyPerDifficulty { get; private set; } = 6f;

        [field: Title("Timer", CustomColor.Red, CustomColor.Orange)]
        [field: SerializeField] public float BaseTimerSeconds { get; private set; } = 150f;
        [field: SerializeField] public float TimerDecayFactor { get; private set; } = 0.8f;
        [field: SerializeField] public float MinimumTimerSeconds { get; private set; } = 60f;

        [field: Title("Validation", CustomColor.Green, CustomColor.DarkGreen)]
        [field: SerializeField] public float MetricMatchThreshold { get; private set; } = 0.5f;
        [field: SerializeField] public float BlockEfficiencyThreshold { get; private set; } = 0.5f;

        [field: Title("Archetype Profiles", CustomColor.Green, CustomColor.DarkGreen)]
        [field: SerializeField] public StructuresArchetypeProfile[] ArchetypeProfiles { get; private set; }

        [field: Title("Structure Pool", CustomColor.Cyan, CustomColor.Blue)]
        [field: SerializeField] public StructureConfig[] AvailableStructures { get; private set; }
    }
}