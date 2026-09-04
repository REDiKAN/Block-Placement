using System;
using Game.Views;

namespace Game.Services.Animation
{
    public interface IStructureAnimationService
    {
        void AnimateSpawn(StructureView structure, Action onComplete);
        void AnimateDespawn(StructureView structure, Action onComplete);
    }
}