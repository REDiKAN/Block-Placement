using UnityEngine;
using Game.Attributes;

namespace Game.Data.Ranked
{
    [CreateAssetMenu(fileName = nameof(RankedBlocksConfig), menuName = "Game/Ranked/" + nameof(RankedBlocksConfig))]
    public class RankedBlocksConfig : ScriptableObject
    {
        [field: Title("MMR Settings", CustomColor.Cyan, CustomColor.Blue)]
        [field: SerializeField] public int StartingMmr { get; private set; } = 1000;
        [field: SerializeField] public int MinimumMmr { get; private set; } = 0;
        [field: SerializeField] public string MmrPlayerPrefsKey { get; private set; } = "ranked_mmr";

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

        [field: Title("Validation", CustomColor.Green, CustomColor.DarkGreen)]
        [field: SerializeField] public float MetricMatchThreshold { get; private set; } = 0.7f;
        [field: SerializeField] public float BlockEfficiencyThreshold { get; private set; } = 0.9f;

        [field: Title("Archetype Profiles", CustomColor.Green, CustomColor.DarkGreen)]
        [field: SerializeField] public BlocksArchetypeProfile[] ArchetypeProfiles { get; private set; }
    }
}