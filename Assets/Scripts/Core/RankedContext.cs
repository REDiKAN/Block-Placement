using Game.Data;

namespace Game.Core
{
    public static class RankedContext
    {
        public static bool IsRankedModeActive { get; set; }
        public static GameMode CurrentMode { get; set; }
        public static int PlayerMmr { get; set; }
        public static int MatchPoints { get; set; }
        public static int CurrentMatchLevel { get; set; }
        public static int LevelsCompleted { get; set; }
        public static int LevelsFailed { get; set; }
        public static bool IsLevel5Completed { get; set; }

        public static void Reset()
        {
            IsRankedModeActive = false;
            CurrentMode = GameMode.Blocks;
            PlayerMmr = 0;
            MatchPoints = 0;
            CurrentMatchLevel = 1;
            LevelsCompleted = 0;
            LevelsFailed = 0;
            IsLevel5Completed = false;
        }

        public static void StartMatch(int startingMmr, GameMode mode)
        {
            IsRankedModeActive = true;
            CurrentMode = mode;
            PlayerMmr = startingMmr;
            MatchPoints = 0;
            CurrentMatchLevel = 1;
            LevelsCompleted = 0;
            LevelsFailed = 0;
            IsLevel5Completed = false;
        }

        public static bool IsPerfectRun => LevelsFailed == 0 && LevelsCompleted > 0;
    }
}