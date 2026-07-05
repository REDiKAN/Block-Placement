using UnityEditor;
using UnityEngine;

namespace ScriptCombiner.Editor.UI.UITabs
{
    public class ConfigTab
    {
        private readonly ScriptCombinerWindow window;
        private Vector2 scrollPosition;

        public ConfigTab(ScriptCombinerWindow window)
        {
            this.window = window;
        }

        public void Render()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Space(5);

            RenderSettingsSection();
            GUILayout.Space(10);

            RenderSelectionSection();
            GUILayout.Space(10);

            RenderOutputSection();
            GUILayout.Space(10);

            EditorGUILayout.EndScrollView();
        }

        private void RenderSettingsSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            UIStyles.RenderSectionHeader("⚙️ Generation Settings");

            GUILayout.Space(5);
            RenderEncodingSelection();
            EditorGUILayout.Space(5);
            RenderAdvancedOptions();
            EditorGUILayout.EndVertical();
        }

        private void RenderSelectionSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            UIStyles.RenderSectionHeader("📂 File Selection");

            RenderSelectedPaths();
            GUILayout.Space(5);
            RenderActionButtons();
            EditorGUILayout.EndVertical();
        }

        private void RenderOutputSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            UIStyles.RenderSectionHeader("📊 Statistics & Output");

            RenderStatistics();
            GUILayout.Space(5);
            RenderOutputButtons();
            EditorGUILayout.EndVertical();
        }

        private void RenderEncodingSelection()
        {
            GUILayout.Label("Target Encoding:", EditorStyles.miniLabel);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("UTF-8", GUILayout.Height(Constants.ScriptCombinerConstants.ButtonHeight)))
                window.SetSelectedEncoding(System.Text.Encoding.UTF8);
            if (GUILayout.Button("ANSI", GUILayout.Height(Constants.ScriptCombinerConstants.ButtonHeight)))
                window.SetSelectedEncoding(System.Text.Encoding.Default);
            if (GUILayout.Button("Win-1251", GUILayout.Height(Constants.ScriptCombinerConstants.ButtonHeight)))
                window.SetSelectedEncoding(System.Text.Encoding.GetEncoding(1251));
            GUILayout.EndHorizontal();
        }

        private void RenderAdvancedOptions()
        {
            window.SetConsolidateUsings(EditorGUILayout.Toggle(
                new GUIContent("Consolidate Usings", "Group all 'using' statements at the top and remove duplicates"),
                window.GetConsolidateUsings()));

            window.SetEnableExclusions(EditorGUILayout.Toggle(
                new GUIContent("Enable Exclusions", "Ignore files matching patterns"),
                window.GetEnableExclusions()));

            EditorGUI.BeginDisabledGroup(!window.GetEnableExclusions());
            window.SetExclusionPatterns(EditorGUILayout.TextField("Exclude Patterns (comma sep):", window.GetExclusionPatterns()));
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(5);

            window.SetCleanupCode(EditorGUILayout.Foldout(window.GetCleanupCode(), "Code Cleanup Options", true));
            if (window.GetCleanupCode())
            {
                EditorGUI.indentLevel++;
                window.SetRemoveComments(EditorGUILayout.Toggle("Remove Comments", window.GetRemoveComments()));
                window.SetRemoveEmptyLines(EditorGUILayout.Toggle("Remove Empty Lines", window.GetRemoveEmptyLines()));
                window.SetRemoveRegions(EditorGUILayout.Toggle("Remove Regions", window.GetRemoveRegions()));
                EditorGUI.indentLevel--;
            }

            window.SetDetailedStats(EditorGUILayout.Toggle(
                new GUIContent("Detailed Statistics", "Count code, blank, and comment lines separately"),
                window.GetDetailedStats()));
        }

        private void RenderSelectedPaths()
        {
            Rect dropArea = GUILayoutUtility.GetRect(0.0f, Constants.ScriptCombinerConstants.DropAreaHeight, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "Drag & Drop Files/Folders Here", EditorStyles.centeredGreyMiniLabel);
            HandleDragAndDrop(dropArea);

            var listRect = GUILayoutUtility.GetRect(0.0f, Constants.ScriptCombinerConstants.ListHeight, GUILayout.ExpandWidth(true));
            GUI.Box(listRect, "");

            var listScroll = EditorGUILayout.BeginScrollView(Vector2.zero, GUILayout.Height(Constants.ScriptCombinerConstants.ListHeight));
            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            EditorGUILayout.BeginVertical();

            var selectedPaths = window.GetSelectedPaths();
            int removeIndex = -1;

            for (int i = 0; i < selectedPaths.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(Utilities.FileHelper.DirectoryExists(Utilities.PathHelper.ConvertToFullPath(selectedPaths[i])) ? "📁 " : "📄 ", GUILayout.Width(20));
                EditorGUILayout.LabelField(selectedPaths[i], EditorStyles.miniLabel, GUILayout.ExpandWidth(true));

                if (GUILayout.Button("✖", GUILayout.Width(20)))
                {
                    removeIndex = i;
                }
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            GUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();

            if (removeIndex != -1)
            {
                window.RemovePath(removeIndex);
            }
        }

        private void RenderActionButtons()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Selected", GUILayout.Height(Constants.ScriptCombinerConstants.ButtonHeight)))
            {
                window.AddSelectedInProject();
            }
            if (GUILayout.Button("Add Folder", GUILayout.Height(Constants.ScriptCombinerConstants.ButtonHeight)))
            {
                window.TriggerAddFolderDialog();
            }
            if (GUILayout.Button("Clear", GUILayout.Height(Constants.ScriptCombinerConstants.ButtonHeight)))
            {
                window.ClearAll();
            }
            GUILayout.EndHorizontal();
        }

        private void RenderStatistics()
        {
            var statistics = window.GetStatistics();
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical(GUILayout.Width(150));
            EditorGUILayout.LabelField("Files: " + statistics.TotalFiles, EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField("Size: " + statistics.TotalSizeKB.ToString("F2") + " KB", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            if (window.GetDetailedStats())
            {
                EditorGUILayout.LabelField($"Code: {statistics.CodeLines} | Comment: {statistics.CommentLines} | Empty: {statistics.BlankLines}", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"Classes: {statistics.TotalClasses} | Methods: {statistics.TotalMethods}", EditorStyles.miniLabel);
            }
            else
            {
                EditorGUILayout.LabelField($"Lines: {statistics.TotalLines}", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"Classes: {statistics.TotalClasses} | Methods: {statistics.TotalMethods}", EditorStyles.miniLabel);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        private void RenderOutputButtons()
        {
            EditorGUI.BeginDisabledGroup(window.GetSelectedPaths().Count == 0);

            if (GUILayout.Button("Save Combined Scripts To File...", GUILayout.Height(Constants.ScriptCombinerConstants.LargeButtonHeight)))
            {
                window.TriggerSaveDialog();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("Copy to Clipboard", GUILayout.Height(Constants.ScriptCombinerConstants.LargeButtonHeight)))
            {
                window.CopyCombinedScripts();
            }

            EditorGUI.EndDisabledGroup();
        }

        private void HandleDragAndDrop(Rect dropArea)
        {
            Event evt = Event.current;
            if (evt.type == EventType.DragUpdated)
            {
                if (dropArea.Contains(evt.mousePosition))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    evt.Use();
                }
            }
            else if (evt.type == EventType.DragPerform)
            {
                if (dropArea.Contains(evt.mousePosition))
                {
                    DragAndDrop.AcceptDrag();
                    bool added = false;

                    foreach (UnityEngine.Object obj in DragAndDrop.objectReferences)
                    {
                        string path = UnityEditor.AssetDatabase.GetAssetPath(obj);
                        if (!string.IsNullOrEmpty(path) && !window.GetSelectedPaths().Contains(path))
                        {
                            if (Utilities.FileHelper.DirectoryExists(path) || Utilities.PathHelper.IsCSharpFile(path))
                            {
                                window.AddPath(path);
                                added = true;
                            }
                        }
                    }

                    foreach (string path in DragAndDrop.paths)
                    {
                        if (!string.IsNullOrEmpty(path) && !window.GetSelectedPaths().Contains(path))
                        {
                            string relativePath = Utilities.PathHelper.ConvertToRelativePath(path);
                            if (!string.IsNullOrEmpty(relativePath))
                                window.AddPath(relativePath);
                            else
                                window.AddPath(path);
                            added = true;
                        }
                    }

                    if (added) window.UpdateStatistics();
                    evt.Use();
                }
            }
        }
    }
}