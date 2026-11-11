using Game.Core;
using Game.Data;
using Game.Data.Ranked;

namespace Game.Services.Ranked
{
    public class ArchetypeResolver : IArchetypeResolver
    {
        private readonly RankedBlocksConfig _blocksConfig;
        private readonly RankedStructuresConfig _structuresConfig;

        public ArchetypeResolver(RankedBlocksConfig blocksConfig, RankedStructuresConfig structuresConfig)
        {
            _blocksConfig = blocksConfig;
            _structuresConfig = structuresConfig;
        }

        public IRankedArchetypeProfile Resolve(int mmr)
        {
            var profiles = GetActiveProfiles();
            if (profiles is null || profiles.Length == 0)
                return GetMaxProfile();

            foreach (var profile in profiles)
            {
                if (mmr >= profile.MinMmr && mmr <= profile.MaxMmr)
                    return profile;
            }

            return GetMaxProfile();
        }

        public int GetBaseDifficulty(Archetype archetype) =>
            FindProfile(archetype)?.BaseDifficulty ?? 1;

        public bool IsRotationRequired(Archetype archetype) =>
            FindProfile(archetype)?.IsRotationRequired ?? false;

        public bool IsBlockLimitEnabled(Archetype archetype) =>
            FindProfile(archetype)?.IsBlockLimitEnabled ?? false;

        public int GetBlockLimitPadding(Archetype archetype) =>
            FindProfile(archetype)?.BlockLimitPadding ?? 0;

        private IRankedArchetypeProfile FindProfile(Archetype archetype)
        {
            var profiles = GetActiveProfiles();
            if (profiles is null) return null;

            foreach (var profile in profiles)
            {
                if (profile.Archetype == archetype)
                    return profile;
            }

            return null;
        }

        private IRankedArchetypeProfile[] GetActiveProfiles()
        {
            if (RankedContext.CurrentMode == GameMode.Structures)
            {
                var source = _structuresConfig?.ArchetypeProfiles;
                if (source is null) return null;

                var result = new IRankedArchetypeProfile[source.Length];
                for (var i = 0; i < source.Length; i++)
                    result[i] = source[i];

                return result;
            }

            var blocksSource = _blocksConfig?.ArchetypeProfiles;
            if (blocksSource is null) return null;

            var blocksResult = new IRankedArchetypeProfile[blocksSource.Length];
            for (var i = 0; i < blocksSource.Length; i++)
                blocksResult[i] = blocksSource[i];

            return blocksResult;
        }

        private IRankedArchetypeProfile GetMaxProfile()
        {
            var profiles = GetActiveProfiles();
            if (profiles is null || profiles.Length == 0) return null;

            IRankedArchetypeProfile maxProfile = null;
            foreach (var p in profiles)
            {
                if (maxProfile is null || p.MaxMmr > maxProfile.MaxMmr)
                    maxProfile = p;
            }
            return maxProfile;
        }
    }
}