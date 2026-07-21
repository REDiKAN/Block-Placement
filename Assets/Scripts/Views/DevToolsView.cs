using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Game.Data;
using Game.Services.Dev;
using Game.Services.Input;

namespace Game.Views
{
    public class DevToolsView : MonoBehaviour
    {
        [field: SerializeField] private GameObject _mainMenu;
        [field: SerializeField] private GameObject _blockSubMenu;
        [field: SerializeField] private GameObject _floorEditorPanel;
        [field: SerializeField] private GameObject _densityEditorPanel;
        [field: SerializeField] private GameObject _timeLimitPanel;
        [field: SerializeField] private Transform _blockListContent;
        [field: SerializeField] private Button _blockItemPrefab;
        [field: SerializeField] private GameObject _levelGeneratorPanel;
        [field: SerializeField] private Toggle _blockLimitToggle;
        [field: SerializeField] private Toggle _timeLimitToggle;
        [field: SerializeField] private TMP_InputField _timeLimitInput;

        [Inject] private readonly IDevModeService _devModeService;
        [Inject] private readonly IInputContextService _contextService;
        [Inject] private readonly BlockConfig[] _blockConfigs;

        private readonly List<Button> _spawnedBlockButtons = new();
        private readonly CompositeDisposable _disposables = new();

        private void Start()
        {
            PopulateBlockMenu();
            ShowMainMenu();

            if (_blockLimitToggle is not null)
            {
                _devModeService.IsBlockLimitEnabled
                    .Subscribe(value => _blockLimitToggle.isOn = value)
                    .AddTo(_disposables);

                _blockLimitToggle.OnValueChangedAsObservable()
                    .Subscribe(value => _devModeService.SetBlockLimitEnabled(value))
                    .AddTo(_disposables);
            }

            if (_timeLimitToggle is not null)
            {
                _devModeService.IsTimeLimitEnabled
                    .Subscribe(value => _timeLimitToggle.isOn = value)
                    .AddTo(_disposables);

                _timeLimitToggle.OnValueChangedAsObservable()
                    .Subscribe(value => _devModeService.SetTimeLimitEnabled(value))
                    .AddTo(_disposables);
            }

            if (_timeLimitInput is not null)
            {
                _devModeService.TimeLimitSeconds
                    .Subscribe(value =>
                    {
                        var newText = value.ToString("F1", CultureInfo.InvariantCulture);
                        if (_timeLimitInput.text != newText)
                            _timeLimitInput.text = newText;
                    })
                    .AddTo(_disposables);

                _timeLimitInput.onEndEdit.AsObservable()
                    .Subscribe(text =>
                    {
                        if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds) && seconds > 0f)
                            _devModeService.SetTimeLimitSeconds(seconds);
                    })
                    .AddTo(_disposables);
            }
        }

        private void PopulateBlockMenu()
        {
            if (_blockConfigs is null || _blockItemPrefab is null || _blockListContent is null) return;

            foreach (var config in _blockConfigs)
            {
                var button = Instantiate(_blockItemPrefab, _blockListContent, false);
                var textComponent = button.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent is not null)
                    textComponent.text = config.DisplayName;

                var capturedConfig = config;
                button.onClick.AddListener(() => SelectBlock(capturedConfig));
                _spawnedBlockButtons.Add(button);
            }
        }

        public void OpenLevelGenerator()
        {
            _mainMenu.SetActive(false);
            _blockSubMenu.SetActive(false);
            _floorEditorPanel.SetActive(false);
            _densityEditorPanel.SetActive(false);
            if (_timeLimitPanel is not null) _timeLimitPanel.SetActive(false);
            if (_levelGeneratorPanel is not null) _levelGeneratorPanel.SetActive(true);
            _contextService.SetContext(InputContext.None);
        }

        public void OpenBlockMode()
        {
            _mainMenu.SetActive(false);
            _floorEditorPanel.SetActive(false);
            _densityEditorPanel.SetActive(false);
            if (_timeLimitPanel is not null) _timeLimitPanel.SetActive(false);
            _blockSubMenu.SetActive(true);
        }

        public void OpenFloorEditor()
        {
            _mainMenu.SetActive(false);
            _blockSubMenu.SetActive(false);
            _densityEditorPanel.SetActive(false);
            if (_timeLimitPanel is not null) _timeLimitPanel.SetActive(false);
            _floorEditorPanel.SetActive(true);
            _contextService.SetContext(InputContext.None);
        }

        public void OpenShadowDensityEditor()
        {
            _mainMenu.SetActive(false);
            _blockSubMenu.SetActive(false);
            _floorEditorPanel.SetActive(false);
            if (_timeLimitPanel is not null) _timeLimitPanel.SetActive(false);
            _densityEditorPanel.SetActive(true);
            _contextService.SetContext(InputContext.None);
        }

        public void OpenTimeLimitSettings()
        {
            _mainMenu.SetActive(false);
            _blockSubMenu.SetActive(false);
            _floorEditorPanel.SetActive(false);
            _densityEditorPanel.SetActive(false);
            if (_timeLimitPanel is not null) _timeLimitPanel.SetActive(true);
            _contextService.SetContext(InputContext.None);
        }

        public void ShowMainMenu()
        {
            _blockSubMenu.SetActive(false);
            _floorEditorPanel.SetActive(false);
            _densityEditorPanel.SetActive(false);
            if (_timeLimitPanel is not null) _timeLimitPanel.SetActive(false);
            if (_levelGeneratorPanel is not null) _levelGeneratorPanel.SetActive(false);
            _mainMenu.SetActive(true);
        }

        public void ResetContextAndShowMainMenu()
        {
            ShowMainMenu();
            _contextService.SetContext(InputContext.None);
        }

        private void SelectBlock(BlockConfig config)
        {
            _devModeService.SetActiveBlockConfig(config);
            ShowMainMenu();
        }

        private void OnDestroy()
        {
            foreach (var button in _spawnedBlockButtons)
                if (button is not null) Destroy(button.gameObject);
        }
    }
}