using System;
using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Game.Data;
using Game.Services.Menu;
using Game.Services.Settings;
using Game.Services.Water;

namespace Game.Views.Menu
{
    public class SettingsView : MonoBehaviour
    {
        [field: SerializeField] private Button QualityButton { get; set; }
        [field: SerializeField] private TextMeshProUGUI QualityText { get; set; }
        [field: SerializeField] private Button ResolutionButton { get; set; }
        [field: SerializeField] private TextMeshProUGUI ResolutionText { get; set; }
        [field: SerializeField] private Button FullscreenButton { get; set; }
        [field: SerializeField] private TextMeshProUGUI FullscreenText { get; set; }
        [field: SerializeField] private Button WaterButton { get; set; }
        [field: SerializeField] private TextMeshProUGUI WaterText { get; set; }
        [field: SerializeField] private Button PreviewButton { get; set; }
        [field: SerializeField] private TextMeshProUGUI PreviewText { get; set; }
        [field: SerializeField] private Scrollbar FpsScrollbar { get; set; }
        [field: SerializeField] private TextMeshProUGUI FpsText { get; set; }
        [field: SerializeField] private Scrollbar MusicScrollbar { get; set; }
        [field: SerializeField] private TextMeshProUGUI MusicText { get; set; }
        [field: SerializeField] private Scrollbar SfxScrollbar { get; set; }
        [field: SerializeField] private TextMeshProUGUI SfxText { get; set; }
        [field: SerializeField] private Scrollbar DialogueScrollbar { get; set; }
        [field: SerializeField] private TextMeshProUGUI DialogueText { get; set; }
        [field: SerializeField] private Button BackButton { get; set; }

        [Inject] private ISettingsService _settingsService;
        [Inject] private IMenuNavigationService _navigationService;
        [Inject] private IWaterShaderService _waterShaderService;
        [Inject] private FpsLimitConfig _fpsLimitConfig;

        private readonly CompositeDisposable _disposables = new();
        private static readonly string[] QualityNames = { "Low", "Medium", "High" };
        private Tween _fpsTextTween;
        private int _lastFpsValue = -2;

        private bool _isSyncingUI;
        private bool _isUiInitialized;

