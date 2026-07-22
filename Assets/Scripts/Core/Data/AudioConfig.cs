using UnityEngine;
using UnityEngine.Audio;
using Game.Attributes;

namespace Game.Data
{
    [CreateAssetMenu(fileName = nameof(AudioConfig), menuName = "Game/" + nameof(AudioConfig))]
    public class AudioConfig : ScriptableObject
    {
        [field: Title("Audio Mixer Groups", CustomColor.Cyan, CustomColor.Blue)]
        [field: SerializeField] public AudioMixerGroup SfxMixerGroup { get; private set; }
        [field: SerializeField] public AudioMixerGroup MusicMixerGroup { get; private set; }

        [field: Title("SFX Settings", CustomColor.Yellow, CustomColor.Orange)]
        [field: SerializeField, Range(1, 50)] public int SfxPoolSize { get; private set; } = 10;
        [field: SerializeField, Range(0f, 1f)] public float DefaultSfxVolume { get; private set; } = 1f;

        [field: Title("Default Gameplay SFX", CustomColor.Green, CustomColor.DarkGreen)]
        [field: SerializeField] public AudioClip DefaultPlaceClip { get; private set; }
        [field: SerializeField] public AudioClip DefaultRemoveClip { get; private set; }

        [field: Title("Music Settings", CustomColor.Cyan, CustomColor.Blue)]
        [field: SerializeField, Range(0f, 1f)] public float DefaultMusicVolume { get; private set; } = 0.8f;
        [field: SerializeField, Range(0.1f, 5f)] public float CrossfadeDuration { get; private set; } = 1.5f;
    }
}