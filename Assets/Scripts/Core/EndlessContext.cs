using Game.Data;

namespace Game.Core
{
    public static class EndlessContext
    {
        public static bool IsEndlessModeActive { get; set; }
        public static CustomGenerationSettings Settings { get; set; }

        public static void Reset()
        {
            IsEndlessModeActive = false;
            Settings = default;
        }
    }
}