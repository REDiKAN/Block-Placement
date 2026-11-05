using Game.Data;

namespace Game.Services.Ranked
{
    public interface IRankedLevelGeneratorService
    {
        LevelConfig GenerateLevel(int mmr, int matchLevel, int matchPoints);
    }
}