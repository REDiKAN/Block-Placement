using System;
using DG.Tweening;
using UniRx;
using UnityEngine;
using Zenject;
using Game.Data;
using Game.Views;

namespace Game.Services.Animation
{
    public class BlockAnimationService : IBlockAnimationService, IInitializable, IDisposable
    {
        public IObservable<Unit> OnAnimationCompleted => _onAnimationCompleted;

        private readonly Subject<Unit> _onAnimationCompleted = new();
        private readonly CompositeDisposable _disposables = new();
        private readonly BlockAnimationConfig _config;

        public BlockAnimationService(BlockAnimationConfig config)
        {
            _config = config;
        }

        public void Initialize()
        {
        }

        public void AnimateSpawn(BlockView block, Action onComplete)
        {
            if (block is null)
            {
                onComplete?.Invoke();
                return;
            }

            block.transform.localScale = Vector3.zero;
            var targetScale = Vector3.one * _config.SpawnScale;
            var ease = ConvertEase(_config.SpawnEase);

            var sequence = DOTween.Sequence();
            sequence.Append(block.transform.DOScale(targetScale, _config.SpawnDuration * 0.7f).SetEase(ease));
            sequence.Append(block.transform.DOScale(Vector3.one, _config.SpawnDuration * 0.3f).SetEase(Ease.OutQuad));
            sequence.OnComplete(() =>
            {
                onComplete?.Invoke();
                _onAnimationCompleted.OnNext(Unit.Default);
            });
        }

        public void AnimateDespawn(BlockView block, Action onComplete)
        {
            if (block is null)
            {
                onComplete?.Invoke();
                return;
            }

            var ease = ConvertEase(_config.DespawnEase);

            block.transform.DOScale(Vector3.zero, _config.DespawnDuration)
                .SetEase(ease)
                .SetAutoKill(true)
                .OnComplete(() =>
                {
                    onComplete?.Invoke();
                    _onAnimationCompleted.OnNext(Unit.Default);
                });
        }

        private Ease ConvertEase(BlockAnimationEase animationEase) => animationEase switch
        {
            BlockAnimationEase.OutQuad => Ease.OutQuad,
            BlockAnimationEase.OutCubic => Ease.OutCubic,
            BlockAnimationEase.OutBack => Ease.OutBack,
            BlockAnimationEase.InQuad => Ease.InQuad,
            BlockAnimationEase.InCubic => Ease.InCubic,
            BlockAnimationEase.InBack => Ease.InBack,
            _ => Ease.OutCubic
        };

        public void Dispose() => _disposables?.Dispose();
    }
}