        private void Start()
        {
            _isUiInitialized = false;

            _navigationService.CurrentView
                .Subscribe(view => gameObject.SetActive(view == MenuView.Settings))
                .AddTo(_disposables);

            if (QualityButton is not null)
                QualityButton.OnClickAsObservable().Subscribe(_ => _settingsService.CycleQuality()).AddTo(_disposables);
            if (ResolutionButton is not null)
                ResolutionButton.OnClickAsObservable().Subscribe(_ => _settingsService.CycleResolution()).AddTo(_disposables);
            if (FullscreenButton is not null)
                FullscreenButton.OnClickAsObservable().Subscribe(_ => _settingsService.CycleFullscreen()).AddTo(_disposables);
            if (WaterButton is not null)
                WaterButton.OnClickAsObservable().Subscribe(_ => _waterShaderService.CycleConfig()).AddTo(_disposables);
            if (PreviewButton is not null)
                PreviewButton.OnClickAsObservable().Subscribe(_ => _settingsService.CyclePreview()).AddTo(_disposables);
            if (BackButton is not null)
                BackButton.OnClickAsObservable().Subscribe(_ => _navigationService.NavigateTo(MenuView.MainMenu)).AddTo(_disposables);

            _settingsService.CurrentQualityLevel
                .Subscribe(level => { if (QualityText is not null) QualityText.text = $"Quality: {QualityNames[level]}"; })
                .AddTo(_disposables);
            _settingsService.CurrentResolution
                .Subscribe(resolution => { if (ResolutionText is not null && resolution is not null) ResolutionText.text = $"Resolution: {resolution}"; })
                .AddTo(_disposables);
            _settingsService.IsFullscreen
                .Subscribe(isFullscreen => { if (FullscreenText is not null) FullscreenText.text = $"Fullscreen: {(isFullscreen ? "On" : "Off")}"; })
                .AddTo(_disposables);
            _waterShaderService.CurrentConfig
                .Subscribe(config => { if (WaterText is not null && config is not null) WaterText.text = $"Water: {config.DisplayName}"; })
                .AddTo(_disposables);
            _settingsService.IsPreviewEnabled
                .Subscribe(isEnabled => { if (PreviewText is not null) PreviewText.text = $"Preview: {(isEnabled ? "On" : "Off")}"; })
                .AddTo(_disposables);

            if (FpsScrollbar is not null && _fpsLimitConfig?.Presets is not null && _fpsLimitConfig.Presets.Length > 0)
            {
                FpsScrollbar.OnValueChangedAsObservable()
                    .Subscribe(val =>
                    {
                        if (_isSyncingUI || !_isUiInitialized) return;
                        var index = Mathf.RoundToInt(val * (_fpsLimitConfig.Presets.Length - 1));
                        _settingsService.SetFpsLimitByIndex(index);
                    })
                    .AddTo(_disposables);
            }

            if (MusicScrollbar is not null)
            {
                MusicScrollbar.OnValueChangedAsObservable()
                    .Subscribe(val =>
                    {
                        if (_isSyncingUI || !_isUiInitialized) return;
                        _settingsService.SetMusicVolume(val);
                    })
                    .AddTo(_disposables);
            }

            if (SfxScrollbar is not null)
            {
                SfxScrollbar.OnValueChangedAsObservable()
                    .Subscribe(val =>
                    {
                        if (_isSyncingUI || !_isUiInitialized) return;
                        _settingsService.SetSfxVolume(val);
                    })
                    .AddTo(_disposables);
            }

            if (DialogueScrollbar is not null)
            {
                DialogueScrollbar.OnValueChangedAsObservable()
                    .Subscribe(val =>
                    {
                        if (_isSyncingUI || !_isUiInitialized) return;
                        _settingsService.SetDialogueVolume(val);
                    })
                    .AddTo(_disposables);
            }

            _settingsService.CurrentFpsLimit
                .Subscribe(fps =>
                {
                    if (FpsText is not null) AnimateFpsText(fps);
                    if (FpsScrollbar is not null && _fpsLimitConfig?.Presets is not null && _fpsLimitConfig.Presets.Length > 0)
                    {
                        var index = Array.IndexOf(_fpsLimitConfig.Presets, fps);
                        if (index >= 0)
                        {
                            _isSyncingUI = true;
                            FpsScrollbar.value = (float)index / (_fpsLimitConfig.Presets.Length - 1);
                            _isSyncingUI = false;
                        }
                    }
                })
                .AddTo(_disposables);

            _settingsService.CurrentMusicVolume
                .Subscribe(volume =>
                {
                    if (MusicText is not null) MusicText.text = $"Music: {Mathf.RoundToInt(volume * 100)}%";
                    if (MusicScrollbar is not null)
                    {
                        _isSyncingUI = true;
                        MusicScrollbar.value = volume;
                        _isSyncingUI = false;
                    }
                })
                .AddTo(_disposables);

            _settingsService.CurrentSfxVolume
                .Subscribe(volume =>
                {
                    if (SfxText is not null) SfxText.text = $"SFX: {Mathf.RoundToInt(volume * 100)}%";
                    if (SfxScrollbar is not null)
                    {
                        _isSyncingUI = true;
                        SfxScrollbar.value = volume;
                        _isSyncingUI = false;
                    }
                })
                .AddTo(_disposables);

            _settingsService.CurrentDialogueVolume
                .Subscribe(volume =>
                {
                    if (DialogueText is not null) DialogueText.text = $"Dialogue: {Mathf.RoundToInt(volume * 100)}%";
                    if (DialogueScrollbar is not null)
                    {
                        _isSyncingUI = true;
                        DialogueScrollbar.value = volume;
                        _isSyncingUI = false;
                    }
                })
                .AddTo(_disposables);

            _isUiInitialized = true;
        }

        private void AnimateFpsText(int targetFps)
        {
            if (_lastFpsValue == targetFps) return;
            _fpsTextTween?.Kill();
            var startValue = _lastFpsValue < 0 ? targetFps : _lastFpsValue;
            var currentDisplayValue = (float)startValue;
            if (targetFps == -1)
            {
                _fpsTextTween = DOTween.To(
                    () => currentDisplayValue,
                    x =>
                    {
                        currentDisplayValue = x;
                        if (FpsText is not null)
                        {
                            var displayValue = Mathf.RoundToInt(currentDisplayValue);
                            FpsText.text = displayValue == -1 ? "FPS: No limit" : $"FPS: {displayValue}";
                        }
                    },
                    targetFps,
                    0.3f
                ).SetEase(Ease.OutCubic).SetAutoKill(true);
            }
            else if (startValue == -1)
            {
                if (FpsText is not null) FpsText.text = $"FPS: {targetFps}";
            }
            else
            {
                _fpsTextTween = DOTween.To(
                    () => currentDisplayValue,
                    x =>
                    {
                        currentDisplayValue = x;
                        if (FpsText is not null)
                            FpsText.text = $"FPS: {Mathf.RoundToInt(currentDisplayValue)}";
                    },
                    targetFps,
                    0.3f
                ).SetEase(Ease.OutCubic).SetAutoKill(true);
            }
            if (FpsText is not null)
            {
                FpsText.transform.DOScale(1.15f, 0.15f)
                    .SetEase(Ease.OutBack)
                    .OnComplete(() => FpsText.transform.DOScale(1f, 0.1f))
                    .SetAutoKill(true);
            }
            _lastFpsValue = targetFps;
        }

        private void OnDestroy()
        {
            _fpsTextTween?.Kill();
            _disposables?.Dispose();
        }
    }
}