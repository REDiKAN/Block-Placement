using UnityEngine;
using Zenject;
using Game.Data.Ranked;
using Game.Services.Ranked;

namespace Game.Installers
{
    public class RankedInstaller : MonoInstaller
    {
        [field: SerializeField] private RankedConfig RankedConfig { get; set; }

        public override void InstallBindings()
        {
            if (RankedConfig is not null)
                Container.BindInstance(RankedConfig);

            Bind<ArchetypeResolver>();
            Bind<ProfileBuilder>();
            Bind<SolverService>();
            Bind<ShapeGenerator>();
            Bind<QualityValidator>();
            Bind<RankedLevelGeneratorService>();
            Bind<RankedModeService>();
        }

        private void Bind<TImplementation>() where TImplementation : class =>
            Container
                .BindInterfacesTo<TImplementation>()
                .AsSingle()
                .Lazy();
    }
}