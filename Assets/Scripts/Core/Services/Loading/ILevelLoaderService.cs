using System;
using UniRx;
using Game.Data;

namespace Game.Services.Loading
{
    public interface ILevelLoaderService
    {
        IObservable<Unit> LoadLevel(LevelConfig config);
    }
}