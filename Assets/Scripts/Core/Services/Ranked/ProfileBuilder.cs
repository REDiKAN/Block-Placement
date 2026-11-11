using Game.Core;
using Game.Data;
using Game.Data.Ranked;
using UnityEngine;

namespace Game.Services.Ranked
{
    public class ProfileBuilder : IProfileBuilder
    {
        private readonly RankedCoreConfig _coreConfig;
        private readonly RankedBlocksConfig _blocksConfig;
        private readonly RankedStructuresConfig _structuresConfig;
        private readonly IArchetypeResolver _archetypeResolver;

        private const int MaxMatchLevel = 5;

        public ProfileBuilder(
            RankedCoreConfig coreConfig,
            RankedBlocksConfig blocksConfig,
            RankedStructuresConfig structuresConfig,
            IArchetypeResolver archetypeResolver)
        {
            _coreConfig = coreConfig;
            _blocksConfig = blocksConfig;
            _structuresConfig = structuresConfig;
            _archetypeResolver = archetypeResolver;
        }

        public RankedDifficultyProfile Build(Archetype archetype, int matchLevel, int matchPoints)
        {
            var baseProfile = GetBaseProfile(archetype);
            var progression = CalculateProgression(matchLevel);
            var profile = ApplyProgression(baseProfile, progression);

            if (matchLevel == MaxMatchLevel && matchPoints > 0)
                profile = ApplyLevel5Bonus(profile, matchPoints);

            return profile;
        }

        public RankedDifficultyProfile BuildFallback(Archetype archetype) =>
            GetBaseProfile(archetype);

        public int CalculateDifficulty(Archetype archetype, int matchLevel, int matchPoints)
        {
            var baseDifficulty = _archetypeResolver.GetBaseDifficulty(archetype);
            var progressionBonus = (matchLevel - 1) * 0.5f;
            var pointsBonus = matchPoints / 100f;
            var difficulty = baseDifficulty + progressionBonus + pointsBonus;
            return Mathf.Clamp(Mathf.RoundToInt(difficulty), 1, 10);
        }

        public float CalculateTimeLimit(int difficulty)
        {
            var isStructures = RankedContext.CurrentMode == GameMode.Structures;
            var baseTime = isStructures ? _structuresConfig.BaseTimerSeconds : _blocksConfig.BaseTimerSeconds;
            var decay = isStructures ? _structuresConfig.TimerDecayFactor : _blocksConfig.TimerDecayFactor;
            var minTime = isStructures ? _structuresConfig.MinimumTimerSeconds : _blocksConfig.MinimumTimerSeconds;

            var calculated = baseTime - (difficulty * difficulty * decay);
            return Mathf.Max(minTime, calculated);
        }

        private RankedDifficultyProfile GetBaseProfile(Archetype archetype)
        {
            var resolved = FindArchetypeProfile(archetype);
            return resolved?.ToProfile() ?? CreateDefaultProfile();
        }

        private IRankedArchetypeProfile FindArchetypeProfile(Archetype archetype)
        {
            if (RankedContext.CurrentMode == GameMode.Structures)
            {
                var profiles = _structuresConfig?.ArchetypeProfiles;
                if (profiles is null) return null;

                foreach (var profile in profiles)
                {
                    if (profile is IRankedArchetypeProfile ranked && ranked.Archetype == archetype)
                        return ranked;
                }
                return null;
            }

            var blockProfiles = _blocksConfig?.ArchetypeProfiles;
            if (blockProfiles is null) return null;

            foreach (var profile in blockProfiles)
            {
                if (profile is IRankedArchetypeProfile ranked && ranked.Archetype == archetype)
                    return ranked;
            }
            return null;
        }

        private static RankedDifficultyProfile CreateDefaultProfile() =>
            new(0.3f, 0.1f, 0.2f, 0f, 0.2f, 0.7f, 10);

        private static float CalculateProgression(int matchLevel) =>
            (matchLevel - 1) / (float)(MaxMatchLevel - 1);

        private static RankedDifficultyProfile ApplyProgression(RankedDifficultyProfile baseProfile, float progression)
        {
            var verticality = baseProfile.Verticality * (0.5f + 0.5f * progression);
            var overhang = baseProfile.OverhangRatio * progression;
            var ambiguity = baseProfile.Ambiguity * progression;
            var density = baseProfile.DensityComplexity * progression;
            var asymmetry = baseProfile.Asymmetry * progression;
            var compactness = baseProfile.Compactness * (1f - 0.3f * progression);

            return new RankedDifficultyProfile(
                verticality, overhang, ambiguity, density, asymmetry, compactness, baseProfile.TargetBlockCount);
        }

        private RankedDifficultyProfile ApplyLevel5Bonus(RankedDifficultyProfile profile, int matchPoints)
        {
            var bonus = Mathf.Clamp01(matchPoints / _coreConfig.Level5PointsDivisor);
            var metricBonus = _coreConfig.Level5MetricBonus;
            return profile.WithBonus(bonus * metricBonus, bonus * metricBonus, bonus * metricBonus);
        }
    }
}