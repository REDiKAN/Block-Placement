using Game.Data.Ranked;

namespace Game.Services.Ranked
{
    public interface IArchetypeResolver
    {
        Archetype Resolve(int mmr);
        int GetBaseDifficulty(Archetype archetype);
        bool IsRotationRequired(Archetype archetype);
        bool IsBlockLimitEnabled(Archetype archetype);
        int GetBlockLimitPadding(Archetype archetype);
        ArchetypeProfileData GetProfileData(Archetype archetype);
    }
}