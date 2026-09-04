using System;
using UniRx;

namespace Game.Services.Achievements
{
    public class AchievementEventBus : IAchievementEventBus
    {
        private readonly Subject<IAchievementEvent> _eventStream = new();

        public void Publish<T>(T achievementEvent) where T : IAchievementEvent =>
            _eventStream.OnNext(achievementEvent);

        public IObservable<T> Subscribe<T>() where T : IAchievementEvent =>
            _eventStream.Where(e => e is T).Cast<IAchievementEvent, T>();
    }
}