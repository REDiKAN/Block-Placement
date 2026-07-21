using System;
using UniRx;
using UnityEngine;
using Zenject;
using Game.Data;
using Game.Services.Shadow;

namespace Game.Services.Time
{
    public class TimeLimitService : ITimeLimitService, IInitializable, IDisposable
    {
        public IReadOnlyReactiveProperty<float> RemainingTime => _remainingTime;
        public IReadOnlyReactiveProperty<bool> IsRunning => _isRunning;
        public IObservable<Unit> OnTimeExpired => _onTimeExpired;

        private readonly ReactiveProperty<float> _remainingTime = new();
        private readonly ReactiveProperty<bool> _isRunning = new(false);
        private readonly Subject<Unit> _onTimeExpired = new();
        private readonly CompositeDisposable _disposables = new();

        private readonly IShadowValidationService _validationService;
        private readonly LevelConfig _levelConfig;
        private readonly bool _isDeveloperMode;

        private float _initialTime;

        public TimeLimitService(
            IShadowValidationService validationService,
            LevelConfig levelConfig,
            [Inject(Id = "IsDeveloperMode")] bool isDeveloperMode)
        {
            _validationService = validationService;
            _levelConfig = levelConfig;
            _isDeveloperMode = isDeveloperMode;
        }

        public void Initialize()
        {
            _validationService.OnLevelCompleted
                .Subscribe(_ => StopTimer())
                .AddTo(_disposables);

            if (_isDeveloperMode) return;
            if (_levelConfig is null || !_levelConfig.IsTimeLimitEnabled || _levelConfig.TimeLimitSeconds <= 0f) return;

            StartTimer(_levelConfig.TimeLimitSeconds);
        }

        public void StartTimer(float seconds)
        {
            _initialTime = seconds;
            _remainingTime.Value = seconds;
            _isRunning.Value = true;

            Observable.EveryUpdate()
                .TakeWhile(_ => _isRunning.Value)
                .Subscribe(_ =>
                {
                    var next = _remainingTime.Value - UnityEngine.Time.deltaTime;

                    if (next <= 0f)
                    {
                        _remainingTime.Value = 0f;
                        _isRunning.Value = false;
                        _onTimeExpired.OnNext(Unit.Default);
                        return;
                    }

                    _remainingTime.Value = next;
                })
                .AddTo(_disposables);
        }

        public void StopTimer() => _isRunning.Value = false;

        public void ResetTimer()
        {
            StopTimer();

            if (_initialTime > 0f)
                StartTimer(_initialTime);
        }

        public void Dispose() => _disposables?.Dispose();
    }
}