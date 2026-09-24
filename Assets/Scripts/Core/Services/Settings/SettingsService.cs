using System;
using System.Collections.Generic;
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
        public IReadOnlyReactiveProperty<bool> IsPreviewEnabled => _isPreviewEnabled;
        public IReadOnlyReactiveProperty<int> CurrentFpsLimit => _currentFpsLimit;
        public IReadOnlyList<int> AllPresets => _allPresets;

        private readonly ReactiveProperty<int> _currentQualityLevel = new();
        private readonly ReactiveProperty<ResolutionData> _currentResolution = new();
        private readonly ReactiveProperty<bool> _isFullscreen = new();
        private readonly ReactiveProperty<bool> _isPreviewEnabled = new();
        private readonly ReactiveProperty<int> _currentFpsLimit = new();
        private readonly CompositeDisposable _disposables = new();
        private readonly SettingsConfig _config;
        private readonly FpsLimitConfig _fpsLimitConfig;
        private List<int> _allPresets = new();

        private const string QualityKey = "settings_quality";
        private const string ResolutionKey = "settings_resolution";
        private const string FullscreenKey = "settings_fullscreen";
        private const string PreviewEnabledKey = "settings_preview_enabled";
        private const string FpsLimitKey = "settings_fps_limit";
        private const string MonitorDetectedKey = "settings_fps_monitor_detected";
        private const string MonitorValueKey = "settings_fps_monitor_value";
        private const int QualityLevelsCount = 3;

        public SettingsService(SettingsConfig config, FpsLimitConfig fpsLimitConfig)
        {
            _config = config;
            _fpsLimitConfig = fpsLimitConfig;
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
            ApplyResolution(_config.Resolutions[resolutionIndex]);

            var savedFullscreen = PlayerPrefs.GetInt(FullscreenKey, 1) == 1;
            _isFullscreen.Value = savedFullscreen;
            ApplyFullscreen(savedFullscreen);

            var savedPreviewEnabled = PlayerPrefs.GetInt(PreviewEnabledKey, 0) == 1;
            _isPreviewEnabled.Value = savedPreviewEnabled;

            var isFirstLaunch = !PlayerPrefs.HasKey(MonitorDetectedKey);

            if (isFirstLaunch)
            {
                var ratio = Screen.currentResolution.refreshRateRatio;
                var roundedValue = Mathf.RoundToInt(ratio.numerator / (float)ratio.denominator);
                PlayerPrefs.SetInt(MonitorValueKey, roundedValue);
                PlayerPrefs.SetInt(MonitorDetectedKey, 1);
            }

            BuildAllPresets();

            var savedFpsIndex = PlayerPrefs.GetInt(FpsLimitKey, -1);

            if (isFirstLaunch)
            {
                var monitorValue = PlayerPrefs.GetInt(MonitorValueKey, -1);
                savedFpsIndex = _allPresets.IndexOf(monitorValue);
            }

            if (savedFpsIndex < 0 || savedFpsIndex >= _allPresets.Count)
            {
                savedFpsIndex = _fpsLimitConfig.DefaultIndex < _allPresets.Count
                    ? _fpsLimitConfig.DefaultIndex
                    : 0;
            }

            var fpsValue = _allPresets[savedFpsIndex];
            _currentFpsLimit.Value = fpsValue;
            ApplyFpsLimit(fpsValue);

            if (isFirstLaunch)
            {
                PlayerPrefs.SetInt(FpsLimitKey, savedFpsIndex);
                PlayerPrefs.Save();
            }
        }

        private void BuildAllPresets()
        {
            _allPresets = new List<int>();

            if (_fpsLimitConfig?.Presets is not null)
            {
                foreach (var preset in _fpsLimitConfig.Presets)
                {
                    if (!_allPresets.Contains(preset))
                        _allPresets.Add(preset);
                }
            }

            if (PlayerPrefs.HasKey(MonitorValueKey))
            {
                var monitorValue = PlayerPrefs.GetInt(MonitorValueKey, -1);
                if (monitorValue > 0 && !_allPresets.Contains(monitorValue))
                    _allPresets.Add(monitorValue);
            }

            _allPresets.Sort();
        }

        public string GetPresetLabel(int presetValue)
        {
            var isMonitorPreset = PlayerPrefs.HasKey(MonitorValueKey) &&
                                  PlayerPrefs.GetInt(MonitorValueKey, -1) == presetValue;

            var isStaticPreset = false;
            if (_fpsLimitConfig?.Presets is not null)
            {
                foreach (var preset in _fpsLimitConfig.Presets)
                {
                    if (preset == presetValue)
                    {
                        isStaticPreset = true;
                        break;
                    }
                }
            }

            return isMonitorPreset && !isStaticPreset
                ? $"{presetValue} (монитор)"
                : $"{presetValue}";
        }

        public void SetFpsLimitByIndex(int index)
        {
            if (_allPresets is null || index < 0 || index >= _allPresets.Count) return;

            var fpsValue = _allPresets[index];
            _currentFpsLimit.Value = fpsValue;
            PlayerPrefs.SetInt(FpsLimitKey, index);
            PlayerPrefs.Save();
            ApplyFpsLimit(fpsValue);
        }

        private static void ApplyFpsLimit(int fpsValue)
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = fpsValue;
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

        public void CyclePreview()
        {
            var next = !_isPreviewEnabled.Value;
            _isPreviewEnabled.Value = next;
            PlayerPrefs.SetInt(PreviewEnabledKey, next ? 1 : 0);
            PlayerPrefs.Save();
        }

        private static void ApplyQuality(int level) => QualitySettings.SetQualityLevel(level, true);

        private static void ApplyResolution(ResolutionData resolution) =>
            Screen.SetResolution(resolution.Width, resolution.Height, Screen.fullScreenMode);

        private static void ApplyFullscreen(bool isFullscreen) =>
            Screen.fullScreenMode = isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;

        public void Dispose() => _disposables?.Dispose();
    }
}