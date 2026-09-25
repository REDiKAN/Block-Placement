using UnityEngine;

namespace Game.Services.Audio
{
    public interface IMusicService
    {
        void Play(AudioClip clip);
        void Stop();
        void SetVolume(float volume);
        void GetSpectrumData(float[] samples, int channel, FFTWindow window);
    }
}