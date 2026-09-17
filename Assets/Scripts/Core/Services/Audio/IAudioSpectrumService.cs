using System;

namespace Game.Services.Audio
{
    public interface IAudioSpectrumService
    {
        int BarCount { get; }
        IObservable<float[]> OnSpectrumUpdated { get; }
    }
}