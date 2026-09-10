#if UNITY_EDITOR
using System;
using UniRx;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;
using Game.Services.Dev;

namespace Game.Editor.Tool
{
    public class LevelAutoTesterWindow : EditorWindow
    {
        private ILevelAutoTesterService _autoTesterService;
        private readonly CompositeDisposable _disposables = new();
        private float _delay = 1f;
        private AutoTesterState _state;
        private int _spawnedCount;
        private int _totalRequired;
        private string _objectLabel = "Objects";
        private int _configuredLimit;
        private bool _isBlockLimitEnabled;
        private bool _isTimeLimitEnabled;
        private float _remainingTime;

        [MenuItem("Tools/Level Auto Tester")]
        public static void ShowWindow() => GetWindow<LevelAutoTesterWindow>("Level Auto Tester");

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            SceneManager.sceneLoaded += OnSceneLoaded;
            ResolveService();
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            _disposables.Clear();
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
                ResolveService();
            else if (state == PlayModeStateChange.ExitingPlayMode)
            {
                _disposables.Clear();
                _autoTesterService = null;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (Application.isPlaying)
                EditorApplication.delayCall += ResolveService;
        }

        private void ResolveService()
        {
            _disposables.Clear();
            _autoTesterService = null;
            ResetLocalState();

            if (!Application.isPlaying) return;

            var sceneContext = UnityEngine.Object.FindObjectOfType<SceneContext>();
            if (sceneContext is null || sceneContext.Container is null) return;

            _autoTesterService = sceneContext.Container.TryResolve<ILevelAutoTesterService>();
            if (_autoTesterService is null) return;

            _autoTesterService.State
                .Subscribe(val => { _state = val; Repaint(); })
                .AddTo(_disposables);

            _autoTesterService.SpawnedCount
                .Subscribe(val => { _spawnedCount = val; Repaint(); })
                .AddTo(_disposables);

            _autoTesterService.TotalRequired
                .Subscribe(val => { _totalRequired = val; Repaint(); })
                .AddTo(_disposables);

            _autoTesterService.ObjectLabel
                .Subscribe(val => { _objectLabel = val; Repaint(); })
                .AddTo(_disposables);

            _autoTesterService.ConfiguredLimit
                .Subscribe(val => { _configuredLimit = val; Repaint(); })
                .AddTo(_disposables);

            _autoTesterService.IsBlockLimitEnabled
                .Subscribe(val => { _isBlockLimitEnabled = val; Repaint(); })
                .AddTo(_disposables);

            _autoTesterService.IsTimeLimitEnabled
                .Subscribe(val => { _isTimeLimitEnabled = val; Repaint(); })
                .AddTo(_disposables);

            _autoTesterService.RemainingTime
                .Subscribe(val => { _remainingTime = val; Repaint(); })
                .AddTo(_disposables);
        }

        private void ResetLocalState()
        {
            _state = AutoTesterState.Idle;
            _spawnedCount = 0;
            _totalRequired = 0;
            _objectLabel = "Objects";
            _configuredLimit = 0;
            _isBlockLimitEnabled = false;
            _isTimeLimitEnabled = false;
            _remainingTime = 0f;
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Level Auto Tester", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            _delay = EditorGUILayout.Slider("Delay (Seconds)", _delay, 0f, 10f);
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            if (_state == AutoTesterState.Running)
            {
                if (GUILayout.Button("Pause", GUILayout.Height(40)))
                    _autoTesterService?.PauseTest();
            }
            else
            {
                var playText = _state == AutoTesterState.Paused ? "Resume" : "Play";
                if (GUILayout.Button(playText, GUILayout.Height(40)))
                {
                    if (_autoTesterService is null)
                    {
                        ResolveService();
                        if (_autoTesterService is null)
                        {
                            Debug.LogError("[LevelAutoTesterWindow] Failed to resolve ILevelAutoTesterService.");
                            return;
                        }
                    }

                    if (_state == AutoTesterState.Paused)
                        _autoTesterService.ResumeTest();
                    else
                        _autoTesterService.StartTest(_delay);
                }
            }

            if (_state != AutoTesterState.Idle)
            {
                if (GUILayout.Button("Stop", GUILayout.Height(40)))
                    _autoTesterService?.StopTest();
            }
            EditorGUILayout.EndHorizontal();

            if (_state != AutoTesterState.Running)
            {
                if (GUILayout.Button("Step", GUILayout.Height(30)))
                    _autoTesterService?.StepTest();
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Level Design Validator", EditorStyles.boldLabel);

            var oldColor = GUI.color;
            if (!_isBlockLimitEnabled) GUI.color = Color.gray;
            else if (_totalRequired == _configuredLimit) GUI.color = Color.green;
            else GUI.color = Color.red;

            var limitText = _isBlockLimitEnabled ? _configuredLimit.ToString() : "∞";
            EditorGUILayout.LabelField($"Required: {_totalRequired} | Limit: {limitText}", EditorStyles.boldLabel);
            GUI.color = oldColor;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Auto-Test Progress", EditorStyles.boldLabel);

            var progress = _totalRequired > 0 ? (float)_spawnedCount / _totalRequired : 0f;
            var rect = GUILayoutUtility.GetRect(0, 20, GUILayout.ExpandWidth(true));
            EditorGUI.ProgressBar(rect, progress, $"{_spawnedCount} / {_totalRequired}");

            EditorGUILayout.LabelField($"{_objectLabel}: {_spawnedCount} / {_totalRequired}");

            var timeText = _isTimeLimitEnabled ? _remainingTime.ToString("F1") : "∞";
            EditorGUILayout.LabelField($"Time: {timeText}");
        }
    }
}
#endif