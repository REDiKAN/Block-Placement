using System;

namespace ScriptCombiner.Editor.Models
{
    [Serializable]
    public class ProcessorOptions
    {
        public bool ConsolidateUsings { get; set; }
        public bool RemoveComments { get; set; }
        public bool RemoveEmptyLines { get; set; }
        public bool RemoveRegions { get; set; }
        public bool IsDetailed { get; set; }
        public Func<string, bool> ExclusionCheck { get; set; }

        public ProcessorOptions()
        {
            ConsolidateUsings = true;
            RemoveComments = false;
            RemoveEmptyLines = false;
            RemoveRegions = false;
            IsDetailed = false;
            ExclusionCheck = path => false;
        }
    }
}