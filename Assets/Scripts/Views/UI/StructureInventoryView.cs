using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;
using Game.Data;
using Game.Services.Placement;

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
            PopulateInventory(_levelConfig);

            _placementService.OnStructureCountChanged
                .Subscribe(UpdateItem)
                .AddTo(_disposables);

            _placementService.OnLevelChanged
                .Subscribe(OnLevelChanged)
                .AddTo(_disposables);
        }

        private void OnLevelChanged(LevelConfig newConfig)
        {
            ClearInventory();
            PopulateInventory(newConfig);
        }

        private void PopulateInventory(LevelConfig config)
        {
            if (config is null || config.Mode != GameMode.Structures || config.AvailableStructures is null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            if (ItemPrefab is null || Content is null) return;

            foreach (var spawnData in config.AvailableStructures)
            {
                if (spawnData is null || spawnData.Config is null) continue;

                var item = Instantiate(ItemPrefab, Content);
                if (item.NameText is not null)
                    item.NameText.text = spawnData.Config.DisplayName;

                item.SetIcon(spawnData.Config.Icon);

                var capturedConfig = spawnData.Config;
                item.Button.onClick.AddListener(() => _placementService.SelectStructure(capturedConfig));

                UpdateItemCount(item, spawnData.MaxCount);
                _items.Add(item);
            }
        }

        private void ClearInventory()
        {
            foreach (var item in _items)
            {
                if (item is not null) Destroy(item.gameObject);
            }
            _items.Clear();
        }

        private void UpdateItem((StructureConfig Config, int Remaining) data)
        {
            for (var i = 0; i < _items.Count; i++)
            {
                if (_items[i] is null) continue;

                var itemName = _items[i].NameText is not null ? _items[i].NameText.text : string.Empty;
                if (itemName == data.Config.DisplayName)
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
            ClearInventory();
        }
    }
}