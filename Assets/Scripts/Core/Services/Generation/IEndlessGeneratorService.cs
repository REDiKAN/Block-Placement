using System;
using UniRx;

namespace Game.Services.Generation
{
    public interface IEndlessGeneratorService
    {
        IObservable<Unit> OnLevelGenerated { get; }
        void GenerateNext();
    }
}