using System;
using System.Collections.Generic;
using System.IO;
using Game.Data;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.Tool
{
    public class LevelImporterWindow : EditorWindow
    {
        [field: SerializeField] public string JsonInput { get; private set; } = string.Empty;
        [field: SerializeField] public string AssetName { get; private set; } = "NewLevel";
        [field: SerializeField] public string SavePath { get; private set; } = "Assets/Data/Levels";
        [field: SerializeField] public bool AutoFixShadows { get; private set; } = true;

        [MenuItem("Tools/Level Importer")]
        public static void ShowWindow() => GetWindow<LevelImporterWindow>("Level Importer");

        private void OnGUI()
        {
            JsonInput = EditorGUILayout.TextArea(JsonInput, GUILayout.Height(300));
            AssetName = EditorGUILayout.TextField("Asset Name", AssetName);
            SavePath = EditorGUILayout.TextField("Save Path", SavePath);
            AutoFixShadows = EditorGUILayout.Toggle("Auto-Fix Shadows", AutoFixShadows);

            if (GUILayout.Button("Create"))
                ProcessImport();
        }

        private void ProcessImport()
        {
            if (string.IsNullOrWhiteSpace(JsonInput))
                return;

            var dto = JsonUtility.FromJson<LevelConfigDto>(JsonInput);

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
                Debug.LogError("Level is mathematically unsolvable (blocks disconnected or floating).");
                return;
            }

            if (AutoFixShadows)
                RecalculateShadowsAndDensities(config);

            CreateDirectoryIfNotExists(SavePath);

            var fullPath = Path.Combine(SavePath, $"{AssetName}.asset");

            EditorUtility.SetDirty(config);
            AssetDatabase.CreateAsset(config, fullPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Level '{AssetName}' created. Floor cells active: {CountTrue(dto.FloorMatrix)}/25");
        }

        private static int CountTrue(bool[] array)
        {
            var count = 0;
            foreach (var b in array)
                if (b) count++;
            return count;
        }

        private static WallData MapWallData(WallDataDto dto)
        {
            var data = new WallData();
            if (dto?.CellDensities is null)
                return data;

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
                {
                    Debug.LogError($"Block {block} is out of bounds.");
                    return false;
                }

                if (block.y == 0)
                {
                    var floorIndex = block.x * 5 + block.z;
                    if (floor is null || floorIndex >= floor.Length || !floor[floorIndex])
                    {
                        Debug.LogError($"Block {block} is on the ground but FloorMatrix is false at index {floorIndex}.");
                        return false;
                    }
                }
                else
                {
                    var below = new Vector3Int(block.x, block.y - 1, block.z);
                    if (!blocks.Contains(below))
                    {
                        Debug.LogError($"Block {block} is floating. Missing support at {below}.");
                        return false;
                    }
                }
            }

            if (blocks.Count == 0) return false;

            var visited = new HashSet<Vector3Int>();
            var queue = new Queue<Vector3Int>();
            var enumerator = blocks.GetEnumerator();
            enumerator.MoveNext();
            var startBlock = enumerator.Current;
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

            if (visited.Count != blocks.Count)
            {
                foreach (var b in blocks)
                {
                    if (!visited.Contains(b))
                    {
                        Debug.LogError($"Block {b} is disconnected from the main structure starting at {startBlock}.");
                        break;
                    }
                }
                return false;
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