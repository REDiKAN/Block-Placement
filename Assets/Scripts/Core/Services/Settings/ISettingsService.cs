using UniRx;
using Game.Data;

namespace Game.Services.Settings
{
    public interface ISettingsService
    {
        IReadOnlyReactiveProperty<int> CurrentQualityLevel { get; }
        IReadOnlyReactiveProperty<ResolutionData> CurrentResolution { get; }
        IReadOnlyReactiveProperty<bool> IsFullscreen { get; }

        void CycleQuality();
        void CycleResolution();
        void CycleFullscreen();
    }
}