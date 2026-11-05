using Game.Data.Ranked;
using UnityEngine;

namespace Game.Services.Ranked
{
    public class ProfileBuilder : IProfileBuilder
    {
        private readonly RankedConfig _config;
        private readonly IArchetypeResolver _archetypeResolver;

        private const int MaxMatchLevel = 5;
        private const float MinTimeLimit = 40f;
        private const float BaseTimeSeconds = 120f;

        public ProfileBuilder(RankedConfig config, IArchetypeResolver archetypeResolver)
        {
            _config = config;
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
            var baseTime = BaseTimeSeconds - (difficulty * difficulty * _config.TimerDecayFactor);
            return Mathf.Max(MinTimeLimit, baseTime);
        }

        private RankedDifficultyProfile GetBaseProfile(Archetype archetype)
        {
            var profileData = FindArchetypeProfile(archetype);
            return profileData?.ToProfile() ?? CreateDefaultProfile();
        }

        private ArchetypeProfileData FindArchetypeProfile(Archetype archetype)
        {
            if (_config?.ArchetypeProfiles is null)
                return null;

            foreach (var profile in _config.ArchetypeProfiles)
            {
                if (profile.Archetype == archetype)
                    return profile;
            }

            return null;
        }

        private static RankedDifficultyProfile CreateDefaultProfile() =>
            new(0.3f, 0.1f, 0.2f, 0f, 0.2f, 0.7f, 10);

        private static float CalculateProgression(int matchLevel) =>
            (matchLevel - 1) / (float)(MaxMatchLevel - 1);

        private static RankedDifficultyProfile ApplyProgression(
            RankedDifficultyProfile baseProfile,
            float progression)
        {
            var verticality = baseProfile.Verticality * (0.5f + 0.5f * progression);
            var overhang = baseProfile.OverhangRatio * progression;
            var ambiguity = baseProfile.Ambiguity * progression;
            var density = baseProfile.DensityComplexity * progression;
            var asymmetry = baseProfile.Asymmetry * progression;
            var compactness = baseProfile.Compactness * (1f - 0.3f * progression);

            return new RankedDifficultyProfile(
                verticality,
                overhang,
                ambiguity,
                density,
                asymmetry,
                compactness,
                baseProfile.TargetBlockCount);
        }

        private RankedDifficultyProfile ApplyLevel5Bonus(
            RankedDifficultyProfile profile,
            int matchPoints)
        {
            var bonus = Mathf.Clamp01(matchPoints / _config.Level5PointsDivisor);
            var metricBonus = _config.Level5MetricBonus;

            return profile.WithBonus(
                bonus * metricBonus,
                bonus * metricBonus,
                bonus * metricBonus);
        }
    }
}