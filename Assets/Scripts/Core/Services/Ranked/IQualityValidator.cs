using Game.Data;
using Game.Data.Ranked;

namespace Game.Services.Ranked
{
    public interface IQualityValidator
    {
        RankedValidationReport Validate(LevelConfig config, RankedDifficultyProfile targetProfile);
        bool IsAcceptable(RankedValidationReport report, RankedDifficultyProfile targetProfile);
    }
}