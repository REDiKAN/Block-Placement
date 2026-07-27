using Game.Data;
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

        public override void InstallBindings()
        {
            Container.BindInstance(Catalog);
            Bind<MenuNavigationService>();
            Bind<PreviewService>();
            Bind<CategoryContextService>();
            Bind<ProgressionService>();
            Bind<GenerationContext>();
        }

        private void Bind<TImplementation>() where TImplementation : class =>
            Container.BindInterfacesTo<TImplementation>().AsSingle().Lazy();
    }
}