using Game.Data.Ranked;

namespace Game.Services.Ranked
{
    public class ArchetypeResolver : IArchetypeResolver
    {
        private readonly RankedConfig _config;

        public ArchetypeResolver(RankedConfig config)
        {
            _config = config;
        }

        public Archetype Resolve(int mmr)
        {
            if (_config?.ArchetypeProfiles is null || _config.ArchetypeProfiles.Length == 0)
                return Archetype.I;

            foreach (var profile in _config.ArchetypeProfiles)
            {
                if (mmr >= profile.MinMmr && mmr <= profile.MaxMmr)
                    return profile.Archetype;
            }

            return Archetype.Max;
        }

        public int GetBaseDifficulty(Archetype archetype) =>
            FindProfile(archetype)?.BaseDifficulty ?? 1;

        public bool IsRotationRequired(Archetype archetype) =>
            FindProfile(archetype)?.IsRotationRequired ?? false;

        public bool IsBlockLimitEnabled(Archetype archetype) =>
            FindProfile(archetype)?.IsBlockLimitEnabled ?? false;

        public int GetBlockLimitPadding(Archetype archetype) =>
            FindProfile(archetype)?.BlockLimitPadding ?? 0;

        public ArchetypeProfileData GetProfileData(Archetype archetype) =>
            FindProfile(archetype);

        private ArchetypeProfileData FindProfile(Archetype archetype)
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
    }
}