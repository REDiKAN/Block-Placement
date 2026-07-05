namespace ScriptCombiner.Editor.Constants
{
    public static class ScriptCombinerConstants
    {
        public const string WindowTitle = "Script Combiner";
        public const string MenuItemPath = "Tools/Script Combiner";

        public const float MinWindowWidth = 450f;
        public const float MinWindowHeight = 600f;
        public const float ButtonHeight = 25f;
        public const float LargeButtonHeight = 30f;
        public const float DropAreaHeight = 45f;
        public const float ListHeight = 100f;

        public const string DefaultExclusionPatterns = "Test, Temp, AssemblyInfo";
        public const string FileExtensionCSharp = ".cs";
        public const string FileExtensionOutput = "txt";

        public const string HeaderTemplate = "// ===== Combined Scripts Header =====";
        public const string FooterTemplate = "// =====================================";
        public const string UsingsHeader = "// ===== Consolidated Usings =====";

        public const string LogPrefix = "[ScriptCombiner]";
    }
}