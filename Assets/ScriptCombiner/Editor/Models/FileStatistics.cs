namespace ScriptCombiner.Editor.Models
{
    public struct FileStatistics
    {
        public string FilePath { get; set; }
        public long SizeBytes { get; set; }
        public int LineCount { get; set; }
        public int ClassCount { get; set; }
        public int MethodCount { get; set; }
        public int CommentCount { get; set; }
        public int CodeLines { get; set; }
        public int BlankLines { get; set; }
        public int CommentLines { get; set; }
    }
}