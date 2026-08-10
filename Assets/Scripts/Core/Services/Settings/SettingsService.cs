using System;
using UniRx;
using UnityEngine;
using Zenject;
using Game.Data;

namespace Game.Services.Settings
{
    public class SettingsService : ISettingsService, IInitializable, IDisposable
    {
        public IReadOnlyReactiveProperty<int> CurrentQualityLevel => _currentQualityLevel;
        public IReadOnlyReactiveProperty<ResolutionData> CurrentResolution => _currentResolution;
        public IReadOnlyReactiveProperty<bool> IsFullscreen => _isFullscreen;

        private readonly ReactiveProperty<int> _currentQualityLevel = new();
        private readonly ReactiveProperty<ResolutionData> _currentResolution = new();
        private readonly ReactiveProperty<bool> _isFullscreen = new();
        private readonly CompositeDisposable _disposables = new();
        private readonly SettingsConfig _config;

        private const string QualityKey = "settings_quality";
        private const string ResolutionKey = "settings_resolution";
        private const string FullscreenKey = "settings_fullscreen";
        private const int QualityLevelsCount = 3;

        public SettingsService(SettingsConfig config)
        {
            _config = config;
        }

        public void Initialize()
        {
            var savedQuality = PlayerPrefs.GetInt(QualityKey, QualityLevelsCount - 1);
            _currentQualityLevel.Value = savedQuality;
            ApplyQuality(savedQuality);

            var savedResolutionIndex = PlayerPrefs.GetInt(ResolutionKey, 0);
            var resolutionIndex = _config.Resolutions is not null && savedResolutionIndex < _config.Resolutions.Length
                ? savedResolutionIndex
                : 0;
            _currentResolution.Value = _config.Resolutions[resolutionIndex];
            ApplyResolution(_currentResolution.Value);

            var savedFullscreen = PlayerPrefs.GetInt(FullscreenKey, 1) == 1;
            _isFullscreen.Value = savedFullscreen;
            ApplyFullscreen(savedFullscreen);
        }

        public void CycleQuality()
        {
            var next = _currentQualityLevel.Value - 1;
            if (next < 0) next = QualityLevelsCount - 1;
            _currentQualityLevel.Value = next;
            PlayerPrefs.SetInt(QualityKey, next);
            PlayerPrefs.Save();
            ApplyQuality(next);
        }

        public void CycleResolution()
        {
            if (_config.Resolutions is null || _config.Resolutions.Length == 0) return;
            var currentIndex = Array.IndexOf(_config.Resolutions, _currentResolution.Value);
            var next = (currentIndex + 1) % _config.Resolutions.Length;
            _currentResolution.Value = _config.Resolutions[next];
            PlayerPrefs.SetInt(ResolutionKey, next);
            PlayerPrefs.Save();
            ApplyResolution(_config.Resolutions[next]);
        }

        public void CycleFullscreen()
        {
            var next = !_isFullscreen.Value;
            _isFullscreen.Value = next;
            PlayerPrefs.SetInt(FullscreenKey, next ? 1 : 0);
            PlayerPrefs.Save();
            ApplyFullscreen(next);
        }

        private static void ApplyQuality(int level) =>
            QualitySettings.SetQualityLevel(level, true);

        private static void ApplyResolution(ResolutionData resolution) =>
            Screen.SetResolution(resolution.Width, resolution.Height, Screen.fullScreenMode);

        private static void ApplyFullscreen(bool isFullscreen) =>
            Screen.fullScreenMode = isFullscreen
                ? FullScreenMode.FullScreenWindow
                : FullScreenMode.Windowed;

        public void Dispose() => _disposables?.Dispose();
    }
}