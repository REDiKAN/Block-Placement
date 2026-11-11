using Game.Data.Ranked;

namespace Game.Services.Ranked
{
    public interface IArchetypeResolver
    {
        IRankedArchetypeProfile Resolve(int mmr);
        int GetBaseDifficulty(Archetype archetype);
        bool IsRotationRequired(Archetype archetype);
        bool IsBlockLimitEnabled(Archetype archetype);
        int GetBlockLimitPadding(Archetype archetype);
        MmrBoundsInfo GetMmrBounds(int mmr);
    }
}