using System;
using UniRx;
using UnityEngine;
using Zenject;
using Game.Data;
using Game.Services.Menu;
using Game.Services.Grid;
using Game.Services.Shadow;

namespace Game.Services.Preview
{
    public class PreviewStateManager : IInitializable, IDisposable
    {
        private readonly IPreviewService _previewService;
        private readonly IGridService _gridService;
        private readonly IShadowCalculationService _calculationService;
        private readonly IPreviewShadowService _shadowService;
        private readonly CompositeDisposable _disposables = new();

        private const int GridSize = 5;

        public PreviewStateManager(
            IPreviewService previewService,
            IGridService gridService,
            IShadowCalculationService calculationService,
            IPreviewShadowService shadowService)
        {
            _previewService = previewService;
            _gridService = gridService;
            _calculationService = calculationService;
            _shadowService = shadowService;
        }

        public void Initialize()
        {
            _previewService.PreviewLevel
                .Subscribe(UpdatePreview)
                .AddTo(_disposables);
        }

        private void UpdatePreview(LevelConfig level)
        {
            if (level is null)
            {
                ClearPreview();
                return;
            }

            for (var x = 0; x < GridSize; x++)
            {
                for (var z = 0; z < GridSize; z++)
                {
                    var index = x * GridSize + z;
                    var exists = level.FloorMatrix is not null && index < level.FloorMatrix.Length && level.FloorMatrix[index];
                    _gridService.SetFloorExists(new Vector2Int(x, z), exists);
                }
            }

            try
            {
                var projection = _calculationService.Calculate(level.InitialBlocks, GridSize);
                _shadowService.UpdateShadows(projection.Wall1, projection.Wall2);

                var wall1Densities = level.WallYZ?.CellDensities ?? Array.Empty<WallCellDensityData>();
                var wall2Densities = level.WallXY?.CellDensities ?? Array.Empty<WallCellDensityData>();
                _shadowService.UpdateDensities(wall1Densities, wall2Densities);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PreviewStateManager] Failed to calculate shadows: {ex.Message}");
                _shadowService.ClearShadows();
            }
        }

        private void ClearPreview()
        {
            for (var x = 0; x < GridSize; x++)
            {
                for (var z = 0; z < GridSize; z++)
                {
                    _gridService.SetFloorExists(new Vector2Int(x, z), true);
                }
            }
            _shadowService.ClearShadows();
        }

        public void Dispose() => _disposables?.Dispose();
    }
}