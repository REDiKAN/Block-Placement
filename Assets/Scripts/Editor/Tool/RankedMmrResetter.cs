#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Game.Data.Ranked;

namespace Game.Editor.Tool
{
    public static class RankedMmrResetter
    {
        private const string DefaultMmrKey = "ranked_mmr";
        private const int DefaultStartingMmr = 1000;

        [MenuItem("Tools/Reset Ranked MMR")]
        public static void ResetMmr()
        {
            var startingMmr = ResolveStartingMmr();
            var mmrKey = ResolveMmrKey();

            PlayerPrefs.SetInt(mmrKey, startingMmr);
            PlayerPrefs.Save();

            Debug.Log($"[RankedMmrResetter] MMR reset to {startingMmr} (key: '{mmrKey}').");
        }

        [MenuItem("Tools/Delete Ranked MMR")]
        public static void DeleteMmr()
        {
            var mmrKey = ResolveMmrKey();

            if (!PlayerPrefs.HasKey(mmrKey))
            {
                Debug.LogWarning($"[RankedMmrResetter] Key '{mmrKey}' not found in PlayerPrefs.");
                return;
            }

            PlayerPrefs.DeleteKey(mmrKey);
            PlayerPrefs.Save();

            Debug.Log($"[RankedMmrResetter] MMR key '{mmrKey}' deleted from PlayerPrefs.");
        }

        [MenuItem("Tools/Show Current Ranked MMR")]
        public static void ShowCurrentMmr()
        {
            var startingMmr = ResolveStartingMmr();
            var mmrKey = ResolveMmrKey();
            var currentMmr = PlayerPrefs.GetInt(mmrKey, startingMmr);

            Debug.Log($"[RankedMmrResetter] Current MMR: {currentMmr} (key: '{mmrKey}', default: {startingMmr}).");
        }

        private static int ResolveStartingMmr()
        {
            var config = FindRankedConfig();
            return config is not null ? config.StartingMmr : DefaultStartingMmr;
        }

        private static string ResolveMmrKey()
        {
            var config = FindRankedConfig();
            return config is not null && !string.IsNullOrEmpty(config.MmrPlayerPrefsKey)
                ? config.MmrPlayerPrefsKey
                : DefaultMmrKey;
        }

        private static RankedConfig FindRankedConfig()
        {
            var guids = AssetDatabase.FindAssets($"t:{nameof(RankedConfig)}");
            if (guids is null || guids.Length == 0) return null;

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<RankedConfig>(path);
        }
    }
}
#endif