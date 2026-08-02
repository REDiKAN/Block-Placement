namespace Game.Core
{
    public static class LevelContext
    {
        public static int SelectedLevelId { get; set; }
        public static int SelectedCategoryId { get; set; }

        public static void Reset()
        {
            SelectedLevelId = 0;
            SelectedCategoryId = 0;
        }
    }
}