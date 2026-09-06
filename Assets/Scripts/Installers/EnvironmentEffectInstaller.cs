using Zenject;
using Game.Views.Effects;
using Game.Services.EnvironmentEffects;

namespace Game.Installers
{
    public class EnvironmentEffectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IEffectView>().FromComponentsInHierarchy().AsCached();
            Bind<EnvironmentEffectService>();
        }

        private void Bind<TImplementation>() where TImplementation : class =>
            Container
                .BindInterfacesTo<TImplementation>()
                .AsSingle()
                .Lazy();
    }
}