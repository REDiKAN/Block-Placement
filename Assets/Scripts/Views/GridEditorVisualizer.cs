#if UNITY_EDITOR
using UnityEngine;
using Zenject;
using Game.Data;
using Game.Core;
using Game.Installers;
using Game.Services.Grid;
using Game.Services.Rotation;

namespace Game.Views
{
    [ExecuteInEditMode]
    public class GridEditorVisualizer : MonoBehaviour
    {
        private const int GridSize = 5;
        private const float CellSize = 1f;
        private static readonly Vector3 CellOffset = new(0.5f, 0.5f, 0.5f);

        [Header("Editor Fallback")]
        [SerializeField] private LevelConfig EditorConfig;

        [Header("Visualization")]
        [SerializeField] private bool ShowGrid = true;
        [SerializeField] private bool ShowInitialBlocks = true;
        [SerializeField] private bool ShowTargetBlocks = true;
        [SerializeField] private bool ShowActiveBlocks = true;

        [Header("Colors")]
        [SerializeField] private Color GridColor = new(1f, 1f, 1f, 0.2f);
        [SerializeField] private Color InitialBlockColor = new(0f, 1f, 0f, 0.3f);
        [SerializeField] private Color TargetBlockColor = new(0f, 0.5f, 1f, 0.4f);
        [SerializeField] private Color ActiveBlockColor = new(1f, 1f, 0f, 0.4f);

        private LevelConfig _resolvedConfig;
        private IGridService _gridService;
        private IRotationService _rotationService;
        private GameInstaller _installer;
        private bool _cachedPlayMode;

        private void OnEnable()
        {
            ResolveDependencies();
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying != _cachedPlayMode)
            {
                _cachedPlayMode = Application.isPlaying;
                ResolveDependencies();
            }

            if (ShowGrid) DrawGrid();
            DrawLevelData();
        }

        private void ResolveDependencies()
        {
            if (!Application.isPlaying)
            {
                _resolvedConfig = EditorConfig;
                _gridService = null;
                _rotationService = null;
                return;
            }

            _installer = Object.FindObjectOfType<GameInstaller>();
            var sceneContext = Object.FindObjectOfType<SceneContext>();
            var container = sceneContext?.Container;

            _gridService = container?.TryResolve<IGridService>();
            _rotationService = container?.TryResolve<IRotationService>();
            _resolvedConfig = ResolveActiveLevelConfig();
        }

        private LevelConfig ResolveActiveLevelConfig()
        {
            if (EndlessContext.IsEndlessModeActive) return null;
            if (_installer is null) return null;

            var catalog = _installer.LevelCatalog;
            if (catalog?.Categories is null) return _installer.LevelConfig;

            var catId = LevelContext.SelectedCategoryId;
            var lvlId = LevelContext.SelectedLevelId;

            if (catId < 0 || catId >= catalog.Categories.Length) return _installer.LevelConfig;
            var category = catalog.Categories[catId];
            if (category?.Levels is null) return _installer.LevelConfig;
            if (lvlId < 0 || lvlId >= category.Levels.Length) return _installer.LevelConfig;

            return category.Levels[lvlId] ?? _installer.LevelConfig;
        }

        private void DrawLevelData()
        {
            if (Application.isPlaying)
            {
                if (ShowTargetBlocks && _rotationService is not null)
                {
                    var targetBlocks = _rotationService.CurrentInitialBlocks;
                    if (targetBlocks is not null)
                    {
                        Gizmos.color = TargetBlockColor;
                        foreach (var block in targetBlocks)
                        {
                            if (IsWithinBounds(block))
                                Gizmos.DrawCube(block + CellOffset, Vector3.one * CellSize);
                        }
                    }
                }

                if (ShowActiveBlocks && _gridService is not null)
                {
                    Gizmos.color = ActiveBlockColor;
                    for (var x = 0; x < GridSize; x++)
                        for (var y = 0; y < GridSize; y++)
                            for (var z = 0; z < GridSize; z++)
                            {
                                var cell = new Vector3Int(x, y, z);
                                if (_gridService.IsCellOccupied(cell))
                                    Gizmos.DrawWireCube(cell + CellOffset, Vector3.one * CellSize * 0.95f);
                            }
                }
            }
            else
            {
                if (ShowInitialBlocks && _resolvedConfig?.InitialBlocks is not null)
                {
                    Gizmos.color = InitialBlockColor;
                    foreach (var block in _resolvedConfig.InitialBlocks)
                    {
                        if (IsWithinBounds(block))
                            Gizmos.DrawCube(block + CellOffset, Vector3.one * CellSize);
                    }
                }
            }
        }

        private void DrawGrid()
        {
            Gizmos.color = GridColor;
            for (var x = 0; x < GridSize; x++)
                for (var y = 0; y < GridSize; y++)
                    for (var z = 0; z < GridSize; z++)
                    {
                        Gizmos.DrawWireCube(new Vector3Int(x, y, z) + CellOffset, Vector3.one * CellSize);
                    }
        }

        private static bool IsWithinBounds(Vector3Int cell) =>
            cell.x >= 0 && cell.x < GridSize &&
            cell.y >= 0 && cell.y < GridSize &&
            cell.z >= 0 && cell.z < GridSize;
    }
}
#endif