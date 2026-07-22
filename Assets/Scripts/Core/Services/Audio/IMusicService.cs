using UnityEngine;

namespace Game.Services.Audio
{
    public interface IMusicService
    {
        void Play(AudioClip clip);
        void Stop();
    }
}