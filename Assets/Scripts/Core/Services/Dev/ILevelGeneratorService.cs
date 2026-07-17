using System;
using UniRx;

namespace Game.Services.Dev
{
    public interface ILevelGeneratorService
    {
        IObservable<(int Current, int Total)> OnProgress { get; }
        IObservable<bool> OnGenerationCompleted { get; }
        IObservable<bool> OnValidationCompleted { get; }
        void Generate(int difficulty, int strategy);
        void ValidateSolvability();
    }
}