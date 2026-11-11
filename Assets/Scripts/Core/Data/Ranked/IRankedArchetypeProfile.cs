namespace Game.Data.Ranked
{
    public interface IRankedArchetypeProfile
    {
        Archetype Archetype { get; }
        int MinMmr { get; }
        int MaxMmr { get; }
        int BaseDifficulty { get; }
        bool IsBlockLimitEnabled { get; }
        int BlockLimitPadding { get; }
        bool IsRotationRequired { get; }
        RankedDifficultyProfile ToProfile();
    }
}