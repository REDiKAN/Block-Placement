namespace Game.Services.Achievements
{
    public readonly struct LevelCompletedEvent : IAchievementEvent
    {
        public int CategoryId { get; }
        public int LevelId { get; }
        public bool HasRain { get; }
        public bool HasFog { get; }

        public LevelCompletedEvent(int categoryId, int levelId, bool hasRain, bool hasFog)
        {
            CategoryId = categoryId;
            LevelId = levelId;
            HasRain = hasRain;
            HasFog = hasFog;
        }
    }
}