namespace Game.Services.Achievements
{
    public readonly struct StructurePlacedEvent : IAchievementEvent
    {
        public static readonly StructurePlacedEvent Default = new();
    }
}