using UniRx;

namespace Game.Services.Dev
{
    public enum AutoTesterState
    {
        Idle,
        Running,
        Paused
    }

    public interface ILevelAutoTesterService
    {
        IReadOnlyReactiveProperty<AutoTesterState> State { get; }
        IReadOnlyReactiveProperty<int> SpawnedCount { get; }
        IReadOnlyReactiveProperty<int> TotalRequired { get; }
        IReadOnlyReactiveProperty<string> ObjectLabel { get; }
        IReadOnlyReactiveProperty<int> ConfiguredLimit { get; }
        IReadOnlyReactiveProperty<bool> IsBlockLimitEnabled { get; }
        IReadOnlyReactiveProperty<bool> IsTimeLimitEnabled { get; }
        IReadOnlyReactiveProperty<float> RemainingTime { get; }

        void StartTest(float delay);
        void PauseTest();
        void ResumeTest();
        void StepTest();
        void StopTest();
    }
}