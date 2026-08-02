using System;
using UniRx;
using Game.Views;

namespace Game.Services.Animation
{
    public interface IBlockAnimationService
    {
        IObservable<Unit> OnAnimationCompleted { get; }
        void AnimateSpawn(BlockView block, Action onComplete);
        void AnimateDespawn(BlockView block, Action onComplete);
    }
}