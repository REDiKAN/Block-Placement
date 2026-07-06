using System.IO;
using UnityEditor;
using UnityEngine;
using Game.Data;

namespace Game.Editor.Tool
{
    public class ShadowLevelEditorWindow : EditorWindow
    {
        private const int _gridSize = 5;
        private const int _cellCount = 25;
        private const string _defaultConfigPath = "Assets/ShadowLevelConfigs";

        private ShadowLevelConfig _currentConfig;
        private SerializedObject _serializedConfig;
        private SerializedProperty _wall1Prop;
        private SerializedProperty _wall2Prop;
        private SerializedProperty _floorProp;

        private bool[,,] _testGrid = new bool[_gridSize, _gridSize, _gridSize];
        private int _currentZLayer;
        private int _difficulty;
        private Vector2 _scrollPosition;

        private readonly bool[] _calcWall1 = new bool[_cellCount];
        private readonly bool[] _calcWall2 = new bool[_cellCount];
        private readonly ShadowLevelGenerator _generator = new();

        [MenuItem("Tools/Shadow Level Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<ShadowLevelEditorWindow>("Shadow Level Editor");
            window.minSize = new Vector2(850, 650);
        }

        private void OnGUI()
        {
            DrawToolbar();
            DrawGenerationPanel();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            EditorGUILayout.BeginHorizontal();
            Draw3DGridEditor();
            GUILayout.FlexibleSpace();
            DrawTargetShadowEditor();
            EditorGUILayout.EndHorizontal();

            DrawValidationPanel();

            EditorGUILayout.EndScrollView();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("New Config", EditorStyles.toolbarButton))
                CreateNewConfig();

            if (GUILayout.Button("Save", EditorStyles.toolbarButton))
                SaveConfig();

            EditorGUILayout.Space();

            _currentConfig = (ShadowLevelConfig)EditorGUILayout.ObjectField(_currentConfig, typeof(ShadowLevelConfig), false, GUILayout.Width(300));

            if (_currentConfig != null && _serializedConfig == null)
                InitSerializedConfig();
            else if (_currentConfig == null)
            {
                _serializedConfig = null;
                _wall1Prop = null;
                _wall2Prop = null;
                _floorProp = null;
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawGenerationPanel()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            EditorGUILayout.LabelField("Difficulty", GUILayout.Width(80));
            _difficulty = EditorGUILayout.IntSlider(_difficulty, 0, 10, GUILayout.Width(300));
            EditorGUILayout.LabelField(_difficulty.ToString(), GUILayout.Width(30));

            if (GUILayout.Button("Generate", EditorStyles.toolbarButton))
                GenerateLevel();

            EditorGUILayout.EndHorizontal();
        }

        private void GenerateLevel()
        {
            if (_currentConfig is null || _serializedConfig is null) return;

            var result = _generator.Generate(_difficulty);
            _testGrid = result.Grid;

            for (var i = 0; i < _cellCount; i++)
            {
                if (i < _wall1Prop.arraySize)
                    _wall1Prop.GetArrayElementAtIndex(i).boolValue = result.Wall1Target[i];
                if (i < _wall2Prop.arraySize)
                    _wall2Prop.GetArrayElementAtIndex(i).boolValue = result.Wall2Target[i];
                if (i < _floorProp.arraySize)
                    _floorProp.GetArrayElementAtIndex(i).boolValue = result.FloorMatrix[i];
            }

            _serializedConfig.ApplyModifiedProperties();
            EditorUtility.SetDirty(_currentConfig);
            AssetDatabase.SaveAssets();
        }

        private void CreateNewConfig()
        {
            if (!Directory.Exists(_defaultConfigPath))
                Directory.CreateDirectory(_defaultConfigPath);

            var path = EditorUtility.SaveFilePanelInProject("Save Shadow Level Config", "NewShadowLevel", "asset", "Enter a file name", _defaultConfigPath);

            if (string.IsNullOrEmpty(path)) return;

            var config = CreateInstance<ShadowLevelConfig>();
            AssetDatabase.CreateAsset(config, path);
            AssetDatabase.SaveAssets();

            _currentConfig = config;
            InitSerializedConfig();
            ClearArrays();
            _serializedConfig.ApplyModifiedProperties();
        }

        private void SaveConfig()
        {
            if (_serializedConfig is null) return;

            _serializedConfig.ApplyModifiedProperties();
            EditorUtility.SetDirty(_currentConfig);
            AssetDatabase.SaveAssets();
        }

        private void InitSerializedConfig()
        {
            _serializedConfig = new SerializedObject(_currentConfig);
            _wall1Prop = _serializedConfig.FindProperty("<Wall1Target>k__BackingField");
            _wall2Prop = _serializedConfig.FindProperty("<Wall2Target>k__BackingField");
            _floorProp = _serializedConfig.FindProperty("<FloorMatrix>k__BackingField");

            if (_wall1Prop != null && _wall1Prop.arraySize != _cellCount) _wall1Prop.arraySize = _cellCount;
            if (_wall2Prop != null && _wall2Prop.arraySize != _cellCount) _wall2Prop.arraySize = _cellCount;
            if (_floorProp != null && _floorProp.arraySize != _cellCount) _floorProp.arraySize = _cellCount;
        }

        private void ClearArrays()
        {
            if (_wall1Prop != null)
                for (var i = 0; i < _cellCount; i++)
                    _wall1Prop.GetArrayElementAtIndex(i).boolValue = false;

            if (_wall2Prop != null)
                for (var i = 0; i < _cellCount; i++)
                    _wall2Prop.GetArrayElementAtIndex(i).boolValue = false;

            if (_floorProp != null)
                for (var i = 0; i < _cellCount; i++)
                    _floorProp.GetArrayElementAtIndex(i).boolValue = true;

            for (var x = 0; x < _gridSize; x++)
                for (var y = 0; y < _gridSize; y++)
                    for (var z = 0; z < _gridSize; z++)
                        _testGrid[x, y, z] = false;
        }

        private void Draw3DGridEditor()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(250));
            EditorGUILayout.LabelField("3D Block Placer", EditorStyles.boldLabel);

