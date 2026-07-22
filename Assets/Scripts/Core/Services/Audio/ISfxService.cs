using UnityEngine;

namespace Game.Services.Audio
{
    public interface ISfxService
    {
        void Play(AudioClip clip);
        void Play(AudioClip clip, float volume);
    }
}