using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;
using Game.Data;
using Game.Services.Placement;
using TMPro;

namespace Game.Views.UI
{
    public class StructureInventoryView : MonoBehaviour
    {
        [field: SerializeField] private Transform Content { get; set; }
        [field: SerializeField] private StructureInventoryItemView ItemPrefab { get; set; }

        [Inject] private LevelConfig _levelConfig;
        [Inject] private IStructurePlacementService _placementService;

        private readonly List<StructureInventoryItemView> _items = new();
        private readonly CompositeDisposable _disposables = new();

        private void Start()
        {
            if (_levelConfig is null || _levelConfig.Mode != GameMode.Structures || _levelConfig.AvailableStructures is null)
            {
                gameObject.SetActive(false);
                return;
            }

            PopulateInventory();

            _placementService.OnStructureCountChanged
                .Subscribe(UpdateItem)
                .AddTo(_disposables);
        }

        private void PopulateInventory()
        {
            if (ItemPrefab is null || Content is null) return;

            foreach (var spawnData in _levelConfig.AvailableStructures)
            {
                if (spawnData is null || spawnData.Config is null) continue;

                var item = Instantiate(ItemPrefab, Content);
                var textComponent = item.Button.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent is not null && textComponent != item.CountText)
                {
                    textComponent.text = spawnData.Config.DisplayName;
                }

                var capturedConfig = spawnData.Config;
                item.Button.onClick.AddListener(() => _placementService.SelectStructure(capturedConfig));

                UpdateItemCount(item, spawnData.MaxCount);
                _items.Add(item);
            }
        }

        private void UpdateItem((StructureConfig Config, int Remaining) data)
        {
            if (_levelConfig?.AvailableStructures is null) return;

            for (var i = 0; i < _levelConfig.AvailableStructures.Length; i++)
            {
                var spawnData = _levelConfig.AvailableStructures[i];
                if (spawnData is not null && spawnData.Config == data.Config && i < _items.Count)
                {
                    UpdateItemCount(_items[i], data.Remaining);
                    break;
                }
            }
        }

        private void UpdateItemCount(StructureInventoryItemView item, int remaining)
        {
            if (item is null || item.Button is null) return;

            if (remaining < 0)
            {
                if (item.CountText is not null) item.CountText.gameObject.SetActive(false);
                item.Button.interactable = true;
            }
            else
            {
                if (item.CountText is not null)
                {
                    item.CountText.gameObject.SetActive(true);
                    item.CountText.text = remaining.ToString();
                }
                item.Button.interactable = remaining > 0;
            }
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
            foreach (var item in _items)
            {
                if (item is not null) Destroy(item.gameObject);
            }
        }
    }
}