            _currentZLayer = EditorGUILayout.IntSlider("Z Layer", _currentZLayer, 0, _gridSize - 1);

            if (GUILayout.Button("Clear 3D Grid"))
                for (var x = 0; x < _gridSize; x++)
                    for (var y = 0; y < _gridSize; y++)
                        for (var z = 0; z < _gridSize; z++)
                            _testGrid[x, y, z] = false;

            EditorGUILayout.Space();

            for (var y = _gridSize - 1; y >= 0; y--)
            {
                EditorGUILayout.BeginHorizontal();
                for (var x = 0; x < _gridSize; x++)
                {
                    var isOccupied = _testGrid[x, y, _currentZLayer];
                    var newOccupied = GUILayout.Toggle(isOccupied, "", GUILayout.Width(35), GUILayout.Height(35));

                    if (newOccupied != isOccupied)
                        _testGrid[x, y, _currentZLayer] = newOccupied;
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawTargetShadowEditor()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(450));
            EditorGUILayout.LabelField("Target Shadows & Floor", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical();
            DrawGrid2D(_wall1Prop, "Wall 1 (YZ)");
            EditorGUILayout.EndVertical();

            GUILayout.Space(20);

            EditorGUILayout.BeginVertical();
            DrawGrid2D(_wall2Prop, "Wall 2 (XY)");
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(20);
            DrawFloorEditor();

            EditorGUILayout.EndVertical();
        }

        private void DrawGrid2D(SerializedProperty arrayProp, string label)
        {
            EditorGUILayout.LabelField(label, EditorStyles.miniBoldLabel);

            if (arrayProp is null)
            {
                EditorGUILayout.HelpBox("Load or Create a config first.", MessageType.Info);
                return;
            }

            for (var y = _gridSize - 1; y >= 0; y--)
            {
                EditorGUILayout.BeginHorizontal();
                for (var x = 0; x < _gridSize; x++)
                {
                    var index = y * _gridSize + x;
                    if (index >= arrayProp.arraySize) continue;

                    var element = arrayProp.GetArrayElementAtIndex(index);
                    var currentValue = element.boolValue;
                    var newValue = GUILayout.Toggle(currentValue, "", GUILayout.Width(35), GUILayout.Height(35));

                    if (newValue != currentValue)
                        element.boolValue = newValue;
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawFloorEditor()
        {
            EditorGUILayout.LabelField("Floor Matrix (XZ)", EditorStyles.miniBoldLabel);

            if (_floorProp is null)
            {
                EditorGUILayout.HelpBox("Load or Create a config first.", MessageType.Info);
                return;
            }

            for (var z = _gridSize - 1; z >= 0; z--)
            {
                EditorGUILayout.BeginHorizontal();
                for (var x = 0; x < _gridSize; x++)
                {
                    var index = x * _gridSize + z;
                    if (index >= _floorProp.arraySize) continue;

                    var element = _floorProp.GetArrayElementAtIndex(index);
                    var currentValue = element.boolValue;
                    var newValue = GUILayout.Toggle(currentValue, "", GUILayout.Width(35), GUILayout.Height(35));

                    if (newValue != currentValue)
                        element.boolValue = newValue;
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawValidationPanel()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Validation & Feedback", EditorStyles.boldLabel);

            if (_wall1Prop is null || _wall2Prop is null)
            {
                EditorGUILayout.HelpBox("No configuration loaded.", MessageType.Warning);
                return;
            }

            CalculateShadowsForValidation();

            EditorGUILayout.BeginHorizontal();

            DrawValidationGrid("Current Wall 1", _calcWall1, _wall1Prop);
            GUILayout.Space(20);
            DrawValidationGrid("Current Wall 2", _calcWall2, _wall2Prop);

            EditorGUILayout.EndHorizontal();
        }

        private void CalculateShadowsForValidation()
        {
            for (var i = 0; i < _calcWall1.Length; i++) _calcWall1[i] = false;
            for (var i = 0; i < _calcWall2.Length; i++) _calcWall2[i] = false;

            for (var x = 0; x < _gridSize; x++)
                for (var y = 0; y < _gridSize; y++)
                    for (var z = 0; z < _gridSize; z++)
                        if (_testGrid[x, y, z])
                        {
                            _calcWall1[y * _gridSize + z] = true;
                            _calcWall2[x * _gridSize + y] = true;
                        }
        }

        private void DrawValidationGrid(string label, bool[] currentShadow, SerializedProperty targetProp)
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(label, EditorStyles.miniLabel);

            var missingCount = 0;
            var extraCount = 0;

            for (var y = _gridSize - 1; y >= 0; y--)
            {
                EditorGUILayout.BeginHorizontal();
                for (var x = 0; x < _gridSize; x++)
                {
                    var index = y * _gridSize + x;
                    var isTarget = targetProp.GetArrayElementAtIndex(index).boolValue;
                    var isCurrent = currentShadow[index];

                    var originalBg = GUI.backgroundColor;

                    if (isTarget && !isCurrent)
                    {
                        GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
                        missingCount++;
                    }
                    else if (!isTarget && isCurrent)
                    {
                        GUI.backgroundColor = new Color(1f, 0.8f, 0.2f);
                        extraCount++;
                    }
                    else if (isTarget && isCurrent)
                    {
                        GUI.backgroundColor = new Color(0.4f, 1f, 0.4f);
                    }
                    else
                    {
                        GUI.backgroundColor = new Color(0.3f, 0.3f, 0.3f);
                    }

                    GUILayout.Box("", GUILayout.Width(35), GUILayout.Height(35));
                    GUI.backgroundColor = originalBg;
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Missing: {missingCount} | Extra: {extraCount}");
            EditorGUILayout.EndVertical();
        }
    }
}