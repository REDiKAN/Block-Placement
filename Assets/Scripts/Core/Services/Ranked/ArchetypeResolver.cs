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

        public int GetBaseDifficulty(Archetype archetype)
        {
            var profile = FindProfile(archetype);
            return profile?.BaseDifficulty ?? 1;
        }

        public bool IsRotationRequired(Archetype archetype)
        {
            var profile = FindProfile(archetype);
            return profile?.IsRotationRequired ?? false;
        }

        public bool IsBlockLimitEnabled(Archetype archetype)
        {
            var profile = FindProfile(archetype);
            return profile?.IsBlockLimitEnabled ?? false;
        }

        public int GetBlockLimitPadding(Archetype archetype)
        {
            var profile = FindProfile(archetype);
            return profile?.BlockLimitPadding ?? 0;
        }

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