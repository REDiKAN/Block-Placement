using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ScriptCombiner.Editor.Constants;
using ScriptCombiner.Editor.Models;
using ScriptCombiner.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace ScriptCombiner.Editor.Core
{
    public class ScriptProcessor : IScriptProcessor
    {
        public FileStatistics ProcessFile(string filePath, bool detailed)
        {
            try
            {
                if (!FileHelper.FileExists(filePath))
                {
                    Debug.LogWarning($"{ScriptCombinerConstants.LogPrefix} File not found: {filePath}");
                    return new FileStatistics();
                }

                string content = FileReader.ReadFileWithAutoEncoding(filePath);
                FileInfo fileInfo = new FileInfo(filePath);

                var stats = new FileStatistics
                {
                    FilePath = filePath,
                    SizeBytes = fileInfo.Length,
                    LineCount = content.Split('\n').Length,
                    ClassCount = CountOccurrences(content, "class "),
                    MethodCount = CountMethods(content),
                    CommentCount = CountComments(content)
                };

                if (detailed)
                {
                    AnalyzeLines(content, out int code, out int blank, out int comments);
                    stats.CodeLines = code;
                    stats.BlankLines = blank;
                    stats.CommentLines = comments;
                }

                return stats;
            }
            catch (Exception e)
            {
                Debug.LogError($"{ScriptCombinerConstants.LogPrefix} Error processing file {filePath}: {e.Message}");
                return new FileStatistics();
            }
        }

        public string GenerateCombinedText(List<string> paths, ScriptStatistics statistics, Encoding encoding, ProcessorOptions options)
        {
            var allScriptPaths = CollectScriptPaths(paths, options.ExclusionCheck);

            if (allScriptPaths.Count == 0)
            {
                return "// No .cs files found to combine (or all were excluded).";
            }

            HashSet<string> allUsings = new HashSet<string>();
            List<FileContent> processedFiles = new List<FileContent>();

            foreach (string scriptPath in allScriptPaths)
            {
                try
                {
                    string content = FileReader.ReadFileWithAutoEncoding(scriptPath);
                    content = ApplyProcessingOptions(content, options);
                    processedFiles.Add(new FileContent { Path = scriptPath, Content = content });
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"{ScriptCombinerConstants.LogPrefix} Error reading file {scriptPath}: {e.Message}");
                }
            }

            return BuildCombinedText(processedFiles, statistics, encoding, options.ConsolidateUsings, allUsings);
        }

        private string ApplyProcessingOptions(string content, ProcessorOptions options)
        {
            if (options.RemoveComments) content = RemoveComments(content);
            if (options.RemoveRegions) content = RemoveRegions(content);
            if (options.RemoveEmptyLines)
            {
                content = Regex.Replace(content, @"^\s*$[\r\n]*", string.Empty, RegexOptions.Multiline);
            }

            return content;
        }

        private string BuildCombinedText(List<FileContent> files, ScriptStatistics stats, Encoding encoding, bool consolidateUsings, HashSet<string> usings)
        {
            StringBuilder combinedText = new StringBuilder();

            AppendHeader(combinedText, files.Count, encoding);

            if (consolidateUsings && usings.Count > 0)
            {
                AppendConsolidatedUsings(combinedText, usings);
            }

            AppendFileContents(combinedText, files);
            AppendStatistics(combinedText, stats);

            return combinedText.ToString();
        }

        private void AppendHeader(StringBuilder sb, int fileCount, Encoding encoding)
        {
            sb.AppendLine(ScriptCombinerConstants.HeaderTemplate);
            sb.AppendLine($"// Generation Time: {DateTime.Now}");
            sb.AppendLine($"// Encoding: {encoding.EncodingName}");
            sb.AppendLine($"// Total Files: {fileCount}");
            sb.AppendLine(ScriptCombinerConstants.FooterTemplate);
            sb.AppendLine();
        }

        private void AppendConsolidatedUsings(StringBuilder sb, HashSet<string> usings)
        {
            sb.AppendLine(ScriptCombinerConstants.UsingsHeader);
            var sortedUsings = usings.OrderBy(u => u).ToList();
            foreach (var u in sortedUsings)
            {
                sb.AppendLine(u);
            }
            sb.AppendLine(ScriptCombinerConstants.FooterTemplate);
            sb.AppendLine();
        }

        private void AppendFileContents(StringBuilder sb, List<FileContent> files)
        {
            for (int i = 0; i < files.Count; i++)
            {
                sb.AppendLine($"//==== File {i + 1} of {files.Count}: {files[i].Path} ====");
                sb.AppendLine(files[i].Content);
                sb.AppendLine();
            }
        }

        private void AppendStatistics(StringBuilder sb, ScriptStatistics stats)
        {
            sb.AppendLine();
            sb.AppendLine("// ============ Statistics =============");
            sb.AppendLine($"// Total Files: {stats.TotalFiles}");
            sb.AppendLine($"// Total Size: {stats.TotalSizeKB:F2} KB");

            if (stats.CodeLines > 0)
            {
                sb.AppendLine($"// Code Lines: {stats.CodeLines}");
                sb.AppendLine($"// Comment Lines: {stats.CommentLines}");
                sb.AppendLine($"// Blank Lines: {stats.BlankLines}");
            }
            else
            {
                sb.AppendLine($"// Total Lines: {stats.TotalLines}");
            }

            sb.AppendLine($"// Classes: {stats.TotalClasses}");
            sb.AppendLine($"// Methods: {stats.TotalMethods}");
            sb.AppendLine($"// Comments (Blocks): {stats.TotalComments}");
            sb.AppendLine("// =====================================");
        }

        private class FileContent
        {
            public string Path { get; set; }
            public string Content { get; set; }
        }

        private string RemoveComments(string content)
        {
            content = Regex.Replace(content, @"//.*$", "", RegexOptions.Multiline);
            content = Regex.Replace(content, @"/\*[\s\S]*?\*/", "", RegexOptions.Multiline);
            return content;
        }

        private string RemoveRegions(string content)
        {
            content = Regex.Replace(content, @"#region\s.*", "", RegexOptions.Multiline);
            content = Regex.Replace(content, @"#endregion", "", RegexOptions.Multiline);
            return content;
        }

        private void AnalyzeLines(string content, out int code, out int blank, out int comments)
        {
            code = 0;
            blank = 0;
            comments = 0;
            var lines = content.Split('\n');
            bool inBlockComment = false;

            foreach (var line in lines)
            {
                string trimmed = line.Trim();

                if (string.IsNullOrEmpty(trimmed))
                {
                    blank++;
                    continue;
                }

                if (trimmed.Contains("/*")) inBlockComment = true;
                if (trimmed.Contains("*/")) inBlockComment = false;

                if (inBlockComment || trimmed.StartsWith("//"))
                {
                    comments++;
                    continue;
                }

                code++;
            }
        }

        private int CountOccurrences(string text, string pattern)
        {
            int count = 0;
            int index = 0;
            while ((index = text.IndexOf(pattern, index, StringComparison.Ordinal)) != -1)
            {
                index += pattern.Length;
                count++;
            }
            return count;
        }

        private int CountMethods(string content)
        {
            return content.Split('\n')
                .Count(line => line.Trim().Contains("(") &&
                              line.Trim().Contains(")") &&
                              IsMethodSignature(line.Trim()));
        }

        private bool IsMethodSignature(string line)
        {
            string[] returnTypes = { "void", "int", "string", "float", "bool", "double", "object" };
            string[] excludedKeywords = { "class", "struct", "interface", "enum" };

            return returnTypes.Any(rt => line.Contains(rt)) &&
                   !excludedKeywords.Any(ek => line.Contains(ek));
        }

        private int CountComments(string content)
        {
            return content.Split('\n')
                .Count(line => line.Trim().StartsWith("//") ||
                              line.Trim().Contains("/*") ||
                              line.Trim().Contains("*/"));
        }

        private List<string> CollectScriptPaths(List<string> paths, Func<string, bool> exclusionCheck)
        {
            var allScriptPaths = new List<string>();

            foreach (string path in paths)
            {
                string fullPath = PathHelper.ConvertToFullPath(path);

                if (Directory.Exists(fullPath))
                {
                    allScriptPaths.AddRange(Directory.GetFiles(fullPath, $"*{ScriptCombinerConstants.FileExtensionCSharp}", SearchOption.AllDirectories)
                        .Where(p => !exclusionCheck(p)));
                }
                else if (FileHelper.FileExists(fullPath) && PathHelper.IsCSharpFile(fullPath))
                {
                    if (!exclusionCheck(fullPath)) allScriptPaths.Add(fullPath);
                }
            }

            return allScriptPaths.Distinct().ToList();
        }
    }
}