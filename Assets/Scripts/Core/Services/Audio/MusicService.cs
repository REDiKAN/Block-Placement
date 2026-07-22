using DG.Tweening;
using UnityEngine;
using Zenject;
using Game.Data;

namespace Game.Services.Audio
{
    public class MusicService : IMusicService, IInitializable, System.IDisposable
    {
        private readonly AudioConfig _config;
        private readonly AudioClip _startMusicClip;

        private AudioSource _currentSource;
        private AudioSource _nextSource;
        private GameObject _currentObject;
        private GameObject _nextObject;
        private Sequence _crossfadeSequence;

        public MusicService(AudioConfig config, [InjectOptional] AudioClip startMusicClip)
        {
            _config = config;
            _startMusicClip = startMusicClip;
        }

        public void Initialize()
        {
            InitializeAudioSources();

            if (_startMusicClip != null && _config is not null)
                Play(_startMusicClip);
        }

        private void InitializeAudioSources()
        {
            if (_config is null)
                return;

            _currentObject = new GameObject("MusicSource_Current");
            Object.DontDestroyOnLoad(_currentObject);
            _currentSource = _currentObject.AddComponent<AudioSource>();
            _currentSource.loop = true;
            _currentSource.spatialBlend = 0f;
            _currentSource.playOnAwake = false;
            _currentSource.outputAudioMixerGroup = _config.MusicMixerGroup;

            _nextObject = new GameObject("MusicSource_Next");
            Object.DontDestroyOnLoad(_nextObject);
            _nextSource = _nextObject.AddComponent<AudioSource>();
            _nextSource.loop = true;
            _nextSource.spatialBlend = 0f;
            _nextSource.playOnAwake = false;
            _nextSource.outputAudioMixerGroup = _config.MusicMixerGroup;
        }

        public void Play(AudioClip clip)
        {
            if (clip == null || _config is null || _currentSource is null || _nextSource is null)
                return;

            _crossfadeSequence?.Kill();

            if (_currentSource.isPlaying)
                PerformCrossfade(clip);
            else
                StartImmediately(clip);
        }

        public void Stop()
        {
            _crossfadeSequence?.Kill();
            _crossfadeSequence = null;

            if (_currentSource != null && _currentSource.isPlaying)
                _currentSource.Stop();

            if (_nextSource != null && _nextSource.isPlaying)
                _nextSource.Stop();
        }

        private void StartImmediately(AudioClip clip)
        {
            _currentSource.clip = clip;
            _currentSource.volume = _config.DefaultMusicVolume;
            _currentSource.Play();
        }

        private void PerformCrossfade(AudioClip newClip)
        {
            _nextSource.clip = newClip;
            _nextSource.volume = 0f;
            _nextSource.Play();

            var targetVolume = _config.DefaultMusicVolume;
            var duration = _config.CrossfadeDuration;

            _crossfadeSequence = DOTween.Sequence()
                .Join(_currentSource.DOFade(0f, duration))
                .Join(_nextSource.DOFade(targetVolume, duration))
                .OnComplete(() => SwapSources());
        }

        private void SwapSources()
        {
            var temp = _currentSource;
            _currentSource = _nextSource;
            _nextSource = temp;

            _nextSource.Stop();
            _nextSource.clip = null;
        }

        public void Dispose()
        {
            _crossfadeSequence?.Kill();
            _crossfadeSequence = null;

            if (_currentSource != null)
                _currentSource.Stop();

            if (_nextSource != null)
                _nextSource.Stop();

            if (_currentObject != null)
                Object.Destroy(_currentObject);

            if (_nextObject != null)
                Object.Destroy(_nextObject);
        }
    }
}