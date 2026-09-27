using System;
using System.Collections.Generic;
using UniRx;
using Zenject;
using Game.Data;
using UnityEngine;

namespace Game.Services.Achievements
{
    public class AchievementService : IAchievementService, IInitializable, IDisposable
    {
        public ReactiveCollection<AchievementRuntimeData> Achievements { get; } = new();
        public IObservable<AchievementRuntimeData> OnAchievementUnlocked => _onAchievementUnlocked;

        private readonly Subject<AchievementRuntimeData> _onAchievementUnlocked = new();
        private readonly CompositeDisposable _disposables = new();
        private readonly AchievementConfig[] _configs;
        private readonly IAchievementEventBus _eventBus;
        private readonly Dictionary<string, AchievementRuntimeData> _runtimeDataMap = new();
        private const string PlayerPrefsPrefix = "achievement_";

        public AchievementService(
            [InjectOptional] AchievementConfig[] configs,
            IAchievementEventBus eventBus)
        {
            _configs = configs ?? Array.Empty<AchievementConfig>();
            _eventBus = eventBus;
        }

        public void Initialize()
        {
            foreach (var config in _configs)
            {
                if (config is null) continue;
                var savedProgress = PlayerPrefs.GetInt($"{PlayerPrefsPrefix}{config.Id}_progress", 0);
                var isCompleted = PlayerPrefs.GetInt($"{PlayerPrefsPrefix}{config.Id}_completed", 0) == 1;
                var runtimeData = new AchievementRuntimeData(config, savedProgress, isCompleted);
                _runtimeDataMap[config.Id] = runtimeData;
                Achievements.Add(runtimeData);
            }

            _eventBus.Subscribe<BlockPlacedEvent>()
                .Subscribe(_ => HandleSimpleEvent(AchievementConditionType.PlaceBlocks))
                .AddTo(_disposables);

            _eventBus.Subscribe<StructurePlacedEvent>()
                .Subscribe(_ => HandleSimpleEvent(AchievementConditionType.PlaceStructures))
                .AddTo(_disposables);

            _eventBus.Subscribe<DecorSunkEvent>()
                .Subscribe(_ => HandleSimpleEvent(AchievementConditionType.Quack))
                .AddTo(_disposables);

            _eventBus.Subscribe<LevelCompletedEvent>()
                .Subscribe(HandleLevelCompletedEvent)
                .AddTo(_disposables);
        }

        private void HandleLevelCompletedEvent(LevelCompletedEvent evt)
        {
            foreach (var runtimeData in Achievements)
            {
                if (runtimeData.IsCompleted.Value) continue;

                var conditionMet = runtimeData.Config.ConditionType switch
                {
                    AchievementConditionType.CompleteLevels => true,
                    AchievementConditionType.CompleteWithRain => evt.HasRain && !evt.HasFog,
                    AchievementConditionType.CompleteWithFog => !evt.HasRain && evt.HasFog,
                    AchievementConditionType.CompleteWithRainAndFog => evt.HasRain && evt.HasFog,
                    _ => false
                };

                if (!conditionMet) continue;

                UpdateAndSaveProgress(runtimeData);
            }
        }

        private void HandleSimpleEvent(AchievementConditionType conditionType)
        {
            foreach (var runtimeData in Achievements)
            {
                if (runtimeData.Config.ConditionType != conditionType) continue;
                if (runtimeData.IsCompleted.Value) continue;

                UpdateAndSaveProgress(runtimeData);
            }
        }

        private void UpdateAndSaveProgress(AchievementRuntimeData runtimeData)
        {
            var newProgress = runtimeData.CurrentProgress.Value + 1;
            runtimeData.UpdateProgress(newProgress);

            if (runtimeData.IsCompleted.Value)
            {
                SaveState(runtimeData.Config.Id, newProgress, true);
                _onAchievementUnlocked.OnNext(runtimeData);
            }
            else
            {
                SaveState(runtimeData.Config.Id, newProgress, false);
            }
        }

        private void SaveState(string configId, int progress, bool isCompleted)
        {
            PlayerPrefs.SetInt($"{PlayerPrefsPrefix}{configId}_progress", progress);
            PlayerPrefs.SetInt($"{PlayerPrefsPrefix}{configId}_completed", isCompleted ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void Dispose() => _disposables?.Dispose();
    }
}