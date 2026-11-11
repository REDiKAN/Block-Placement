using UnityEngine;
using Zenject;
using Game.Data.Ranked;
using Game.Services.Ranked;

namespace Game.Installers
{
    public class RankedInstaller : MonoInstaller
    {
        [field: SerializeField] private RankedCoreConfig RankedCoreConfig { get; set; }
        [field: SerializeField] private RankedBlocksConfig RankedBlocksConfig { get; set; }
        [field: SerializeField] private RankedStructuresConfig RankedStructuresConfig { get; set; }

        public override void InstallBindings()
        {
            if (RankedCoreConfig is not null) Container.BindInstance(RankedCoreConfig);
            if (RankedBlocksConfig is not null) Container.BindInstance(RankedBlocksConfig);
            if (RankedStructuresConfig is not null) Container.BindInstance(RankedStructuresConfig);

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