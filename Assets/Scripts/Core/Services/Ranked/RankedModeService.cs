using System;
using UniRx;
using UnityEngine;
using Zenject;
using Game.Data.Ranked;
using Game.Services.Loading;
using Game.Services.Time;
using Game.Services.Shadow;
using Game.Services.Input;
using Game.Core;
using Game.Data;

namespace Game.Services.Ranked
{
    public class RankedModeService : IRankedModeService, IInitializable, IDisposable
    {
        public IReadOnlyReactiveProperty<int> CurrentMmr => _currentMmr;
        public IReadOnlyReactiveProperty<int> MatchPoints => _matchPoints;
        public IReadOnlyReactiveProperty<int> CurrentMatchLevel => _currentMatchLevel;
        public IReadOnlyReactiveProperty<bool> IsMatchActive => _isMatchActive;
        public IReadOnlyReactiveProperty<bool> IsLevel5Completed => _isLevel5Completed;

        public IObservable<Unit> OnMatchStarted => _onMatchStarted;
        public IObservable<RankedLevelResult> OnLevelCompleted => _onLevelCompleted;
        public IObservable<RankedMatchResult> OnMatchEnded => _onMatchEnded;

        private readonly ReactiveProperty<int> _currentMmr = new();
        private readonly ReactiveProperty<int> _matchPoints = new();
        private readonly ReactiveProperty<int> _currentMatchLevel = new();
        private readonly ReactiveProperty<bool> _isMatchActive = new();
        private readonly ReactiveProperty<bool> _isLevel5Completed = new();

        private readonly Subject<Unit> _onMatchStarted = new();
        private readonly Subject<RankedLevelResult> _onLevelCompleted = new();
        private readonly Subject<RankedMatchResult> _onMatchEnded = new();

        private readonly CompositeDisposable _disposables = new();
        private readonly CompositeDisposable _matchDisposables = new();

        private readonly RankedConfig _config;
        private readonly IRankedLevelGeneratorService _generatorService;
        private readonly ILevelLoaderService _levelLoaderService;
        private readonly ITimeLimitService _timeLimitService;
        private readonly IShadowValidationService _validationService;
        private readonly IProfileBuilder _profileBuilder;
        private readonly IArchetypeResolver _archetypeResolver;
        private readonly IInputContextService _contextService;

        public RankedModeService(
            RankedConfig config,
            IRankedLevelGeneratorService generatorService,
            ILevelLoaderService levelLoaderService,
            ITimeLimitService timeLimitService,
            IShadowValidationService validationService,
            IProfileBuilder profileBuilder,
            IArchetypeResolver archetypeResolver,
            IInputContextService contextService)
        {
            _config = config;
            _generatorService = generatorService;
            _levelLoaderService = levelLoaderService;
            _timeLimitService = timeLimitService;
            _validationService = validationService;
            _profileBuilder = profileBuilder;
            _archetypeResolver = archetypeResolver;
            _contextService = contextService;
        }

        public void Initialize()
        {
            _currentMmr.Value = LoadSavedMmr();

            if (RankedContext.IsRankedModeActive)
            {
                Observable.NextFrame(FrameCountType.EndOfFrame)
                    .Subscribe(_ => StartMatch())
                    .AddTo(_disposables);
            }
        }

        public int LoadSavedMmr() => PlayerPrefs.GetInt(_config.MmrPlayerPrefsKey, _config.StartingMmr);

        public void StartMatch()
        {
            RankedContext.StartMatch(_currentMmr.Value);
            _matchPoints.Value = 0;
            _currentMatchLevel.Value = 1;
            _isLevel5Completed.Value = false;
            _isMatchActive.Value = true;
            _matchDisposables.Clear();

            SubscribeToLevelEvents();
            LoadNextLevel();
            _onMatchStarted.OnNext(Unit.Default);
        }

        private void SubscribeToLevelEvents()
        {
            _validationService.OnLevelCompleted
                .Subscribe(_ => HandleLevelEnd(true))
                .AddTo(_matchDisposables);

            _timeLimitService.OnTimeExpired
                .Subscribe(_ => HandleLevelEnd(false))
                .AddTo(_matchDisposables);
        }

        public void CompleteLevel(bool success, float remainingTime, float totalTime) { }

