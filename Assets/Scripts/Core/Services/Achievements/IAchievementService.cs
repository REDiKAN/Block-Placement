using System;
using UniRx;

namespace Game.Services.Achievements
{
    public interface IAchievementService
    {
        ReactiveCollection<AchievementRuntimeData> Achievements { get; }
        IObservable<AchievementRuntimeData> OnAchievementUnlocked { get; }
    }
}