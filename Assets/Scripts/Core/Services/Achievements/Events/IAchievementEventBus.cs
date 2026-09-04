using System;

namespace Game.Services.Achievements
{
    public interface IAchievementEventBus
    {
        void Publish<T>(T achievementEvent) where T : IAchievementEvent;
        IObservable<T> Subscribe<T>() where T : IAchievementEvent;
    }
}