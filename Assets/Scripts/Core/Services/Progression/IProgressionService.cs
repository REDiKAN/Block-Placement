using System;
using UniRx;

namespace Game.Services.Progression
{
    public readonly struct ProgressionData
    {
        public int CategoryId { get; }
        public int CompletedCount { get; }
        public int TotalLevels { get; }
        public float ProgressPercent { get; }

        public ProgressionData(int categoryId, int completedCount, int totalLevels, float progressPercent)
        {
            CategoryId = categoryId;
            CompletedCount = completedCount;
            TotalLevels = totalLevels;
            ProgressPercent = progressPercent;
        }
    }

    public interface IProgressionService
    {
        IObservable<ProgressionData> OnProgressionChanged { get; }
        ProgressionData GetProgression(int categoryId);
        bool IsLevelUnlocked(int categoryId, int levelIndex);
        int GetUnlockedCount(int categoryId);
        void MarkLevelCompleted(int categoryId, int levelIndex);
    }
}