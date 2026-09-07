using UnityEngine;
using Zenject;
using Game.Data;
using Game.Views.Effects;
using Game.Services.EnvironmentEffects;

namespace Game.Installers
{
    public class EnvironmentEffectInstaller : MonoInstaller
    {
        [field: SerializeField] private RainConfig RainConfig { get; set; }

        public override void InstallBindings()
        {
            if (RainConfig is not null)
                Container.BindInstance(RainConfig);

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