using UnityEngine;
using Zenject;
using Game.Data;
using Game.Services.Water;

namespace Game.Installers
{
    public class WaterInstaller : MonoInstaller
    {
        [field: SerializeField] private WaterShaderConfigCatalog Catalog { get; set; }

        public override void InstallBindings()
        {
            if (Catalog is not null)
                Container.BindInstance(Catalog);

            Bind<WaterShaderService>();
        }

        private void Bind<TImplementation>() where TImplementation : class =>
            Container
                .BindInterfacesTo<TImplementation>()
                .AsSingle()
                .Lazy();
    }
}