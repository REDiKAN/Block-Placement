using System;
using UniRx;
using UnityEngine;
using Zenject;
using Game.Data;
using Game.Services.Audio;
using Game.Services.Dialogue;

namespace Game.Services.Settings
{
    public class SettingsService : ISettingsService, IInitializable, IDisposable
    {
        public IReadOnlyReactiveProperty<int> CurrentQualityLevel => _currentQualityLevel;
        public IReadOnlyReactiveProperty<ResolutionData> CurrentResolution => _currentResolution;
        public IReadOnlyReactiveProperty<bool> IsFullscreen => _isFullscreen;
        public IReadOnlyReactiveProperty<bool> IsPreviewEnabled => _isPreviewEnabled;
        public IReadOnlyReactiveProperty<int> CurrentFpsLimit => _currentFpsLimit;
        public IReadOnlyReactiveProperty<float> CurrentMusicVolume => _currentMusicVolume;
        public IReadOnlyReactiveProperty<float> CurrentSfxVolume => _currentSfxVolume;
        public IReadOnlyReactiveProperty<float> CurrentDialogueVolume => _currentDialogueVolume;

        private readonly ReactiveProperty<int> _currentQualityLevel = new();
        private readonly ReactiveProperty<ResolutionData> _currentResolution = new();
        private readonly ReactiveProperty<bool> _isFullscreen = new();
        private readonly ReactiveProperty<bool> _isPreviewEnabled = new();
        private readonly ReactiveProperty<int> _currentFpsLimit = new();
        private readonly ReactiveProperty<float> _currentMusicVolume = new();
        private readonly ReactiveProperty<float> _currentSfxVolume = new();
        private readonly ReactiveProperty<float> _currentDialogueVolume = new();
        private readonly CompositeDisposable _disposables = new();
        private readonly SettingsConfig _config;
        private readonly FpsLimitConfig _fpsLimitConfig;
        private readonly AudioConfig _audioConfig;
        private readonly IMusicService _musicService;
        private readonly ISfxService _sfxService;
        private readonly IDialogueAudioService _dialogueAudioService;
        private bool _isInitializing;

        private const string QualityKey = "settings_quality";
        private const string ResolutionKey = "settings_resolution";
        private const string FullscreenKey = "settings_fullscreen";
        private const string PreviewEnabledKey = "settings_preview_enabled";
        private const string FpsLimitKey = "settings_fps_limit";
        private const string MusicVolumeKey = "settings_music_volume";
        private const string SfxVolumeKey = "settings_sfx_volume";
        private const string DialogueVolumeKey = "settings_dialogue_volume";
        private const int QualityLevelsCount = 3;

        public SettingsService(
            SettingsConfig config,
            FpsLimitConfig fpsLimitConfig,
            [InjectOptional] AudioConfig audioConfig,
            [InjectOptional] IMusicService musicService,
            [InjectOptional] ISfxService sfxService,
            [InjectOptional] IDialogueAudioService dialogueAudioService)
        {
            _isInitializing = true;
            _config = config;
            _fpsLimitConfig = fpsLimitConfig;
            _audioConfig = audioConfig;
            _musicService = musicService;
            _sfxService = sfxService;
            _dialogueAudioService = dialogueAudioService;
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

            var savedFpsIndex = PlayerPrefs.GetInt(FpsLimitKey, _fpsLimitConfig.DefaultIndex);
            if (_fpsLimitConfig.Presets is null || savedFpsIndex < 0 || savedFpsIndex >= _fpsLimitConfig.Presets.Length)
                savedFpsIndex = _fpsLimitConfig.DefaultIndex;
            var fpsValue = _fpsLimitConfig.Presets[savedFpsIndex];
            _currentFpsLimit.Value = fpsValue;
            ApplyFpsLimit(fpsValue);

            var defaultMusicVolume = _audioConfig is not null ? _audioConfig.DefaultMusicVolume : 1f;
            var defaultSfxVolume = _audioConfig is not null ? _audioConfig.DefaultSfxVolume : 1f;
            var defaultDialogueVolume = _audioConfig is not null ? _audioConfig.DefaultDialogueVolume : 1f;

            var savedMusicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, defaultMusicVolume);
            var savedSfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, defaultSfxVolume);
            var savedDialogueVolume = PlayerPrefs.GetFloat(DialogueVolumeKey, defaultDialogueVolume);

            _currentMusicVolume.Value = savedMusicVolume;
            _currentSfxVolume.Value = savedSfxVolume;
            _currentDialogueVolume.Value = savedDialogueVolume;

            _musicService?.SetVolume(savedMusicVolume);
            _sfxService?.SetVolume(savedSfxVolume);
            _dialogueAudioService?.SetVolume(savedDialogueVolume);

            _isInitializing = false;
        }

        public void SetMusicVolume(float volume)
        {
            if (_isInitializing) return;
            _currentMusicVolume.Value = volume;
            PlayerPrefs.SetFloat(MusicVolumeKey, volume);
            PlayerPrefs.Save();
            _musicService?.SetVolume(volume);
        }

        public void SetSfxVolume(float volume)
        {
            if (_isInitializing) return;
            _currentSfxVolume.Value = volume;
            PlayerPrefs.SetFloat(SfxVolumeKey, volume);
            PlayerPrefs.Save();
            _sfxService?.SetVolume(volume);
        }

        public void SetDialogueVolume(float volume)
        {
            if (_isInitializing) return;
            _currentDialogueVolume.Value = volume;
            PlayerPrefs.SetFloat(DialogueVolumeKey, volume);
            PlayerPrefs.Save();
            _dialogueAudioService?.SetVolume(volume);
        }

        public void SetFpsLimitByIndex(int index)
        {
            if (_isInitializing) return;
            if (_fpsLimitConfig.Presets is null || index < 0 || index >= _fpsLimitConfig.Presets.Length) return;
            var fpsValue = _fpsLimitConfig.Presets[index];
            _currentFpsLimit.Value = fpsValue;
            PlayerPrefs.SetInt(FpsLimitKey, index);
            PlayerPrefs.Save();
            ApplyFpsLimit(fpsValue);
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

        private static void ApplyFpsLimit(int fpsValue)
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = fpsValue;
        }

        private static void ApplyQuality(int level) => QualitySettings.SetQualityLevel(level, true);

        private static void ApplyResolution(ResolutionData resolution) =>
            Screen.SetResolution(resolution.Width, resolution.Height, Screen.fullScreenMode);

        private static void ApplyFullscreen(bool isFullscreen) =>
            Screen.fullScreenMode = isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;

        public void Dispose() => _disposables?.Dispose();
    }
}