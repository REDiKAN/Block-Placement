using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Game.Data;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.Tool
{
    public class LevelImporterWindow : EditorWindow
    {
        [field: SerializeField] private string _jsonInput = string.Empty;
        [field: SerializeField] private string _assetName = "NewLevel";
        [field: SerializeField] private string _savePath = "Assets/Data/Levels";
        [field: SerializeField] private bool _autoFixShadows = true;
        [field: SerializeField] private List<StructurePromptItem> _structuresForPrompt = new();

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

            if (GUILayout.Button("Add Structure"))
                _structuresForPrompt.Add(new StructurePromptItem());

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

            EditorGUILayout.Space();

            if (GUILayout.Button("Generate AI Prompt", GUILayout.Height(30)))
                GenerateAIPrompt();
        }

        private void GenerateAIPrompt()
        {
            var validStructures = _structuresForPrompt.Where(s => s.Config is not null).ToArray();
            if (validStructures.Length == 0)
            {
                EditorUtility.DisplayDialog("Error", "No valid structures selected.", "OK");
                return;
            }

            var prompt = StructurePromptBuilder.Build(validStructures);
            EditorGUIUtility.systemCopyBuffer = prompt;
            EditorUtility.DisplayDialog("Success", "AI prompt copied to clipboard!", "OK");
        }

        private void ProcessImport()
        {
            if (string.IsNullOrWhiteSpace(_jsonInput)) return;

            var dto = JsonUtility.FromJson<LevelConfigDto>(_jsonInput);
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

            var config = ScriptableObject.CreateInstance<LevelConfig>();
            var wallYZ = MapWallData(dto.WallYZ);
            var wallXY = MapWallData(dto.WallXY);

            config.SetData(dto.InitialBlocks, dto.FloorMatrix, wallYZ, wallXY);
            config.SetBlockLimit(dto.IsBlockLimitEnabled, dto.MaxBlocks);
            config.SetTimeLimit(dto.IsTimeLimitEnabled, dto.TimeLimitSeconds);

            if (!ValidateSolvability(config))
            {
                Debug.LogError("Level is mathematically unsolvable.");
                return;
            }

            if (_autoFixShadows)
                RecalculateShadowsAndDensities(config);

            CreateDirectoryIfNotExists(_savePath);
            var fullPath = Path.Combine(_savePath, $"{_assetName}.asset");

            EditorUtility.SetDirty(config);
            AssetDatabase.CreateAsset(config, fullPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
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
                else
                {
                    var below = new Vector3Int(block.x, block.y - 1, block.z);
                    if (!blocks.Contains(below))
                        return false;
                }
            }

            if (blocks.Count == 0) return false;

            var visited = new HashSet<Vector3Int>();
            var queue = new Queue<Vector3Int>();
            var startBlock = blocks.First();

            queue.Enqueue(startBlock);
            visited.Add(startBlock);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var directions = new[]
                {
                    Vector3Int.up, Vector3Int.down,
                    Vector3Int.left, Vector3Int.right,
                    Vector3Int.forward, Vector3Int.back
                };

                foreach (var dir in directions)
                {
                    var next = current + dir;
                    if (blocks.Contains(next) && visited.Add(next))
                        queue.Enqueue(next);
                }
            }

            return visited.Count == blocks.Count;
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
    }
}