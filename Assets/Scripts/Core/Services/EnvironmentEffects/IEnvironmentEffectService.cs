using Game.Views.Effects;

namespace Game.Services.EnvironmentEffects
{
    public interface IEnvironmentEffectService
    {
        void Regenerate();
        bool IsEffectVisible<T>() where T : class, IEffectView;
    }
}