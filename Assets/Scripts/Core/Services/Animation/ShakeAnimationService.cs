using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Zenject;

namespace Game.Services.Animation
{
    public interface IShakeAnimationService
    {
        void Shake(Transform target, float duration, float strength);
    }

    public class ShakeAnimationService : IShakeAnimationService
    {
        private readonly Dictionary<Transform, (Tween Tween, Vector3 OriginalPosition)> _activeTweens = new();

        public void Shake(Transform target, float duration, float strength)
        {
            if (target is null) return;

            if (_activeTweens.TryGetValue(target, out var existing))
            {
                existing.Tween.Kill();
                target.localPosition = existing.OriginalPosition;
                _activeTweens.Remove(target);
            }

            var originalPosition = target.localPosition;
            var tween = target.DOShakePosition(duration, strength, 10, 90, false, false, ShakeRandomnessMode.Harmonic)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    target.localPosition = originalPosition;
                    _activeTweens.Remove(target);
                });

            _activeTweens[target] = (tween, originalPosition);
        }
    }
}