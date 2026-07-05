using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ScriptCombiner.Editor.Constants;
using ScriptCombiner.Editor.Core;
using ScriptCombiner.Editor.Models;
using ScriptCombiner.Editor.UI.UITabs;
using ScriptCombiner.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace ScriptCombiner.Editor.UI
{
    public class ScriptCombinerWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private List<string> selectedPaths = new List<string>();
        private Encoding selectedEncoding = Encoding.UTF8;
        private ScriptStatistics statistics = new ScriptStatistics();
        private string previewContent = "";
        private int selectedTab = 0;
        private string[] tabTitles = { "Configuration", "Preview" };

        private bool consolidateUsings = true;
        private bool enableExclusions = false;
        private string exclusionPatterns = ScriptCombinerConstants.DefaultExclusionPatterns;
        private bool cleanupCode = false;
        private bool removeComments = false;
        private bool removeEmptyLines = false;
        private bool removeRegions = false;
        private bool detailedStats = false;

        private bool triggerSaveDialog = false;
        private bool triggerAddFolderDialog = false;

        private IScriptProcessor processor;
        private ConfigTab configTab;
        private PreviewTab previewTab;

        [MenuItem(ScriptCombinerConstants.MenuItemPath)]
        public static void ShowWindow()
        {
            var window = GetWindow<ScriptCombinerWindow>(ScriptCombinerConstants.WindowTitle);
            window.minSize = new Vector2(ScriptCombinerConstants.MinWindowWidth, ScriptCombinerConstants.MinWindowHeight);
        }

        private void OnEnable()
        {
            processor = new ScriptProcessor();
            configTab = new ConfigTab(this);
            previewTab = new PreviewTab(this);
        }

        private void OnGUI()
        {
            selectedTab = GUILayout.Toolbar(selectedTab, tabTitles);

            if (selectedTab == 0)
            {
                configTab.Render();
            }
            else
            {
                previewTab.Render();
            }

            HandleDeferredActions();
        }

        private void HandleDeferredActions()
        {
            if (triggerSaveDialog)
            {
                triggerSaveDialog = false;
                SaveCombinedScripts();
            }

            if (triggerAddFolderDialog)
            {
                triggerAddFolderDialog = false;
                AddFolder();
            }
        }

        public void TriggerSaveDialog() => triggerSaveDialog = true;
        public void TriggerAddFolderDialog() => triggerAddFolderDialog = true;

        public IScriptProcessor GetProcessor() => processor;
        public ScriptStatistics GetStatistics() => statistics;
        public List<string> GetSelectedPaths() => selectedPaths;
        public string GetPreviewContent() => previewContent;
        public Encoding GetSelectedEncoding() => selectedEncoding;

        public void SetPreviewContent(string content) => previewContent = content;
        public void SetSelectedEncoding(Encoding encoding) => selectedEncoding = encoding;

        public ProcessorOptions GetProcessorOptions()
        {
            return new ProcessorOptions
            {
                ConsolidateUsings = consolidateUsings,
                RemoveComments = removeComments && cleanupCode,
                RemoveEmptyLines = removeEmptyLines && cleanupCode,
                RemoveRegions = removeRegions && cleanupCode,
                IsDetailed = detailedStats,
                ExclusionCheck = IsExcluded
            };
        }

        public bool IsExcluded(string filePath)
        {
            if (!enableExclusions || string.IsNullOrEmpty(exclusionPatterns)) return false;

            string fileName = Path.GetFileName(filePath);
            var patterns = exclusionPatterns.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var pattern in patterns)
            {
                if (fileName.Contains(pattern.Trim()))
                    return true;
            }
            return false;
        }

        public void UpdateStatistics()
        {
            statistics.Clear();

            foreach (string path in selectedPaths)
            {
                string fullPath = PathHelper.ConvertToFullPath(path);

                if (FileHelper.DirectoryExists(fullPath))
                {
                    foreach (string file in Directory.GetFiles(fullPath, $"*{ScriptCombinerConstants.FileExtensionCSharp}", SearchOption.AllDirectories))
                    {
                        if (IsExcluded(file)) continue;
                        statistics.Add(processor.ProcessFile(file, detailedStats));
                    }
                }
                else if (FileHelper.FileExists(fullPath) && PathHelper.IsCSharpFile(fullPath))
                {
                    if (IsExcluded(fullPath)) continue;
                    statistics.Add(processor.ProcessFile(fullPath, detailedStats));
                }
            }

            Repaint();
        }

        public void AddSelectedInProject()
        {
            bool added = false;
            foreach (UnityEngine.Object obj in Selection.objects)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path) &&
                    (FileHelper.DirectoryExists(path) || PathHelper.IsCSharpFile(path)))
                {
                    if (!selectedPaths.Contains(path))
                    {
                        selectedPaths.Add(path);
                        added = true;
                    }
                }
            }

            if (added) UpdateStatistics();
        }

        private void AddFolder()
        {
            string folder = EditorUtility.OpenFolderPanel("Select Folder", Application.dataPath, "");
            if (!string.IsNullOrEmpty(folder))
            {
                string relativePath = folder.StartsWith(Application.dataPath) ?
                    "Assets" + folder.Substring(Application.dataPath.Length) : folder;

                if (!selectedPaths.Contains(relativePath))
                {
                    selectedPaths.Add(relativePath);
                    UpdateStatistics();
                }
            }
        }

        private void SaveCombinedScripts()
        {
            if (selectedPaths.Count == 0)
            {
                EditorUtility.DisplayDialog("Info", "Please select files or folders first", "OK");
                return;
            }

            GeneratePreviewContent();

            string directory = Application.dataPath;
            if (selectedPaths.Count == 1 && !File.Exists(selectedPaths[0]) && FileHelper.DirectoryExists(PathHelper.ConvertToFullPath(selectedPaths[0])))
            {
                directory = PathHelper.ConvertToFullPath(selectedPaths[0]);
            }

            string fileName = $"CombinedScripts_{selectedEncoding.EncodingName}.{ScriptCombinerConstants.FileExtensionOutput}";
            string savePath = EditorUtility.SaveFilePanel("Save Combined Scripts", directory, fileName, ScriptCombinerConstants.FileExtensionOutput);

            if (!string.IsNullOrEmpty(savePath))
            {
                try
                {
                    File.WriteAllText(savePath, previewContent, selectedEncoding);
                    EditorUtility.RevealInFinder(savePath);
                    EditorUtility.DisplayDialog("Success", "Scripts combined successfully!", "OK");
                }
                catch (Exception e)
                {
                    Debug.LogError($"{ScriptCombinerConstants.LogPrefix} Error writing combined file: {e.Message}");
                    EditorUtility.DisplayDialog("Error", $"Error writing file: {e.Message}", "OK");
                }
            }
        }

        public void GeneratePreviewContent()
        {
            if (selectedPaths.Count == 0)
            {
                previewContent = "// No files selected.";
                return;
            }

            var options = GetProcessorOptions();
            previewContent = processor.GenerateCombinedText(selectedPaths, statistics, selectedEncoding, options);
        }

        public void CopyCombinedScripts()
        {
            if (selectedPaths.Count == 0)
            {
                EditorUtility.DisplayDialog("Info", "Please select files or folders first", "OK");
                return;
            }

            GeneratePreviewContent();
            GUIUtility.systemCopyBuffer = previewContent;
            Debug.Log($"{ScriptCombinerConstants.LogPrefix} Combined {statistics.TotalFiles} scripts copied to clipboard.");
        }

        public void ClearAll()
        {
            selectedPaths.Clear();
            statistics.Clear();
            previewContent = "";
        }

        public void RemovePath(int index)
        {
            if (index >= 0 && index < selectedPaths.Count)
            {
                selectedPaths.RemoveAt(index);
                UpdateStatistics();
            }
        }

        public void AddPath(string path)
        {
            if (!selectedPaths.Contains(path))
            {
                selectedPaths.Add(path);
                UpdateStatistics();
            }
        }

        public bool GetConsolidateUsings() => consolidateUsings;
        public void SetConsolidateUsings(bool value) => consolidateUsings = value;
        public bool GetEnableExclusions() => enableExclusions;
        public void SetEnableExclusions(bool value) => enableExclusions = value;
        public string GetExclusionPatterns() => exclusionPatterns;
        public void SetExclusionPatterns(string value) => exclusionPatterns = value;
        public bool GetCleanupCode() => cleanupCode;
        public void SetCleanupCode(bool value) => cleanupCode = value;
        public bool GetRemoveComments() => removeComments;
        public void SetRemoveComments(bool value) => removeComments = value;
        public bool GetRemoveEmptyLines() => removeEmptyLines;
        public void SetRemoveEmptyLines(bool value) => removeEmptyLines = value;
        public bool GetRemoveRegions() => removeRegions;
        public void SetRemoveRegions(bool value) => removeRegions = value;
        public bool GetDetailedStats() => detailedStats;
        public void SetDetailedStats(bool value) => detailedStats = value;
    }
}