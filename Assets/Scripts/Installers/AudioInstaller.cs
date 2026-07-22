using UnityEngine;
using Zenject;
using Game.Data;
using Game.Services.Audio;

namespace Game.Installers
{
    public class AudioInstaller : MonoInstaller
    {
        [field: SerializeField] private AudioConfig AudioConfig { get; set; }
        [field: SerializeField] private AudioClip StartMusicClip { get; set; }

        public override void InstallBindings()
        {
            if (AudioConfig is not null)
                Container.BindInstance(AudioConfig);

            if (StartMusicClip is not null)
                Container.BindInstance(StartMusicClip);

            Bind<SfxService>();
            Bind<MusicService>();
        }

        private void Bind<TImplementation>() where TImplementation : class =>
            Container
                .BindInterfacesTo<TImplementation>()
                .AsSingle()
                .Lazy();
    }
}