namespace Game.Data
{
    public readonly struct CustomGenerationSettings
    {
        public bool HasFloorHoles { get; }
        public bool UseDensity { get; }
        public bool IsSymmetrical { get; }
        public int Difficulty { get; }

        public CustomGenerationSettings(bool hasFloorHoles, bool useDensity, bool isSymmetrical, int difficulty)
        {
            HasFloorHoles = hasFloorHoles;
            UseDensity = useDensity;
            IsSymmetrical = isSymmetrical;
            Difficulty = difficulty;
        }
    }
}