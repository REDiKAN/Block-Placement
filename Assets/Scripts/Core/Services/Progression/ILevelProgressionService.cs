using System;
using UniRx;

namespace Game.Services.Progression
{
    public readonly struct LevelTransitionData
    {
        public string SceneName { get; }
        public int NextLevelId { get; }

        public LevelTransitionData(string sceneName, int nextLevelId)
        {
            SceneName = sceneName;
            NextLevelId = nextLevelId;
        }
    }

    public interface ILevelProgressionService
    {
        IObservable<string> OnLevelCompletedMessage { get; }
        IObservable<LevelTransitionData> OnTransitionRequested { get; }
        void RequestRestart();
    }
}