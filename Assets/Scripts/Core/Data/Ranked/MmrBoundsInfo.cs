namespace Game.Data.Ranked
{
    public readonly struct MmrBoundsInfo
    {
        public Archetype CurrentArchetype { get; }
        public int CurrentMin { get; }
        public int CurrentMax { get; }

        public Archetype? PrevArchetype { get; }
        public int PrevMax { get; }

        public Archetype? NextArchetype { get; }
        public int NextMin { get; }

        public int CurrentMmr { get; }

        public MmrBoundsInfo(
            Archetype currentArchetype, int currentMin, int currentMax,
            Archetype? prevArchetype, int prevMax,
            Archetype? nextArchetype, int nextMin,
            int currentMmr)
        {
            CurrentArchetype = currentArchetype;
            CurrentMin = currentMin;
            CurrentMax = currentMax;
            PrevArchetype = prevArchetype;
            PrevMax = prevMax;
            NextArchetype = nextArchetype;
            NextMin = nextMin;
            CurrentMmr = currentMmr;
        }
    }
}