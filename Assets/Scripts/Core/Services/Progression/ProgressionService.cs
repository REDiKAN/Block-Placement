using System;
using UniRx;
using Zenject;
using Game.Data;
using UnityEngine;

namespace Game.Services.Progression
{
    public class ProgressionService : IProgressionService, IInitializable, IDisposable
    {
        public IObservable<ProgressionData> OnProgressionChanged => _onProgressionChanged;

        private readonly Subject<ProgressionData> _onProgressionChanged = new();
        private readonly CompositeDisposable _disposables = new();
        private readonly LevelCatalog _catalog;

        private const string _playerPrefsKeyPrefix = "progression_";
        private const string _playerPrefsKeySuffix = "_completed";

        public ProgressionService(LevelCatalog catalog)
        {
            _catalog = catalog;
        }

        public void Initialize()
        {
            if (_catalog?.Categories is null) return;
            for (var i = 0; i < _catalog.Categories.Length; i++)
                PublishProgression(i);
        }

        public ProgressionData GetProgression(int categoryId)
        {
            var category = GetCategory(categoryId);
            if (category?.Levels is null) return default;
            var completedCount = GetCompletedCount(categoryId);
            var totalLevels = category.Levels.Length;
            var progressPercent = totalLevels > 0 ? (float)completedCount / totalLevels * 100f : 0f;
            return new ProgressionData(categoryId, completedCount, totalLevels, progressPercent);
        }

        public bool IsLevelUnlocked(int categoryId, int levelIndex)
        {
            var category = GetCategory(categoryId);
            if (category is null) return false;
            if (!category.IsSequential) return true;
            return levelIndex <= GetCompletedCount(categoryId);
        }

        public int GetUnlockedCount(int categoryId)
        {
            var category = GetCategory(categoryId);
            if (category?.Levels is null) return 0;
            if (!category.IsSequential) return category.Levels.Length;
            var completedCount = GetCompletedCount(categoryId);
            return Mathf.Min(completedCount + 1, category.Levels.Length);
        }

        public void MarkLevelCompleted(int categoryId, int levelIndex)
        {
            var category = GetCategory(categoryId);
            if (category is null || !category.IsSequential) return;
            var completedCount = GetCompletedCount(categoryId);
            if (levelIndex != completedCount) return;
            PlayerPrefs.SetInt(BuildPlayerPrefsKey(categoryId), completedCount + 1);
            PlayerPrefs.Save();
            PublishProgression(categoryId);
        }

        private CategoryConfig GetCategory(int categoryId)
        {
            if (_catalog?.Categories is null || categoryId < 0 || categoryId >= _catalog.Categories.Length)
                return null;
            return _catalog.Categories[categoryId];
        }

        private int GetCompletedCount(int categoryId) =>
            PlayerPrefs.GetInt(BuildPlayerPrefsKey(categoryId), 0);

        private string BuildPlayerPrefsKey(int categoryId) =>
            $"{_playerPrefsKeyPrefix}{categoryId}{_playerPrefsKeySuffix}";

        private void PublishProgression(int categoryId)
        {
            var data = GetProgression(categoryId);
            _onProgressionChanged.OnNext(data);
        }

        public void Dispose() => _disposables?.Dispose();
    }
}