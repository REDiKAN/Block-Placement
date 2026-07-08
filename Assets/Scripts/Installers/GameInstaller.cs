using Game.Data;
using Game.Services.Dev;
using Game.Services.Grid;
using Game.Services.History;
using Game.Services.Input;
using Game.Services.Placement;
using Game.Services.Pool;
using Game.Services.Raycast;
using Game.Services.Registry;
using Game.Services.Rotation;
using Game.Services.Shadow;
using Game.Views;
using UnityEngine;
using Zenject;

namespace Game.Installers
{
    public class GameInstaller : MonoInstaller
    {
        [field: SerializeField] public BlockConfig[] BlockConfigs { get; private set; }
        [field: SerializeField] public BlockView BlockPrefab { get; private set; }
        [field: SerializeField] public BlockView PreviewBlock { get; private set; }
        [field: SerializeField] public Transform BlocksParent { get; private set; }
        [field: SerializeField] public Camera GameCamera { get; private set; }
        [field: SerializeField] public ShadowLevelConfig ShadowConfig { get; private set; }
        [field: SerializeField] public RaycastConfig RaycastConfig { get; private set; }
        [field: SerializeField] public Transform RotationPivot { get; private set; }
        [field: SerializeField] public RotationConfig RotationConfig { get; private set; }
        [field: SerializeField] public bool IsDeveloperMode { get; private set; }

        public override void InstallBindings()
        {
            Container.BindInstance(RotationPivot).WithId("RotationPivot");
            Container.BindInstance(RotationConfig);
            Container.BindInstance(BlockPrefab).WithId("BlockPrefab");
            Container.BindInstance(PreviewBlock).WithId("PreviewBlock");
            Container.BindInstance(BlocksParent);
            Container.BindInstance(GameCamera);
            Container.BindInstance(ShadowConfig);
            Container.BindInstance(RaycastConfig);
            Container.BindInstance(IsDeveloperMode).WithId("IsDeveloperMode");

            if (BlockConfigs is not null && BlockConfigs.Length > 0)
                Container.BindInstance(BlockConfigs);

            Bind<InputService>();
            Bind<InputContextService>();
            Bind<GridService>();
            Bind<RaycastService>();
            Bind<ObjectRegistryService>();
            Bind<BlockPoolService>();
            Bind<BlockHistoryService>();
            Bind<DevModeService>();
            Bind<BlockPlacementService>();
            Bind<ShadowCalculationService>();
            Bind<ShadowValidationService>();
            Bind<DevInputService>();
            Bind<DevLevelExportService>();
            Bind<RotationService>();
        }

        private void Bind<TImplementation>() where TImplementation : class =>
            Container
                .BindInterfacesTo<TImplementation>()
                .AsSingle()
                .Lazy();
    }
}