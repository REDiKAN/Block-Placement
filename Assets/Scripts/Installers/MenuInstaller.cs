using Game.Data;
using Game.Services.Achievements;
using Game.Services.Generation;
using Game.Services.Menu;
using Game.Services.Progression;
using UnityEngine;
using Zenject;

namespace Game.Installers
{
    public class MenuInstaller : MonoInstaller
    {
        [field: SerializeField] public LevelCatalog Catalog { get; private set; }
        [field: SerializeField] public AchievementConfig[] AchievementConfigs { get; private set; }


        public override void InstallBindings()
        {
            if (AchievementConfigs is not null && AchievementConfigs.Length > 0)
                Container.BindInstance(AchievementConfigs);

            Container.BindInstance(Catalog);
            Bind<MenuNavigationService>();
            Bind<PreviewService>();
            Bind<CategoryContextService>();
            Bind<ProgressionService>();
            Bind<GenerationContext>();

            Container.BindInterfacesTo<AchievementEventBus>().AsCached();
            Container.BindInterfacesTo<AchievementService>().AsCached();

        }

        private void Bind<TImplementation>() where TImplementation : class =>
            Container.BindInterfacesTo<TImplementation>().AsSingle().Lazy();
    }
}