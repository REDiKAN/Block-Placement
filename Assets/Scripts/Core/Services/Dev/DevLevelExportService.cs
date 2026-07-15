using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Game.Data;
using Game.Services.Dev;
using Game.Services.Grid;
using Game.Services.Shadow;

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
        private readonly IShadowDensityService _densityService;
        private readonly IShadowCalculationService _calculationService;

        public DevLevelExportService(
            IDevInputService devInputService,
            IGridService gridService,
            IShadowDensityService densityService,
            IShadowCalculationService calculationService)
        {
            _devInputService = devInputService;
            _gridService = gridService;
            _densityService = densityService;
            _calculationService = calculationService;
        }

        public void Initialize()
        {
            _devInputService.OnExportRequested
                .Subscribe(_ => ExportLevel())
                .AddTo(_disposables);
        }

        private void ExportLevel()
        {
            var blocks = new List<Vector3Int>();
            var floorMatrix = new bool[FloorCellCount];

            for (var x = 0; x < GridSize; x++)
                for (var z = 0; z < GridSize; z++)
                    floorMatrix[x * GridSize + z] = _gridService.IsFloorExists(new Vector2Int(x, z));

            for (var x = 0; x < GridSize; x++)
                for (var y = 0; y < GridSize; y++)
                    for (var z = 0; z < GridSize; z++)
                    {
                        var cell = new Vector3Int(x, y, z);
                        if (_gridService.IsCellOccupied(cell))
                            blocks.Add(cell);
                    }

            var yzDensities = new WallCellDensityData[FloorCellCount];
            var xyDensities = new WallCellDensityData[FloorCellCount];

            for (var i = 0; i < FloorCellCount; i++)
            {
                var isYZEnabled = _densityService.IsDensityEnabled(0, i);
                var yzDensity = isYZEnabled ? _calculationService.CalculateDensity(0, i, _gridService, GridSize) : 0;
                if (yzDensity == 0) isYZEnabled = false;
                yzDensities[i] = new WallCellDensityData(isYZEnabled, yzDensity);

                var isXYEnabled = _densityService.IsDensityEnabled(1, i);
                var xyDensity = isXYEnabled ? _calculationService.CalculateDensity(1, i, _gridService, GridSize) : 0;
                if (xyDensity == 0) isXYEnabled = false;
                xyDensities[i] = new WallCellDensityData(isXYEnabled, xyDensity);
            }

            var wallYZData = new WallData();
            wallYZData.SetDensities(yzDensities);

            var wallXYData = new WallData();
            wallXYData.SetDensities(xyDensities);

            var config = ScriptableObject.CreateInstance<LevelConfig>();
            config.SetData(blocks.ToArray(), floorMatrix, wallYZData, wallXYData);

#if UNITY_EDITOR
            AssetDatabase.CreateAsset(config, SavePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#endif
        }

        public void Dispose() => _disposables?.Dispose();
    }
}