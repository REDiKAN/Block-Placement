using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Game.Data;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.Tool
{
    public class LevelImporterWindow : EditorWindow
    {
        [SerializeField] private string _jsonInput = string.Empty;
        [SerializeField] private string _assetName = "NewLevel";
        [SerializeField] private string _savePath = "Assets/Data/Levels";
        [SerializeField] private bool _autoFixShadows = true;
        [SerializeField] private List<StructurePromptItem> _structuresForPrompt = new();
        private Vector2 _scrollPosition;

        [MenuItem("Tools/Level Importer")]
        public static void ShowWindow() => GetWindow<LevelImporterWindow>("Level Importer");

        private void OnGUI()
        {
            DrawImportSection();
            DrawPromptGeneratorSection();
        }

        private void DrawImportSection()
        {
            EditorGUILayout.LabelField("Level Importer", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            _jsonInput = EditorGUILayout.TextArea(_jsonInput, GUILayout.Height(300));
            _assetName = EditorGUILayout.TextField("Asset Name", _assetName);
            _savePath = EditorGUILayout.TextField("Save Path", _savePath);
            _autoFixShadows = EditorGUILayout.Toggle("Auto-Fix Shadows", _autoFixShadows);
            if (GUILayout.Button("Create Level"))
                ProcessImport();
        }

        private void DrawPromptGeneratorSection()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("AI Prompt Generator", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Structures added: {_structuresForPrompt.Count}", EditorStyles.miniLabel);

            if (GUILayout.Button("Add Structure"))
                _structuresForPrompt.Add(new StructurePromptItem());

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.MinHeight(100), GUILayout.MaxHeight(250));
            for (var i = 0; i < _structuresForPrompt.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                _structuresForPrompt[i].Config = (StructureConfig)EditorGUILayout.ObjectField(
                    _structuresForPrompt[i].Config, typeof(StructureConfig), false);
                _structuresForPrompt[i].MaxCount = EditorGUILayout.IntField(
                    _structuresForPrompt[i].MaxCount, GUILayout.Width(50));
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    _structuresForPrompt.RemoveAt(i);
                    i--;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            if (GUILayout.Button("Generate AI Prompt", GUILayout.Height(30)))
                GenerateAIPrompt();
        }

        private void GenerateAIPrompt()
        {
            var validStructures = _structuresForPrompt.Where(s => s.Config is not null).ToArray();
            var sb = new StringBuilder();

            sb.AppendLine("You are a level designer AI. Generate a valid JSON for a 3D shadow puzzle level.");
            sb.AppendLine("The grid size is 5x5x5 (X, Y, Z from 0 to 4).");
            sb.AppendLine("FloorMatrix is a 1D array of 25 booleans (X * 5 + Z).");
            sb.AppendLine("Walls are 1D arrays of 25 items (WallYZ: Y * 5 + Z, WallXY: X * 5 + Y).");
            sb.AppendLine();

            if (validStructures.Length > 0)
            {
                sb.AppendLine("Available structures and their local coordinates:");
                sb.AppendLine(StructurePromptBuilder.Build(validStructures));
                sb.AppendLine();
            }

            sb.AppendLine("JSON Schema:");
            sb.AppendLine("{");
            sb.AppendLine("  \"InitialBlocks\": [ {\"x\": 0, \"y\": 0, \"z\": 0} ],");
            sb.AppendLine("  \"FloorMatrix\": [ true, true, ... ],");
            sb.AppendLine("  \"WallYZ\": { \"CellDensities\": [ {\"IsDensityEnabled\": false, \"TargetDensity\": 0} ] },");
            sb.AppendLine("  \"WallXY\": { \"CellDensities\": [ {\"IsDensityEnabled\": false, \"TargetDensity\": 0} ] },");
            sb.AppendLine("  \"IsBlockLimitEnabled\": false,");
            sb.AppendLine("  \"MaxBlocks\": -1,");
            sb.AppendLine("  \"IsTimeLimitEnabled\": false,");
            sb.AppendLine("  \"TimeLimitSeconds\": -1.0,");
            sb.AppendLine("  \"Mode\": 0,");

            if (validStructures.Length > 0)
            {
                sb.AppendLine("  \"AvailableStructures\": [");
                for (var i = 0; i < validStructures.Length; i++)
                {
                    var item = validStructures[i];
                    sb.Append($"    {{ \"Name\": \"{item.Config.DisplayName}\", \"MaxCount\": {item.MaxCount} }}");
                    if (i < validStructures.Length - 1) sb.Append(",");
                    sb.AppendLine();
                }
                sb.AppendLine("  ],");
            }
            else
            {
                sb.AppendLine("  \"AvailableStructures\": [],");
            }

            sb.AppendLine("  \"Dialogue\": {");
            sb.AppendLine("    \"Replicas\": [");
            sb.AppendLine("      { \"Text\": \"First line of dialogue.\", \"AudioClipName\": \"voice_line_01\" },");
            sb.AppendLine("      { \"Text\": \"Second line (no audio).\", \"AudioClipName\": \"\" }");
            sb.AppendLine("    ]");
            sb.AppendLine("  }");
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine("AudioClipName must match the exact filename of the AudioClip in the Unity project (without extension). Leave empty string for silent replica.");

            EditorGUIUtility.systemCopyBuffer = sb.ToString();
            EditorUtility.DisplayDialog("Success", "AI prompt copied to clipboard!", "OK");
        }

        private void ProcessImport()
        {
            if (string.IsNullOrWhiteSpace(_jsonInput)) return;

            var cleanJson = _jsonInput.Trim();
            if (cleanJson.StartsWith("```"))
            {
                var firstNewline = cleanJson.IndexOf('\n');
                if (firstNewline > 0) cleanJson = cleanJson.Substring(firstNewline + 1);
                if (cleanJson.EndsWith("```")) cleanJson = cleanJson.Substring(0, cleanJson.Length - 3);
                cleanJson = cleanJson.Trim();
            }

            var dto = JsonUtility.FromJson<LevelConfigDto>(cleanJson);
            if (dto is null || dto.InitialBlocks is null || dto.InitialBlocks.Length == 0)
            {
                Debug.LogError("Invalid JSON or empty blocks.");
                return;
            }
            if (dto.FloorMatrix is null || dto.FloorMatrix.Length != 25)
            {
                Debug.LogError($"Invalid FloorMatrix length. Expected 25, got {dto.FloorMatrix?.Length ?? 0}.");
                return;
            }

            DialogueConfig dialogueConfig = null;
            if (dto.Dialogue != null && dto.Dialogue.Replicas != null && dto.Dialogue.Replicas.Length > 0)
            {
                dialogueConfig = ScriptableObject.CreateInstance<DialogueConfig>();
                var serializedDialogue = new SerializedObject(dialogueConfig);
                var replicasProperty = serializedDialogue.FindProperty("<Replicas>k__BackingField");

                if (replicasProperty != null)
                {
                    replicasProperty.arraySize = dto.Dialogue.Replicas.Length;
                    for (int i = 0; i < dto.Dialogue.Replicas.Length; i++)
                    {
                        var replicaProp = replicasProperty.GetArrayElementAtIndex(i);
                        var textProp = replicaProp.FindPropertyRelative("<Text>k__BackingField");
                        var audioProp = replicaProp.FindPropertyRelative("<Audio>k__BackingField");

                        if (textProp != null)
                            textProp.stringValue = dto.Dialogue.Replicas[i].Text ?? string.Empty;

                        if (audioProp != null)
                        {
                            var clipName = dto.Dialogue.Replicas[i].AudioClipName;
                            var clip = ResolveAudioClip(clipName);
                            if (clip != null)
                            {
                                audioProp.objectReferenceValue = clip;
                            }
                            else if (!string.IsNullOrWhiteSpace(clipName))
                            {
                                Debug.LogWarning($"[LevelImporter] AudioClip '{clipName}' not found in project for replica {i}.");
                            }
                        }
                    }
                    serializedDialogue.ApplyModifiedProperties();
                }
            }

            var config = ScriptableObject.CreateInstance<LevelConfig>();
            var wallYZ = MapWallData(dto.WallYZ);
            var wallXY = MapWallData(dto.WallXY);
            config.SetData(dto.InitialBlocks, dto.FloorMatrix, wallYZ, wallXY);
            config.SetBlockLimit(dto.IsBlockLimitEnabled, dto.MaxBlocks);
            config.SetTimeLimit(dto.IsTimeLimitEnabled, dto.TimeLimitSeconds);

            if (dto.Mode == GameMode.Structures && dto.AvailableStructures is not null)
            {
                var structures = new StructureSpawnData[dto.AvailableStructures.Length];
                for (var i = 0; i < dto.AvailableStructures.Length; i++)
                {
                    var structDto = dto.AvailableStructures[i];
                    var resolvedConfig = ResolveStructureConfig(structDto.Name);
                    if (resolvedConfig is null)
                    {
                        Debug.LogError($"StructureConfig with name '{structDto.Name}' not found in project.");
                        if (dialogueConfig != null) ScriptableObject.DestroyImmediate(dialogueConfig);
                        ScriptableObject.DestroyImmediate(config);
                        return;
                    }
                    structures[i] = new StructureSpawnData(resolvedConfig, structDto.MaxCount);
                }
                config.SetAvailableStructures(structures);
            }

            var serializedConfig = new SerializedObject(config);
            var modeProperty = serializedConfig.FindProperty("<Mode>k__BackingField");
            if (modeProperty != null)
                modeProperty.enumValueIndex = (int)dto.Mode;

            if (dialogueConfig != null)
            {
                var dialogueProperty = serializedConfig.FindProperty("<DialogueConfig>k__BackingField");
                if (dialogueProperty != null)
                    dialogueProperty.objectReferenceValue = dialogueConfig;
            }
            serializedConfig.ApplyModifiedProperties();

            if (!ValidateSolvability(config))
            {
                Debug.LogError("Level is mathematically unsolvable.");
                if (dialogueConfig != null) ScriptableObject.DestroyImmediate(dialogueConfig);
                ScriptableObject.DestroyImmediate(config);
                return;
            }

            if (_autoFixShadows)
                RecalculateShadowsAndDensities(config);

            CreateDirectoryIfNotExists(_savePath);

            if (dialogueConfig != null)
            {
                var dialoguePath = Path.Combine(_savePath, $"{_assetName}_Dialogue.asset");
                AssetDatabase.CreateAsset(dialogueConfig, dialoguePath);
            }

            var fullPath = Path.Combine(_savePath, $"{_assetName}.asset");
            EditorUtility.SetDirty(config);
            AssetDatabase.CreateAsset(config, fullPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[LevelImporter] Successfully created level '{_assetName}'{(dialogueConfig != null ? " and dialogue" : "")}.");
        }

        private StructureConfig ResolveStructureConfig(string displayName)
        {
            var guids = AssetDatabase.FindAssets("t:StructureConfig");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var config = AssetDatabase.LoadAssetAtPath<StructureConfig>(path);
                if (config is not null && config.DisplayName == displayName)
                    return config;
            }
            return null;
        }

        private AudioClip ResolveAudioClip(string clipName)
        {
            if (string.IsNullOrWhiteSpace(clipName)) return null;

            var guids = AssetDatabase.FindAssets($"t:AudioClip {clipName}");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clip is not null && clip.name == clipName)
                    return clip;
            }

            var allGuids = AssetDatabase.FindAssets("t:AudioClip");
            foreach (var guid in allGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clip is not null && clip.name == clipName)
                    return clip;
            }

            return null;
        }

        private static WallData MapWallData(WallDataDto dto)
        {
            var data = new WallData();
            if (dto?.CellDensities is null) return data;
            var densities = new WallCellDensityData[dto.CellDensities.Length];
            for (var i = 0; i < dto.CellDensities.Length; i++)
            {
                var d = dto.CellDensities[i];
                densities[i] = new WallCellDensityData(d.IsDensityEnabled, d.TargetDensity);
            }
            data.SetDensities(densities);
            return data;
        }

        private static void CreateDirectoryIfNotExists(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parts = path.Split('/');
            var currentPath = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var nextPath = $"{currentPath}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(nextPath))
                    AssetDatabase.CreateFolder(currentPath, parts[i]);
                currentPath = nextPath;
            }
        }

        private static bool ValidateSolvability(LevelConfig config)
        {
            var blocks = new HashSet<Vector3Int>(config.InitialBlocks);
            var floor = config.FloorMatrix;
            if (blocks.Count == 0) return false;
            foreach (var block in blocks)
            {
                if (block.x < 0 || block.x >= 5 || block.y < 0 || block.y >= 5 || block.z < 0 || block.z >= 5)
                    return false;
                if (block.y == 0)
                {
                    var floorIndex = block.x * 5 + block.z;
                    if (floor is null || floorIndex >= floor.Length || !floor[floorIndex])
                        return false;
                }
                else if (config.Mode == GameMode.Blocks)
                {
                    var below = new Vector3Int(block.x, block.y - 1, block.z);
                    if (!blocks.Contains(below))
                        return false;
                }
            }
            return true;
        }

        private static void RecalculateShadowsAndDensities(LevelConfig config)
        {
            var grid = new bool[5, 5, 5];
            foreach (var block in config.InitialBlocks)
                grid[block.x, block.y, block.z] = true;

            var wallYZ = new WallCellDensityData[25];
            var wallXY = new WallCellDensityData[25];

            for (var i = 0; i < 25; i++)
            {
                var y = i / 5;
                var z = i % 5;
                var countYZ = 0;
                for (var x = 0; x < 5; x++)
                    if (grid[x, y, z]) countYZ++;
                wallYZ[i] = new WallCellDensityData(countYZ > 0, countYZ);

                var x2 = i / 5;
                var y2 = i % 5;
                var countXY = 0;
                for (var z2 = 0; z2 < 5; z2++)
                    if (grid[x2, y2, z2]) countXY++;
                wallXY[i] = new WallCellDensityData(countXY > 0, countXY);
            }

            var dataYZ = new WallData();
            dataYZ.SetDensities(wallYZ);
            var dataXY = new WallData();
            dataXY.SetDensities(wallXY);

            config.SetData(config.InitialBlocks, config.FloorMatrix, dataYZ, dataXY);
            config.SetBlockLimit(config.IsBlockLimitEnabled, config.MaxBlocks);
            config.SetTimeLimit(config.IsTimeLimitEnabled, config.TimeLimitSeconds);
        }

        [Serializable]
        private class LevelConfigDto
        {
            public Vector3Int[] InitialBlocks;
            public bool[] FloorMatrix;
            public WallDataDto WallYZ;
            public WallDataDto WallXY;
            public bool IsBlockLimitEnabled;
            public int MaxBlocks = -1;
            public bool IsTimeLimitEnabled;
            public float TimeLimitSeconds = -1f;
            public GameMode Mode;
            public StructureSpawnDataDto[] AvailableStructures;
            public DialogueDto Dialogue;
        }

        [Serializable]
        private class WallDataDto
        {
            public WallCellDensityDataDto[] CellDensities;
        }

        [Serializable]
        private class WallCellDensityDataDto
        {
            public bool IsDensityEnabled;
            public int TargetDensity;
        }

        [Serializable]
        private class StructureSpawnDataDto
        {
            public string Name;
            public int MaxCount = -1;
        }

        [Serializable]
        private class DialogueDto
        {
            public DialogueReplicaDto[] Replicas;
        }

        [Serializable]
        private class DialogueReplicaDto
        {
            public string Text;
            public string AudioClipName;
        }
    }
}