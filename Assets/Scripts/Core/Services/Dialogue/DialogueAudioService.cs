using System;
using UniRx;
using UnityEngine;
using Zenject;
using Game.Data;

namespace Game.Services.Dialogue
{
    public class DialogueAudioService : IDialogueAudioService, IInitializable, IDisposable
    {
        private readonly IDialogueService _dialogueService;
        private readonly AudioConfig _audioConfig;
        private readonly CompositeDisposable _disposables = new();
        private AudioSource _audioSource;
        private GameObject _audioObject;
        private float _volume = 1f;

        private const string DialogueVolumeKey = "settings_dialogue_volume";

        public DialogueAudioService(
            IDialogueService dialogueService,
            [InjectOptional] AudioConfig audioConfig)
        {
            _dialogueService = dialogueService;
            _audioConfig = audioConfig;
        }

        public void Initialize()
        {
            _audioObject = new GameObject("DialogueAudioSource");
            UnityEngine.Object.DontDestroyOnLoad(_audioObject);
            _audioSource = _audioObject.AddComponent<AudioSource>();
            _audioSource.spatialBlend = 0f;
            _audioSource.playOnAwake = false;
            _audioSource.loop = false;

            if (_audioConfig is not null && _audioConfig.SfxMixerGroup is not null)
                _audioSource.outputAudioMixerGroup = _audioConfig.SfxMixerGroup;

            _volume = PlayerPrefs.GetFloat(DialogueVolumeKey, _audioConfig is not null ? _audioConfig.DefaultDialogueVolume : 1f);
            _audioSource.volume = _volume;

            _dialogueService.CurrentAudioClip
                .Subscribe(OnAudioClipChanged)
                .AddTo(_disposables);
        }

        public void SetVolume(float volume)
        {
            _volume = volume;
            if (_audioSource is not null)
                _audioSource.volume = volume;
        }

        private void OnAudioClipChanged(AudioClip clip)
        {
            if (_audioSource is null) return;

            _audioSource.Stop();
            _audioSource.clip = null;

            if (clip is null) return;

            _audioSource.clip = clip;
            _audioSource.volume = _volume;
            _audioSource.Play();
        }

        public void Dispose()
        {
            _disposables?.Dispose();
            if (_audioSource is not null) _audioSource.Stop();
            if (_audioObject is not null) UnityEngine.Object.Destroy(_audioObject);
        }
    }
}