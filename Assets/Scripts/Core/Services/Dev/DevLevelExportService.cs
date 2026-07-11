using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Game.Data;
using Game.Services.Grid;
namespace Game.Services.Dev
{
    public interface IDevLevelExportService { }
    public class DevLevelExportService : IDevLevelExportService, IInitializable, IDisposable
    {
        private const int GridSize = 5;
        private const int FloorCellCount = 25;
        private const string SavePath = "Assets/LevelConfig.asset";
        private readonly CompositeDisposable _disposables = new();
        private readonly IDevInputService _devInputService;
        private readonly IGridService _gridService;
        public DevLevelExportService(IDevInputService devInputService, IGridService gridService)
        {
            _devInputService = devInputService;
            _gridService = gridService;
        }
        public void Initialize()
        {
            _devInputService.OnExportRequested
            .Subscribe(_ => ExportLevel())
            .AddTo(_disposables);
            Debug.Log("[DevLevelExportService] Service initialized. Listening for level export requests.");
        }
        private void ExportLevel()
        {
            Debug.Log("[DevLevelExportService] Starting level export process...");
            var blocks = new List<Vector3Int>();
            var floorMatrix = new bool[FloorCellCount];
            for (var x = 0; x < GridSize; x++)
            {
                for (var z = 0; z < GridSize; z++)
                {
                    floorMatrix[x * GridSize + z] = _gridService.IsFloorExists(new Vector2Int(x, z));
                }
            }
            for (var x = 0; x < GridSize; x++)
            {
                for (var y = 0; y < GridSize; y++)
                {
                    for (var z = 0; z < GridSize; z++)
                    {
                        var cell = new Vector3Int(x, y, z);
                        if (_gridService.IsCellOccupied(cell))
                        {
                            blocks.Add(cell);
                        }
                    }
                }
            }
            Debug.Log($"[DevLevelExportService] Collected {blocks.Count} blocks from the grid.");
            var config = ScriptableObject.CreateInstance<ShadowLevelConfig>();
            config.SetData(blocks.ToArray(), floorMatrix);
            Debug.Log("[DevLevelExportService] ShadowLevelConfig instance created and populated with real footprint data.");
#if UNITY_EDITOR
            AssetDatabase.CreateAsset(config, SavePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[DevLevelExportService] Level configuration successfully saved to {SavePath}");
#else
Debug.LogWarning("[DevLevelExportService] Asset saving is only available within the Unity Editor environment.");
#endif
        }
        public void Dispose() => _disposables?.Dispose();
    }
}