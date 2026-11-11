#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Game.Data.Ranked;

namespace Game.Editor.Tool
{
    public static class RankedMmrResetter
    {
        private const string DefaultBlocksKey = "ranked_mmr";
        private const string DefaultStructuresKey = "ranked_mmr_structures";
        private const int DefaultStartingMmr = 1000;

        [MenuItem("Tools/Reset All Ranked MMR")]
        public static void ResetAllMmr()
        {
            var (blocksConfig, structuresConfig) = FindConfigs();

            var blocksStart = blocksConfig is not null ? blocksConfig.StartingMmr : DefaultStartingMmr;
            var structsStart = structuresConfig is not null ? structuresConfig.StartingMmr : DefaultStartingMmr;

            var blocksKey = blocksConfig is not null && !string.IsNullOrEmpty(blocksConfig.MmrPlayerPrefsKey)
                ? blocksConfig.MmrPlayerPrefsKey
                : DefaultBlocksKey;

            var structsKey = structuresConfig is not null && !string.IsNullOrEmpty(structuresConfig.MmrPlayerPrefsKey)
                ? structuresConfig.MmrPlayerPrefsKey
                : DefaultStructuresKey;

            PlayerPrefs.SetInt(blocksKey, blocksStart);
            PlayerPrefs.SetInt(structsKey, structsStart);
            PlayerPrefs.Save();

            Debug.Log($"[RankedMmrResetter] Blocks MMR reset to {blocksStart} (key: '{blocksKey}').");
            Debug.Log($"[RankedMmrResetter] Structures MMR reset to {structsStart} (key: '{structsKey}').");
        }

        [MenuItem("Tools/Delete All Ranked MMR")]
        public static void DeleteAllMmr()
        {
            var (blocksConfig, structuresConfig) = FindConfigs();

            var blocksKey = blocksConfig is not null && !string.IsNullOrEmpty(blocksConfig.MmrPlayerPrefsKey)
                ? blocksConfig.MmrPlayerPrefsKey
                : DefaultBlocksKey;

            var structsKey = structuresConfig is not null && !string.IsNullOrEmpty(structuresConfig.MmrPlayerPrefsKey)
                ? structuresConfig.MmrPlayerPrefsKey
                : DefaultStructuresKey;

            if (PlayerPrefs.HasKey(blocksKey)) PlayerPrefs.DeleteKey(blocksKey);
            if (PlayerPrefs.HasKey(structsKey)) PlayerPrefs.DeleteKey(structsKey);

            PlayerPrefs.Save();
            Debug.Log($"[RankedMmrResetter] Deleted keys '{blocksKey}' and '{structsKey}'.");
        }

        [MenuItem("Tools/Show Current Ranked MMR")]
        public static void ShowCurrentMmr()
        {
            var (blocksConfig, structuresConfig) = FindConfigs();

            var blocksStart = blocksConfig is not null ? blocksConfig.StartingMmr : DefaultStartingMmr;
            var structsStart = structuresConfig is not null ? structuresConfig.StartingMmr : DefaultStartingMmr;

            var blocksKey = blocksConfig is not null && !string.IsNullOrEmpty(blocksConfig.MmrPlayerPrefsKey)
                ? blocksConfig.MmrPlayerPrefsKey
                : DefaultBlocksKey;

            var structsKey = structuresConfig is not null && !string.IsNullOrEmpty(structuresConfig.MmrPlayerPrefsKey)
                ? structuresConfig.MmrPlayerPrefsKey
                : DefaultStructuresKey;

            var currentBlocks = PlayerPrefs.GetInt(blocksKey, blocksStart);
            var currentStructs = PlayerPrefs.GetInt(structsKey, structsStart);

            Debug.Log($"[RankedMmrResetter] Blocks MMR: {currentBlocks} (key: '{blocksKey}').");
            Debug.Log($"[RankedMmrResetter] Structures MMR: {currentStructs} (key: '{structsKey}').");
        }

        private static (RankedBlocksConfig, RankedStructuresConfig) FindConfigs()
        {
            var blocksGuids = AssetDatabase.FindAssets($"t:{nameof(RankedBlocksConfig)}");
            var structsGuids = AssetDatabase.FindAssets($"t:{nameof(RankedStructuresConfig)}");

            RankedBlocksConfig blocksConfig = null;
            RankedStructuresConfig structsConfig = null;

            if (blocksGuids is { Length: > 0 })
            {
                var path = AssetDatabase.GUIDToAssetPath(blocksGuids[0]);
                blocksConfig = AssetDatabase.LoadAssetAtPath<RankedBlocksConfig>(path);
            }

            if (structsGuids is { Length: > 0 })
            {
                var path = AssetDatabase.GUIDToAssetPath(structsGuids[0]);
                structsConfig = AssetDatabase.LoadAssetAtPath<RankedStructuresConfig>(path);
            }

            return (blocksConfig, structsConfig);
        }
    }
}
#endif