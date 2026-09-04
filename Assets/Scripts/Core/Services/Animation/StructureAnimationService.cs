using System;
using DG.Tweening;
using Game.Data;
using Game.Views;
using UnityEngine;

namespace Game.Services.Animation
{
    public class StructureAnimationService : IStructureAnimationService
    {
        private readonly StructureAnimationConfig _config;

        public StructureAnimationService(StructureAnimationConfig config)
        {
            _config = config;
        }

        public void AnimateSpawn(StructureView structure, Action onComplete)
        {
            if (structure is null)
            {
                onComplete?.Invoke();
                return;
            }

            structure.transform.localScale = Vector3.zero;
            structure.transform.DOScale(Vector3.one, _config.SpawnDuration)
                .SetEase(_config.SpawnEase)
                .SetAutoKill(true)
                .OnComplete(() => onComplete?.Invoke());
        }

        public void AnimateDespawn(StructureView structure, Action onComplete)
        {
            if (structure is null)
            {
                onComplete?.Invoke();
                return;
            }

            structure.transform.DOScale(Vector3.zero, _config.DespawnDuration)
                .SetEase(_config.DespawnEase)
                .SetAutoKill(true)
                .OnComplete(() => onComplete?.Invoke());
        }
    }
}