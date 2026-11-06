using System;
using System.Collections.Generic;
using UniRx;

namespace Game.Services.Ranked
{
    public readonly struct RankedMatchResult
    {
        public int OldMmr { get; }
        public int NewMmr { get; }
        public int MatchPoints { get; }
        public int Level5Bonus { get; }
        public int PerfectRunBonus { get; }
        public int LevelsCompleted { get; }
        public int LevelsFailed { get; }
        public bool IsPerfectRun { get; }
        public int MmrDelta => NewMmr - OldMmr;

        public RankedMatchResult(
            int oldMmr,
            int newMmr,
            int matchPoints,
            int level5Bonus,
            int perfectRunBonus,
            int levelsCompleted,
            int levelsFailed)
        {
            OldMmr = oldMmr;
            NewMmr = newMmr;
            MatchPoints = matchPoints;
            Level5Bonus = level5Bonus;
            PerfectRunBonus = perfectRunBonus;
            LevelsCompleted = levelsCompleted;
            LevelsFailed = levelsFailed;
            IsPerfectRun = levelsFailed == 0 && levelsCompleted > 0;
        }
    }

    public readonly struct RankedLevelResult
    {
        public int MatchLevel { get; }
        public bool IsCompleted { get; }
        public int PointsDelta { get; }
        public int Difficulty { get; }
        public float RemainingTime { get; }
        public float TotalTime { get; }

        public RankedLevelResult(
            int matchLevel,
            bool isCompleted,
            int pointsDelta,
            int difficulty,
            float remainingTime,
            float totalTime)
        {
            MatchLevel = matchLevel;
            IsCompleted = isCompleted;
            PointsDelta = pointsDelta;
            Difficulty = difficulty;
            RemainingTime = remainingTime;
            TotalTime = totalTime;
        }
    }

    public interface IRankedModeService
    {
        IReadOnlyReactiveProperty<int> CurrentMmr { get; }
        IReadOnlyReactiveProperty<int> MatchPoints { get; }
        IReadOnlyReactiveProperty<int> CurrentMatchLevel { get; }
        IReadOnlyReactiveProperty<bool> IsMatchActive { get; }
        IReadOnlyReactiveProperty<bool> IsLevel5Completed { get; }
        IReadOnlyReactiveProperty<IReadOnlyList<RankedLevelResult>> LevelResults { get; }
        IObservable<Unit> OnMatchStarted { get; }
        IObservable<RankedLevelResult> OnLevelCompleted { get; }
        IObservable<RankedMatchResult> OnMatchEnded { get; }
        void StartMatch();
        void ConfirmPreRoundSummary();
        void CompleteLevel(bool success, float remainingTime, float totalTime);
        void EndMatch();
        void AbortMatch();
        int LoadSavedMmr();
    }
}