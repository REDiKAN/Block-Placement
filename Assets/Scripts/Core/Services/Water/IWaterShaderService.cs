using UniRx;
using Game.Data;

namespace Game.Services.Water
{
    public interface IWaterShaderService
    {
        IReadOnlyReactiveProperty<WaterShaderConfig> CurrentConfig { get; }
        IReadOnlyReactiveProperty<WaterShaderParameters> CurrentParameters { get; }
        void CycleConfig();
    }
}