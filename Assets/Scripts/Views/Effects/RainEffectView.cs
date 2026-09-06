using DG.Tweening;
using UnityEngine;

namespace Game.Views.Effects
{
    public class RainEffectView : MonoBehaviour, IEffectView
    {
        [field: SerializeField, Range(0f, 1f)] public float Probability { get; private set; }
        [field: SerializeField] public ParticleSystem ParticleSystem { get; private set; }

        [Header("Effect Setting")]
        [field: SerializeField] const float TargetRate = 3000f;
        [field: SerializeField] const float FadeDuration = 5f;

        private Tween _fadeTween;

        public void Show()
        {
            gameObject.SetActive(true);
            if (ParticleSystem is null) return;

            ParticleSystem.Play();
            _fadeTween?.Kill();

            _fadeTween = DOTween.To(
                () => ParticleSystem.emission.rateOverTime.constant,
                value =>
                {
                    var emission = ParticleSystem.emission;
                    emission.rateOverTime = value;
                },
                TargetRate,
                FadeDuration
            ).SetEase(Ease.Linear).SetAutoKill(true);
        }

        public void Hide()
        {
            _fadeTween?.Kill();

            if (ParticleSystem is not null)
            {
                var emission = ParticleSystem.emission;
                emission.rateOverTime = 0f;
                ParticleSystem.Stop();
            }

            gameObject.SetActive(false);
        }
    }
}