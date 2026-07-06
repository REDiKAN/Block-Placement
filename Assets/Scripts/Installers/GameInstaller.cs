using Game.Data;
using Game.Services.Grid;
using Game.Services.History;
using Game.Services.Input;
using Game.Services.Placement;
using Game.Services.Pool;
using Game.Services.Raycast;
using Game.Services.Shadow;
using Game.Views;
using UnityEngine;
using Zenject;

namespace Game.Installers
{
    public class GameInstaller : MonoInstaller
    {
        [field: SerializeField] public BlockView BlockPrefab { get; private set; }
        [field: SerializeField] public BlockView PreviewBlock { get; private set; }
        [field: SerializeField] public Transform BlocksParent { get; private set; }
        [field: SerializeField] public Camera GameCamera { get; private set; }
        [field: SerializeField] public ShadowLevelConfig ShadowConfig { get; private set; }
        [field: SerializeField] public RaycastConfig RaycastConfig { get; private set; }

        public override void InstallBindings()
        {
            Container.BindInstance(BlockPrefab).WithId("BlockPrefab");
            Container.BindInstance(PreviewBlock).WithId("PreviewBlock");
            Container.BindInstance(BlocksParent);
            Container.BindInstance(GameCamera);
            Container.BindInstance(ShadowConfig);
            Container.BindInstance(RaycastConfig);

            Bind<InputService>();
            Bind<GridService>();
            Bind<RaycastService>();
            Bind<BlockPoolService>();
            Bind<BlockHistoryService>();
            Bind<BlockPlacementService>();
            Bind<ShadowValidationService>();
        }

        private void Bind<TImplementation>() where TImplementation : class
        {
            Container
                .BindInterfacesTo<TImplementation>()
                .AsSingle()
                .Lazy();
        }
    }
}