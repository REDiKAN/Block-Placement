using System;
using UniRx;

namespace Game.Services.Time
{
    public interface ITimeLimitService
    {
        IReadOnlyReactiveProperty<float> RemainingTime { get; }
        IReadOnlyReactiveProperty<bool> IsRunning { get; }
        IObservable<Unit> OnTimeExpired { get; }
        void StartTimer(float seconds);
        void StopTimer();
        void ResetTimer();
    }
}