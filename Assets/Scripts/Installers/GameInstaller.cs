using Game.Core;
using Game.Data;
using Game.Services.Achievements;
using Game.Services.Animation;
using Game.Services.Dev;
using Game.Services.Generation;
using Game.Services.Grid;
using Game.Services.History;
using Game.Services.Input;
using Game.Services.Placement;
using Game.Services.Pool;
using Game.Services.Progression;
using Game.Services.Raycast;
using Game.Services.Registry;
using Game.Services.Rotation;
using Game.Services.Shadow;
using Game.Services.Time;
using Game.Views;
using UnityEngine;
using Zenject;

namespace Game.Installers
{
    public class GameInstaller : MonoInstaller
    {
        [field: SerializeField] public BlockConfig[] BlockConfigs { get; private set; }
        [field: SerializeField] public StructureConfig[] StructureConfigs { get; private set; }
        [field: SerializeField] public BlockView BlockPrefab { get; private set; }
        [field: SerializeField] public BlockView PreviewBlock { get; private set; }
        [field: SerializeField] public Transform BlocksParent { get; private set; }
        [field: SerializeField] public Camera GameCamera { get; private set; }
        [field: SerializeField] public LevelConfig LevelConfig { get; private set; }
        [field: SerializeField] public RaycastConfig RaycastConfig { get; private set; }
        [field: SerializeField] public Transform RotationPivot { get; private set; }
        [field: SerializeField] public RotationConfig RotationConfig { get; private set; }
        [field: SerializeField] public bool IsDeveloperMode { get; private set; }
        [field: SerializeField] public LevelCatalog LevelCatalog { get; private set; }
        [field: SerializeField] public FloorGridView FloorGridView { get; private set; }
        [field: SerializeField] public WallView[] WallViews { get; private set; }
        [field: SerializeField] public BlockAnimationConfig BlockAnimationConfig { get; private set; }
        [field: SerializeField] public StructureAnimationConfig StructureAnimationConfig { get; private set; }
        [field: SerializeField] public AchievementConfig[] AchievementConfigs { get; private set; }

        public override void InstallBindings()
        {
            Container.BindInstance(RotationPivot).WithId("RotationPivot");
            Container.BindInstance(RotationConfig);
            Container.BindInstance(BlockPrefab).WithId("BlockPrefab");
            Container.BindInstance(PreviewBlock).WithId("PreviewBlock");
            Container.BindInstance(BlocksParent);
            Container.BindInstance(GameCamera);
            Container.BindInstance(GameCamera).WithId("GameCamera");

            var activeConfig = LevelConfig;
            if (LevelCatalog is not null && LevelCatalog.Categories is not null &&
                LevelContext.SelectedCategoryId >= 0 && LevelContext.SelectedCategoryId < LevelCatalog.Categories.Length)
            {
                var activeCategory = LevelCatalog.Categories[LevelContext.SelectedCategoryId];
                if (activeCategory is not null && activeCategory.Levels is not null &&
                    LevelContext.SelectedLevelId >= 0 && LevelContext.SelectedLevelId < activeCategory.Levels.Length)
                {
                    activeConfig = activeCategory.Levels[LevelContext.SelectedLevelId];
                }
            }

            Container.BindInstance(activeConfig);
            Container.BindInstance(RaycastConfig);
            Container.BindInstance(IsDeveloperMode).WithId("IsDeveloperMode");
            Container.BindInstance(LevelCatalog);

            if (BlockConfigs is not null && BlockConfigs.Length > 0)
                Container.BindInstance(BlockConfigs);
            if (StructureConfigs is not null && StructureConfigs.Length > 0)
                Container.BindInstance(StructureConfigs);
            if (AchievementConfigs is not null && AchievementConfigs.Length > 0)
                Container.BindInstance(AchievementConfigs);

            Container.BindInstance(FloorGridView);
            Container.BindInstance(WallViews);

            if (BlockAnimationConfig is not null)
                Container.BindInstance(BlockAnimationConfig);
            if (StructureAnimationConfig is not null)
                Container.BindInstance(StructureAnimationConfig);

            Container.BindInterfacesTo<AchievementEventBus>().AsCached();
            Container.BindInterfacesTo<AchievementService>().AsCached();

            Bind<InputService>();
            Bind<InputContextService>();
            Bind<GridService>();
            Bind<RaycastService>();
            Bind<ObjectRegistryService>();
            Bind<BlockPoolService>();
            Bind<StructurePoolService>();
            Bind<PlacementHistoryService>();
            Bind<DevModeService>();
            Bind<BlockPlacementService>();
            Bind<StructurePlacementService>();
            Bind<ShadowCalculationService>();
            Bind<TargetDensityProjectionService>();
            Bind<ShadowValidationService>();
            Bind<DevInputService>();
            Bind<DevLevelExportService>();
            Bind<RotationService>();
            Bind<ShadowDensityService>();
            Bind<CellHoverService>();
            Bind<TimeLimitService>();
            Bind<LevelProgressionService>();
            Bind<LevelGeneratorService>();
            Bind<ShakeAnimationService>();
            Bind<BlockAnimationService>();
            Bind<StructureAnimationService>();
            Bind<ProgressionService>();
            Bind<GenerationContext>();
            Bind<EndlessGeneratorService>();
            Bind<LevelIntroAnimationService>();

            Container.BindInterfacesTo<CascadeIntroStrategy>().AsSingle().Lazy();
            Container.BindInterfacesTo<WaveFromCenterStrategy>().AsSingle().Lazy();
            Container.BindInterfacesTo<RowByRowStrategy>().AsSingle().Lazy();
        }

        private void Bind<TImplementation>() where TImplementation : class =>
            Container
                .BindInterfacesTo<TImplementation>()
                .AsSingle()
                .Lazy();
    }
}