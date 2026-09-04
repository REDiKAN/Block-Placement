using UniRx;
using Game.Data;

namespace Game.Services.Achievements
{
    public class AchievementRuntimeData
    {
        public AchievementConfig Config { get; }
        public IReadOnlyReactiveProperty<int> CurrentProgress { get; }
        public IReadOnlyReactiveProperty<bool> IsCompleted { get; }

        private readonly ReactiveProperty<int> _currentProgress;
        private readonly ReactiveProperty<bool> _isCompleted;

        public AchievementRuntimeData(AchievementConfig config, int savedProgress, bool isCompleted)
        {
            Config = config;
            _currentProgress = new ReactiveProperty<int>(savedProgress);
            _isCompleted = new ReactiveProperty<bool>(isCompleted);
            CurrentProgress = _currentProgress;
            IsCompleted = _isCompleted;
        }

        public void UpdateProgress(int newProgress)
        {
            if (_isCompleted.Value) return;

            var clampedProgress = newProgress > Config.TargetValue ? Config.TargetValue : newProgress;
            if (clampedProgress > _currentProgress.Value)
            {
                _currentProgress.Value = clampedProgress;
            }

            if (clampedProgress >= Config.TargetValue)
            {
                _isCompleted.Value = true;
            }
        }
    }
}