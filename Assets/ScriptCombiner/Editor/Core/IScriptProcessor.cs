using System.Collections.Generic;
using System.Text;
using ScriptCombiner.Editor.Models;

namespace ScriptCombiner.Editor.Core
{
    public interface IScriptProcessor
    {
        FileStatistics ProcessFile(string filePath, bool detailed);
        string GenerateCombinedText(List<string> paths, ScriptStatistics statistics, Encoding encoding, ProcessorOptions options);
    }
}