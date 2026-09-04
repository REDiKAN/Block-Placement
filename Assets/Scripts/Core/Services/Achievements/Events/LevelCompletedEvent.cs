namespace Game.Services.Achievements
{
    public readonly struct LevelCompletedEvent : IAchievementEvent
    {
        public int CategoryId { get; }
        public int LevelId { get; }

        public LevelCompletedEvent(int categoryId, int levelId)
        {
            CategoryId = categoryId;
            LevelId = levelId;
        }
    }
}