using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Game.Data;
using Game.Services.Placement;

namespace Game.Views.UI
{
    public class StructureInventoryView : MonoBehaviour
    {
        [field: SerializeField] private Transform Content { get; set; }
        [field: SerializeField] private Button ItemPrefab { get; set; }
        [Inject] private LevelConfig _levelConfig;

        [Inject] private IStructurePlacementService _placementService;

        private readonly List<Button> _buttons = new();

        private void Start()
        {
            if (_levelConfig is null || _levelConfig.Mode != GameMode.Structures || _levelConfig.AvailableStructures is null)
            {
                gameObject.SetActive(false);
                return;
            }

            PopulateInventory();
        }

        private void PopulateInventory()
        {
            if (ItemPrefab is null || Content is null) return;

            foreach (var config in _levelConfig.AvailableStructures)
            {
                if (config is null) continue;
                var button = Instantiate(ItemPrefab, Content);
                var textComponent = button.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent is not null) textComponent.text = config.DisplayName;

                var capturedConfig = config;
                button.onClick.AddListener(() => _placementService.SelectStructure(capturedConfig));
                _buttons.Add(button);
            }
        }

        private void OnDestroy()
        {
            foreach (var button in _buttons)
                if (button is not null) Destroy(button.gameObject);
        }
    }
}