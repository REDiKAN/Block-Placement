using UnityEngine;
using Zenject;
using Game.Data;
using Game.Services.Settings;

namespace Game.Installers
{
    public class SettingsInstaller : MonoInstaller
    {
        [field: SerializeField] private SettingsConfig SettingsConfig { get; set; }

        public override void InstallBindings()
        {
            if (SettingsConfig is not null)
                Container.BindInstance(SettingsConfig);

            Bind<SettingsService>();
        }

        private void Bind<TImplementation>() where TImplementation : class =>
            Container
                .BindInterfacesTo<TImplementation>()
                .AsSingle()
                .Lazy();
    }
}