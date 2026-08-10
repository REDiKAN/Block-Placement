using DG.Tweening;
using Game.Data;
using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace Game.Services.Water
{
    public class WaterShaderService : IWaterShaderService, IInitializable, IDisposable
    {
        public IReadOnlyReactiveProperty<WaterShaderConfig> CurrentConfig => _currentConfig;
        public IReadOnlyReactiveProperty<WaterShaderParameters> CurrentParameters => _currentParameters;

        private readonly ReactiveProperty<WaterShaderConfig> _currentConfig = new();
        private readonly ReactiveProperty<WaterShaderParameters> _currentParameters = new();
        private readonly CompositeDisposable _disposables = new();
        private readonly WaterShaderConfigCatalog _catalog;
        private WaterShaderParameters _fromParameters;
        private WaterShaderParameters _toParameters;
        private Tween _transitionTween;
        private float _transitionProgress;
        private int _currentIndex;

        private const string WaterPresetKey = "settings_water_preset";

        public WaterShaderService([InjectOptional] WaterShaderConfigCatalog catalog)
        {
            _catalog = catalog;
        }

        public void Initialize()
        {
            if (_catalog is null || _catalog.Configs is null || _catalog.Configs.Length == 0)
            {
                Debug.LogError("[WaterShaderService] Catalog is null or empty.");
                return;
            }

            var savedIndex = PlayerPrefs.GetInt(WaterPresetKey, 0);
            _currentIndex = savedIndex >= 0 && savedIndex < _catalog.Configs.Length ? savedIndex : 0;
            ApplyInstantly(_catalog.Configs[_currentIndex]);
        }

        public void CycleConfig()
        {
            if (_catalog is null || _catalog.Configs is null || _catalog.Configs.Length == 0)
                return;

            _currentIndex = (_currentIndex + 1) % _catalog.Configs.Length;
            PlayerPrefs.SetInt(WaterPresetKey, _currentIndex);
            PlayerPrefs.Save();

            var target = _catalog.Configs[_currentIndex];
            _currentConfig.Value = target;

            _fromParameters = _currentParameters.Value;
            _toParameters = WaterShaderParameters.FromConfig(target);

            _transitionTween?.Kill();

            if (_catalog.TransitionDuration <= 0f)
            {
                _transitionProgress = 1f;
                _currentParameters.Value = _toParameters;
                return;
            }

            _transitionProgress = 0f;
            _transitionTween = DOTween.To(() => _transitionProgress, SetTransitionProgress, 1f, _catalog.TransitionDuration)
                .SetEase(Ease.Linear)
                .SetAutoKill(true);
        }

        private void SetTransitionProgress(float value)
        {
            _transitionProgress = value;
            _currentParameters.Value = WaterShaderParameters.Lerp(in _fromParameters, in _toParameters, value);
        }

        private void ApplyInstantly(WaterShaderConfig config)
        {
            _transitionTween?.Kill();
            _transitionProgress = 1f;
            _currentConfig.Value = config;
            _currentParameters.Value = WaterShaderParameters.FromConfig(config);
        }

        public void Dispose()
        {
            _transitionTween?.Kill();
            _disposables?.Dispose();
        }
    }
}