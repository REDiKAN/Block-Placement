using System;
using UniRx;
using Zenject;
using Game.Data;
using Game.Services.Input;
using UnityEngine;

namespace Game.Services.Audio
{
    public class AudioSpectrumService : IAudioSpectrumService, ITickable, IDisposable
    {
        public int BarCount => _config.BarCount;
        public IObservable<float[]> OnSpectrumUpdated => _onSpectrumUpdated;

        private readonly Subject<float[]> _onSpectrumUpdated = new();
        private readonly CompositeDisposable _disposables = new();

        private readonly IMusicService _musicService;
        private readonly IInputContextService _inputContextService;
        private readonly AudioSpectrumConfig _config;

        private const int SampleCount = 512;

        private readonly float[] _rawSamples;
        private readonly float[] _processedBars;
        private readonly float[] _currentScales;
        private readonly float[] _peakScales;
        private readonly float[] _lastUpdates;

        public AudioSpectrumService(
            IMusicService musicService,
            IInputContextService inputContextService,
            AudioSpectrumConfig config)
        {
            _musicService = musicService;
            _inputContextService = inputContextService;
            _config = config;

            _rawSamples = new float[SampleCount];
            _processedBars = new float[_config.BarCount];
            _currentScales = new float[_config.BarCount];
            _peakScales = new float[_config.BarCount];
            _lastUpdates = new float[_config.BarCount];
        }

        public void Tick()
        {
            bool isPaused = _inputContextService.CurrentContext.Value == InputContext.Paused;
            float time = UnityEngine.Time.unscaledTime;

            if (!isPaused)
            {
                bool needsUpdate = false;
                for (int i = 0; i < _config.BarCount; i++)
                {
                    if (_currentScales[i] > 0.01f)
                    {
                        needsUpdate = true;
                        break;
                    }
                }

                if (needsUpdate)
                {
                    for (int i = 0; i < _config.BarCount; i++)
                    {
                        float t = (time - _lastUpdates[i]) / _config.ReleaseTime;
                        _currentScales[i] = Mathf.Lerp(_peakScales[i], 0f, t);
                        _processedBars[i] = Mathf.Max(0f, _currentScales[i] * _config.ScaleMultiplier);
                    }
                    _onSpectrumUpdated.OnNext(_processedBars);
                }
                return;
            }

            _musicService.GetSpectrumData(_rawSamples, 0, _config.WindowType);
            ProcessSpectrum(time);
            _onSpectrumUpdated.OnNext(_processedBars);
        }

        private void ProcessSpectrum(float time)
        {
            int minIndex = 2;
            int maxIndex = (SampleCount / 2) - 1;
            float minLog = Mathf.Log10(minIndex);
            float maxLog = Mathf.Log10(maxIndex);

            for (int i = 0; i < _config.BarCount; i++)
            {
                int startIndex = (int)Mathf.Pow(10f, Mathf.Lerp(minLog, maxLog, (float)i / _config.BarCount));
                int endIndex = (int)Mathf.Pow(10f, Mathf.Lerp(minLog, maxLog, (float)(i + 1) / _config.BarCount));

                if (startIndex < minIndex) startIndex = minIndex;
                if (endIndex > maxIndex) endIndex = maxIndex;
                if (endIndex <= startIndex) endIndex = startIndex + 1;

                float sum = 0f;
                int count = 0;
                for (int j = startIndex; j < endIndex; j++)
                {
                    sum += _rawSamples[j];
                    count++;
                }

                float average = count > 0 ? sum / count : 0f;
                float dbValue = 20f * Mathf.Log10(average / _config.RefValue);

                if (dbValue > _currentScales[i])
                {
                    _peakScales[i] = _currentScales[i] = dbValue;
                    _lastUpdates[i] = time;
                }

                float t = (time - _lastUpdates[i]) / _config.ReleaseTime;
                _currentScales[i] = Mathf.Lerp(_peakScales[i], 0f, t);

                _processedBars[i] = Mathf.Max(0f, _currentScales[i] * _config.ScaleMultiplier);
            }
        }

        public void Dispose()
        {
            _disposables.Dispose();
            _onSpectrumUpdated.Dispose();
        }
    }
}