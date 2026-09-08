#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Game.Data;

namespace Game.Editor.Tool
{
    public static class AchievementResetter
    {
        [MenuItem("Tools/Reset All Achievements")]
        public static void ResetAchievements()
        {
            var guids = AssetDatabase.FindAssets("t:AchievementConfig");
            var count = 0;

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var config = AssetDatabase.LoadAssetAtPath<AchievementConfig>(path);

                if (config is not null && !string.IsNullOrEmpty(config.Id))
                {
                    PlayerPrefs.DeleteKey($"achievement_{config.Id}_progress");
                    PlayerPrefs.DeleteKey($"achievement_{config.Id}_completed");
                    count++;
                }
            }

            PlayerPrefs.Save();
            Debug.Log($"[AchievementResetter] Cleared PlayerPrefs for {count} achievements.");
        }
    }
}
#endif