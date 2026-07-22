using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Game.Data;
using Game.Views.Audio;

namespace Game.Services.Audio
{
    public class SfxService : ISfxService, IInitializable, System.IDisposable
    {
        private readonly Queue<PooledAudioSource> _availablePool = new();
        private readonly LinkedList<PooledAudioSource> _activeSources = new();
        private readonly List<GameObject> _poolObjects = new();
        private readonly AudioConfig _config;

        public SfxService(AudioConfig config)
        {
            _config = config;
        }

        public void Initialize()
        {
            InitializePool();
        }

        private void InitializePool()
        {
            if (_config is null)
                return;

            for (var i = 0; i < _config.SfxPoolSize; i++)
            {
                var gameObject = new GameObject($"PooledAudioSource_{i}");
                Object.DontDestroyOnLoad(gameObject);
                _poolObjects.Add(gameObject);

                var audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.spatialBlend = 0f;
                audioSource.playOnAwake = false;

                var pooledSource = gameObject.AddComponent<PooledAudioSource>();
                pooledSource.Setup(audioSource);

                _availablePool.Enqueue(pooledSource);
            }
        }

        public void Play(AudioClip clip) => Play(clip, _config.DefaultSfxVolume);

        public void Play(AudioClip clip, float volume)
        {
            if (clip == null || _config is null)
                return;

            var source = GetAvailableSource();
            if (source is null)
                return;

            _activeSources.AddLast(source);
            source.Play(clip, volume, _config.SfxMixerGroup, () => HandleSourceCompleted(source));
        }

        private PooledAudioSource GetAvailableSource()
        {
            if (_availablePool.Count > 0)
                return _availablePool.Dequeue();

            if (_activeSources.Count > 0)
            {
                var oldest = _activeSources.First.Value;
                oldest.Stop();
                _activeSources.RemoveFirst();
                return oldest;
            }

            return null;
        }

        private void HandleSourceCompleted(PooledAudioSource source)
        {
            _activeSources.Remove(source);
            _availablePool.Enqueue(source);
        }

        public void Dispose()
        {
            foreach (var source in _activeSources)
                source.Stop();

            _activeSources.Clear();
            _availablePool.Clear();

            foreach (var gameObject in _poolObjects)
            {
                if (gameObject != null)
                    Object.Destroy(gameObject);
            }

            _poolObjects.Clear();
        }
    }
}