using System;
using System.Linq;
using UniRx;
using UnityEngine;
using Zenject;
using Game.Services.Input;

namespace Game.Services.Animation
{
    public interface ILevelIntroAnimationService
    {
        IReadOnlyReactiveProperty<bool> IsAnimating { get; }
        IObservable<Unit> OnAnimationCompleted { get; }
        void Play();
    }

    public class LevelIntroAnimationService : ILevelIntroAnimationService, IInitializable, IDisposable
    {
        public IReadOnlyReactiveProperty<bool> IsAnimating => _isAnimating;
        public IObservable<Unit> OnAnimationCompleted => _onAnimationCompleted;

        private readonly ReactiveProperty<bool> _isAnimating = new(false);
        private readonly Subject<Unit> _onAnimationCompleted = new();
        private readonly CompositeDisposable _disposables = new();
        private readonly ILevelIntroAnimationStrategy[] _strategies;
        private readonly IInputContextService _inputContextService;
        private int _lastIndex = -1;
        private int[] _shuffleArray;

        public LevelIntroAnimationService(
            ILevelIntroAnimationStrategy[] strategies,
            IInputContextService inputContextService)
        {
            _strategies = strategies;
            _inputContextService = inputContextService;
            InitializeShuffleArray();
        }

        public void Initialize()
        {
        }

        public void Play()
        {
            if (_isAnimating.Value || _strategies.Length == 0) return;

            var strategy = SelectStrategy();
            _inputContextService.SetContext(InputContext.Generating);
            _isAnimating.Value = true;

            strategy.Execute(HandleAnimationCompleted)
                .Subscribe(_ => { })
                .AddTo(_disposables);
        }

        private void HandleAnimationCompleted()
        {
            _isAnimating.Value = false;
            _inputContextService.SetContext(InputContext.PlaceBlock);
            _onAnimationCompleted.OnNext(Unit.Default);
        }

        private ILevelIntroAnimationStrategy SelectStrategy()
        {
            if (_strategies.Length == 1) return _strategies[0];

            if (IsAllStrategiesUsed())
            {
                ResetShuffleArray();
            }

            var availableIndices = _shuffleArray.Where(i => i != -1).ToArray();
            var randomIndex = UnityEngine.Random.Range(0, availableIndices.Length);
            var selectedIndex = availableIndices[randomIndex];

            MarkStrategyAsUsed(selectedIndex);
            _lastIndex = selectedIndex;

            return _strategies[selectedIndex];
        }

        private void InitializeShuffleArray()
        {
            _shuffleArray = new int[_strategies.Length];
            for (var i = 0; i < _strategies.Length; i++)
            {
                _shuffleArray[i] = i;
            }
        }

        private void ResetShuffleArray()
        {
            for (var i = 0; i < _shuffleArray.Length; i++)
            {
                _shuffleArray[i] = i;
            }
        }

        private bool IsAllStrategiesUsed()
        {
            return _shuffleArray.All(i => i == -1);
        }

        private void MarkStrategyAsUsed(int index)
        {
            for (var i = 0; i < _shuffleArray.Length; i++)
            {
                if (_shuffleArray[i] == index)
                {
                    _shuffleArray[i] = -1;
                    break;
                }
            }
        }

        public void Dispose() => _disposables?.Dispose();
    }
}