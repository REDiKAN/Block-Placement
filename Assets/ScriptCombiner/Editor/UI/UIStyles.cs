using UnityEditor;
using UnityEngine;

namespace ScriptCombiner.Editor.UI
{
    public static class UIStyles
    {
        public static void RenderSectionHeader(string title)
        {
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            Rect rect = GUILayoutUtility.GetLastRect();
            rect.y += rect.height - 2;
            rect.height = 1;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
        }
    }
}