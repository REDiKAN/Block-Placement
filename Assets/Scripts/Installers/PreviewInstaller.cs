using UnityEngine;
using Zenject;
using Game.Data;
using Game.Services.Grid;
using Game.Services.Shadow;
using Game.Services.Preview;

namespace Game.Installers
{
    public class PreviewInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            var emptyConfig = ScriptableObject.CreateInstance<LevelConfig>();
            Container.BindInstance(emptyConfig);
            Container.BindInstance(false).WithId("IsDeveloperMode");

            Bind<GridService>();
            Bind<ShadowCalculationService>();
            Bind<PreviewShadowService>();
            Bind<PreviewStateManager>();
        }

        private void Bind<TImplementation>() where TImplementation : class =>
            Container.BindInterfacesAndSelfTo<TImplementation>().AsSingle().Lazy();
    }
}