using System;
using UniRx;

namespace Game.Services.Animation
{
    public interface ILevelIntroAnimationStrategy
    {
        string Id { get; }
        float Duration { get; }
        IObservable<Unit> Execute(Action onComplete);
    }
}