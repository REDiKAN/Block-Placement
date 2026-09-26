namespace Game.Services.Achievements
{
    public readonly struct DecorSunkEvent : IAchievementEvent
    {
        public static readonly DecorSunkEvent Default = new();
    }
}