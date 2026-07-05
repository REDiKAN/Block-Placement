using System.IO;

namespace ScriptCombiner.Editor.Utilities
{
    public static class FileHelper
    {
        public static bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public static bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }
    }
}