        private void HandleLevelEnd(bool success)
        {
            if (!_isMatchActive.Value) return;

            var matchLevel = _currentMatchLevel.Value;
            var archetype = _archetypeResolver.Resolve(_currentMmr.Value);
            var difficulty = _profileBuilder.CalculateDifficulty(archetype, matchLevel, _matchPoints.Value);
            var totalTime = _profileBuilder.CalculateTimeLimit(difficulty);
            var remainingTime = _timeLimitService.RemainingTime.Value;

            var pointsDelta = success
                ? CalculateReward(difficulty, remainingTime, totalTime)
                : CalculatePenalty(difficulty);

            _matchPoints.Value += pointsDelta;
            RankedContext.MatchPoints = _matchPoints.Value;

            if (success && matchLevel == _config.LevelsPerMatch)
                _isLevel5Completed.Value = true;

            var result = new RankedLevelResult(
                matchLevel,
                success,
                pointsDelta,
                difficulty,
                remainingTime,
                totalTime);

            _onLevelCompleted.OnNext(result);

            if (success) RankedContext.LevelsCompleted++;
            else RankedContext.LevelsFailed++;

            if (matchLevel >= _config.LevelsPerMatch)
            {
                EndMatch();
            }
            else
            {
                _currentMatchLevel.Value++;
                RankedContext.CurrentMatchLevel = _currentMatchLevel.Value;
                LoadNextLevel();
            }
        }

        private void LoadNextLevel()
        {
            _contextService.SetContext(InputContext.Generating);
            var mmr = _currentMmr.Value;
            var level = _currentMatchLevel.Value;
            var points = _matchPoints.Value;

            var config = _generatorService.GenerateLevel(mmr, level, points);
            LevelContext.SelectedLevelConfig = config;

            _levelLoaderService.LoadLevel(config)
                .Subscribe(_ => _contextService.SetContext(InputContext.PlaceBlock))
                .AddTo(_matchDisposables);
        }

        public void EndMatch()
        {
            if (!_isMatchActive.Value) return;

            _isMatchActive.Value = false;
            _matchDisposables.Clear();

            var oldMmr = _currentMmr.Value;
            var matchPoints = _matchPoints.Value;
            var level5Bonus = _isLevel5Completed.Value ? _config.Level5Bonus : 0;
            var perfectRunBonus = RankedContext.IsPerfectRun ? _config.PerfectRunBonus : 0;

            var newMmr = Mathf.Max(_config.MinimumMmr, oldMmr + matchPoints + level5Bonus + perfectRunBonus);

            _currentMmr.Value = newMmr;
            RankedContext.PlayerMmr = newMmr;
            PlayerPrefs.SetInt(_config.MmrPlayerPrefsKey, newMmr);
            PlayerPrefs.Save();

            var matchResult = new RankedMatchResult(
                oldMmr,
                newMmr,
                matchPoints,
                level5Bonus,
                perfectRunBonus,
                RankedContext.LevelsCompleted,
                RankedContext.LevelsFailed);

            _onMatchEnded.OnNext(matchResult);
            RankedContext.Reset();
        }

        public void AbortMatch()
        {
            if (!_isMatchActive.Value) return;

            while (_currentMatchLevel.Value <= _config.LevelsPerMatch)
            {
                var archetype = _archetypeResolver.Resolve(_currentMmr.Value);
                var difficulty = _profileBuilder.CalculateDifficulty(archetype, _currentMatchLevel.Value, _matchPoints.Value);
                _matchPoints.Value += CalculatePenalty(difficulty);
                RankedContext.LevelsFailed++;
                _currentMatchLevel.Value++;
            }

            EndMatch();
        }

        private int CalculateReward(int difficulty, float remainingTime, float totalTime)
        {
            var baseReward = _config.BaseReward + (int)(difficulty * _config.RewardPerDifficulty);
            var timeBonus = totalTime > 0f ? (remainingTime / totalTime) * _config.TimeBonusMultiplier : 0f;
            return Mathf.RoundToInt(baseReward + timeBonus);
        }

        private int CalculatePenalty(int difficulty) => -(_config.BaseReward + (int)(difficulty * _config.RewardPerDifficulty));

        public void Dispose()
        {
            _matchDisposables?.Dispose();
            _disposables?.Dispose();
        }
    }
}