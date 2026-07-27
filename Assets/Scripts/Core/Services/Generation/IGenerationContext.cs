using UniRx;
using Game.Data;

namespace Game.Services.Generation
{
    public interface IGenerationContext
    {
        IReadOnlyReactiveProperty<bool> IsEndlessModeActive { get; }
        IReadOnlyReactiveProperty<CustomGenerationSettings> CurrentSettings { get; }
    }
}