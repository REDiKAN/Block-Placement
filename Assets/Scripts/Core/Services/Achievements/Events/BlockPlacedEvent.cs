namespace Game.Services.Achievements
{
    public readonly struct BlockPlacedEvent : IAchievementEvent
    {
        public static readonly BlockPlacedEvent Default = new();
    }
}