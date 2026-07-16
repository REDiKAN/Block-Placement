using UnityEngine;
using Zenject;
using Game.Data;
using Game.Services.Menu;

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
        }

        private void Bind<TImplementation>() where TImplementation : class =>
            Container.BindInterfacesTo<TImplementation>().AsSingle().Lazy();
    }
}