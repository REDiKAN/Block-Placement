using System;
using UniRx;

namespace Game.Services.Animation
{
    public interface ILevelTransitionAnimationService
    {
        IObservable<Unit> PlayDisappearAnimation();
        IObservable<Unit> PlayAppearAnimation();
    }
}