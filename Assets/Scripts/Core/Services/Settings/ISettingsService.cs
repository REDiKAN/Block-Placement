using UniRx;
using Game.Data;

namespace Game.Services.Settings
{
    public interface ISettingsService
    {
        IReadOnlyReactiveProperty<int> CurrentQualityLevel { get; }
        IReadOnlyReactiveProperty<ResolutionData> CurrentResolution { get; }
        IReadOnlyReactiveProperty<bool> IsFullscreen { get; }
        IReadOnlyReactiveProperty<bool> IsPreviewEnabled { get; }
        IReadOnlyReactiveProperty<int> CurrentFpsLimit { get; }

        void CycleQuality();
        void CycleResolution();
        void CycleFullscreen();
        void CyclePreview();
        void SetFpsLimitByIndex(int index);
    }
}