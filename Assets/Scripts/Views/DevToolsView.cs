using System.Collections.Generic;
using TMPro;
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
        [field: SerializeField] private Transform _blockListContent;
        [field: SerializeField] private Button _blockItemPrefab;

        [Inject] private IDevModeService _devModeService;
        [Inject] private IInputContextService _contextService;
        [Inject] private BlockConfig[] _blockConfigs;

        private readonly List<Button> _spawnedBlockButtons = new();

        private void Start()
        {
            PopulateBlockMenu();
            ShowMainMenu();
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

        public void OpenBlockMode()
        {
            _mainMenu.SetActive(false);
            _floorEditorPanel.SetActive(false);
            _densityEditorPanel.SetActive(false);
            _blockSubMenu.SetActive(true);
        }

        public void OpenFloorEditor()
        {
            _mainMenu.SetActive(false);
            _blockSubMenu.SetActive(false);
            _densityEditorPanel.SetActive(false);
            _floorEditorPanel.SetActive(true);
            _contextService.SetContext(InputContext.None);
        }

        public void OpenShadowDensityEditor()
        {
            _mainMenu.SetActive(false);
            _blockSubMenu.SetActive(false);
            _floorEditorPanel.SetActive(false);
            _densityEditorPanel.SetActive(true);
            _contextService.SetContext(InputContext.None);
        }

        public void ShowMainMenu()
        {
            _blockSubMenu.SetActive(false);
            _floorEditorPanel.SetActive(false);
            _densityEditorPanel.SetActive(false);
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