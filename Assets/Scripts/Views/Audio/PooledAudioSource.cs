using System;
using UniRx;
using UnityEngine;
using UnityEngine.Audio;

namespace Game.Views.Audio
{
    public class PooledAudioSource : MonoBehaviour
    {
        private AudioSource _source;
        private IDisposable _timerDisposable;
        private Action _onCompleteCallback;

        public void Setup(AudioSource source)
        {
            _source = source;
        }

        private void Awake()
        {
            if (_source is null)
                _source = GetComponent<AudioSource>();
        }

        public void Play(AudioClip clip, float volume, AudioMixerGroup mixerGroup, Action onComplete)
        {
            StopInternal();

            if (clip == null || _source is null)
                return;

            _source.clip = clip;
            _source.volume = volume;
            _source.outputAudioMixerGroup = mixerGroup;

            _onCompleteCallback = onComplete;
            _source.Play();

            _timerDisposable = Observable.Timer(TimeSpan.FromSeconds(clip.length))
                .Subscribe(_ => HandleCompletion());
        }

        public void Stop()
        {
            StopInternal();
            ResetState();
        }

        private void HandleCompletion()
        {
            _onCompleteCallback?.Invoke();
            _onCompleteCallback = null;
            ResetState();
        }

        private void StopInternal()
        {
            _timerDisposable?.Dispose();
            _timerDisposable = null;

            if (_source != null && _source.isPlaying)
                _source.Stop();
        }

        private void ResetState()
        {
            if (_source != null)
            {
                _source.clip = null;
                _source.outputAudioMixerGroup = null;
            }
            _onCompleteCallback = null;
        }

        private void OnDestroy()
        {
            _timerDisposable?.Dispose();
            _timerDisposable = null;
            _onCompleteCallback = null;
        }
    }
}