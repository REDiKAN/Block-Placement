using DG.Tweening;
using UnityEngine;
using Zenject;
using Game.Data;

namespace Game.Views.Effects
{
    public class RainEffectView : MonoBehaviour, IEffectView
    {
        [field: SerializeField, Range(0f, 1f)] public float Probability { get; private set; }
        [field: SerializeField] public ParticleSystem ParticleSystem { get; private set; }

        private Tween _fadeTween;
        private RainConfig _rainConfig;

        [Inject]
        public void Construct(RainConfig rainConfig)
        {
            _rainConfig = rainConfig;
        }

        public void Show()
        {
            gameObject.SetActive(true);

            if (ParticleSystem is null || _rainConfig is null || _rainConfig.States is null || _rainConfig.States.Length == 0)
                return;

            var randomIndex = UnityEngine.Random.Range(0, _rainConfig.States.Length);
            var targetRate = _rainConfig.States[randomIndex].TargetRate;

            ParticleSystem.Play();
            _fadeTween?.Kill();

            _fadeTween = DOTween.To(
                () => ParticleSystem.emission.rateOverTime.constant,
                value =>
                {
                    var emission = ParticleSystem.emission;
                    emission.rateOverTime = value;
                },
                targetRate,
                _rainConfig.FadeDuration
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