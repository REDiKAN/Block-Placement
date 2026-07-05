using UnityEditor;
using UnityEngine;

namespace ScriptCombiner.Editor.UI.UITabs
{
    public class PreviewTab
    {
        private readonly ScriptCombinerWindow window;
        private Vector2 previewScrollPosition;

        public PreviewTab(ScriptCombinerWindow window)
        {
            this.window = window;
        }

        public void Render()
        {
            EditorGUILayout.HelpBox("Preview of the generated text. Remember to click 'Regenerate' if you changed settings.", MessageType.Info);
            GUILayout.Space(5);

            if (GUILayout.Button("Regenerate Preview", GUILayout.Height(30)))
            {
                window.GeneratePreviewContent();
            }

            GUILayout.Space(5);

            previewScrollPosition = EditorGUILayout.BeginScrollView(previewScrollPosition, GUILayout.ExpandHeight(true));
            EditorGUI.BeginDisabledGroup(true);

            var textStyle = new GUIStyle(EditorStyles.textArea)
            {
                fontSize = 12,
                font = EditorStyles.label.font
            };
            EditorGUILayout.TextArea(window.GetPreviewContent(), textStyle, GUILayout.ExpandHeight(true));

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndScrollView();
        }
    }
}