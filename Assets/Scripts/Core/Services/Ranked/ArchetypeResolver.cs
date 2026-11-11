using System;
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

        public MmrBoundsInfo GetMmrBounds(int mmr)
        {
            var profiles = GetSortedProfiles();
            if (profiles is null || profiles.Length == 0)
                return default;

            var currentIndex = -1;
            for (var i = 0; i < profiles.Length; i++)
            {
                if (mmr >= profiles[i].MinMmr && mmr <= profiles[i].MaxMmr)
                {
                    currentIndex = i;
                    break;
                }
            }

            if (currentIndex == -1)
            {
                if (mmr > profiles[^1].MaxMmr)
                    currentIndex = profiles.Length - 1;
                else
                    currentIndex = 0;
            }

            var current = profiles[currentIndex];

            var prevArchetype = currentIndex > 0 ? (Archetype?)profiles[currentIndex - 1].Archetype : null;
            var prevMax = currentIndex > 0 ? profiles[currentIndex - 1].MaxMmr : current.MinMmr;

            var nextArchetype = currentIndex < profiles.Length - 1 ? (Archetype?)profiles[currentIndex + 1].Archetype : null;
            var nextMin = currentIndex < profiles.Length - 1 ? profiles[currentIndex + 1].MinMmr : current.MaxMmr;

            return new MmrBoundsInfo(
                current.Archetype, current.MinMmr, current.MaxMmr,
                prevArchetype, prevMax,
                nextArchetype, nextMin,
                mmr);
        }

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

        private IRankedArchetypeProfile[] GetSortedProfiles()
        {
            var profiles = GetActiveProfiles();
            if (profiles is null || profiles.Length == 0) return null;

            var sorted = new IRankedArchetypeProfile[profiles.Length];
            for (var i = 0; i < profiles.Length; i++)
                sorted[i] = profiles[i];

            Array.Sort(sorted, (a, b) => a.MinMmr.CompareTo(b.MinMmr));
            return sorted;
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