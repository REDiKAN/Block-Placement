using Game.Data.Ranked;

namespace Game.Services.Ranked
{
    public interface IProfileBuilder
    {
        RankedDifficultyProfile Build(Archetype archetype, int matchLevel, int matchPoints);
        RankedDifficultyProfile BuildFallback(Archetype archetype);
        int CalculateDifficulty(Archetype archetype, int matchLevel, int matchPoints);
        float CalculateTimeLimit(int difficulty);
    }
}