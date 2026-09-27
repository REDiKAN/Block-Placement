using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Game.Data;
using Game.Services.Placement;
using Game.Services.Input;
using Game.Services.Dev;

namespace Game.Views.UI
{
    public class StructureInventoryView : MonoBehaviour
    {
        [field: SerializeField] private Transform Content { get; set; }
        [field: SerializeField] private StructureInventoryItemView ItemPrefab { get; set; }
        [field: SerializeField] private ScrollRect ScrollRect { get; set; }
        [field: SerializeField] private RectTransform Viewport { get; set; }

        [Inject] private LevelConfig _levelConfig;
        [Inject] private IStructurePlacementService _placementService;
        [Inject] private IInputContextService _contextService;
        [Inject] private StructureInventoryAnimationConfig _animationConfig;
        [Inject] private IDevModeService _devModeService;
        [Inject(Id = "IsDeveloperMode")] private bool _isDeveloperMode;
        [InjectOptional] private StructureConfig[] _allStructureConfigs;

        private readonly List<StructureInventoryItemView> _items = new();
        private readonly CompositeDisposable _disposables = new();
        private GridLayoutGroup _layoutGroup;
        private ContentSizeFitter _sizeFitter;
        private StructureConfig _lastSelectedConfig;

        private void Start()
        {
            _layoutGroup = Content.GetComponent<GridLayoutGroup>();
            _sizeFitter = Content.GetComponent<ContentSizeFitter>();

            if (_isDeveloperMode && _devModeService.IsStructureMode.Value)
                PopulateDevInventory();
            else
                PopulateInventory(_levelConfig);

            _placementService.OnStructureCountChanged
                .Subscribe(UpdateItem)
                .AddTo(_disposables);
            _placementService.OnLevelChanged
                .Subscribe(OnLevelChanged)
                .AddTo(_disposables);
            _placementService.SelectedStructure
                .Subscribe(HandleSelectionChange)
                .AddTo(_disposables);
            _contextService.CurrentContext
                .Subscribe(HandleContextChange)
                .AddTo(_disposables);

            if (_isDeveloperMode)
            {
                _devModeService.IsStructureMode
                    .Subscribe(isStructureMode =>
                    {
                        ClearInventory();
                        if (isStructureMode)
                            PopulateDevInventory();
                        else
                            PopulateInventory(_levelConfig);
                    })
                    .AddTo(_disposables);
            }
        }

        private void HandleContextChange(InputContext context)
        {
            if (context == InputContext.LevelCompleted || context == InputContext.TimeExpired)
                PlayOutro();
        }

        private void OnLevelChanged(LevelConfig newConfig)
        {
            if (_isDeveloperMode && _devModeService.IsStructureMode.Value) return;
            ClearInventory();
            PopulateInventory(newConfig);
        }

        private void PopulateInventory(LevelConfig config)
        {
            if (_isDeveloperMode && _devModeService.IsStructureMode.Value)
            {
                PopulateDevInventory();
                return;
            }

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

            LayoutItems();
        }

        private void PopulateDevInventory()
        {
            if (_allStructureConfigs is null || _allStructureConfigs.Length == 0)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);
            if (ItemPrefab is null || Content is null) return;

            foreach (var config in _allStructureConfigs)
            {
                if (config is null) continue;
                var item = Instantiate(ItemPrefab, Content);
                if (item.NameText is not null)
                    item.NameText.text = config.DisplayName;
                item.SetIcon(config.Icon);
                var capturedConfig = config;
                item.Button.onClick.AddListener(() => _placementService.SelectStructure(capturedConfig));
                UpdateItemCount(item, -1);
                _items.Add(item);
            }

            LayoutItems();
        }

        private void LayoutItems()
        {
            var contentRect = Content as RectTransform;
            if (contentRect is null) return;
            var cellSize = _layoutGroup is not null ? _layoutGroup.cellSize : new Vector2(150f, 150f);
            var spacing = _layoutGroup is not null ? _layoutGroup.spacing : new Vector2(50f, 0f);

            if (_layoutGroup is not null) _layoutGroup.enabled = false;
            if (_sizeFitter is not null) _sizeFitter.enabled = false;

            var count = _items.Count;
            var totalWidth = count * cellSize.x + (count > 1 ? (count - 1) * spacing.x : 0f);

            contentRect.sizeDelta = new Vector2(totalWidth, contentRect.sizeDelta.y);

            var startX = -totalWidth / 2f + cellSize.x / 2f;

            for (var i = 0; i < count; i++)
            {
                var rt = _items[i].GetComponent<RectTransform>();
                if (rt is null) continue;
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = cellSize;
                var x = startX + i * (cellSize.x + spacing.x);
                _items[i].Initialize(new Vector2(x, 0f));
            }

            UpdateScrollability();
            PlayIntro();
        }

        private void UpdateScrollability()
        {
            if (ScrollRect is null || Content is null) return;
            var contentRect = Content as RectTransform;
            if (contentRect is null) return;

            var viewport = Viewport != null ? Viewport : ScrollRect.viewport;
            if (viewport is null) return;

            var viewportWidth = viewport.rect.width;
            var contentWidth = contentRect.sizeDelta.x;
            var isScrollable = contentWidth > viewportWidth;

            ScrollRect.enabled = isScrollable;

            if (!isScrollable)
            {
                contentRect.anchoredPosition = Vector2.zero;
            }
        }

        private void PlayIntro()
        {
            for (var i = 0; i < _items.Count; i++)
            {
                _items[i].PlayIntro(i, _animationConfig);
            }
        }

        private void PlayOutro()
        {
            for (var i = 0; i < _items.Count; i++)
            {
                _items[i].PlayOutro(i, _animationConfig);
            }
        }

        private void HandleSelectionChange(StructureConfig selected)
        {
            if (_lastSelectedConfig is not null && _lastSelectedConfig != selected)
            {
                var prevItem = FindItemByConfig(_lastSelectedConfig);
                prevItem?.Deselect(_animationConfig);
            }

            if (selected is not null)
            {
                var currentItem = FindItemByConfig(selected);
                currentItem?.Select(_animationConfig);
            }

            _lastSelectedConfig = selected;
        }

        private StructureInventoryItemView FindItemByConfig(StructureConfig config)
        {
            if (config is null) return null;
            for (var i = 0; i < _items.Count; i++)
            {
                if (_items[i] is null) continue;
                var itemName = _items[i].NameText is not null ? _items[i].NameText.text : string.Empty;
                if (itemName == config.DisplayName)
                    return _items[i];
            }
            return null;
        }

        private void ClearInventory()
        {
            foreach (var item in _items)
            {
                if (item is not null) Destroy(item.gameObject);
            }
            _items.Clear();
            _lastSelectedConfig = null;

            if (_layoutGroup is not null) _layoutGroup.enabled = true;
            if (_sizeFitter is not null) _sizeFitter.enabled = true;

            if (Content is RectTransform contentRect)
            {
                contentRect.sizeDelta = Vector2.zero;
                contentRect.anchoredPosition = Vector2.zero;
            }

            if (ScrollRect is not null) ScrollRect.enabled = false;
        }

        private void UpdateItem((StructureConfig Config, int Remaining) data)
        {
            var item = FindItemByConfig(data.Config);
            if (item is null) return;
            UpdateItemCount(item, data.Remaining);
            item.SetInteractable(data.Remaining != 0);
        }

        private void UpdateItemCount(StructureInventoryItemView item, int remaining)
        {
            if (item is null || item.Button is null) return;
            if (remaining < 0)
            {
                if (item.CountText is not null) item.CountText.gameObject.SetActive(false);
                item.Button.interactable = true;
                item.SetInteractable(true);
            }
            else
            {
                if (item.CountText is not null)
                {
                    item.CountText.gameObject.SetActive(true);
                    item.CountText.text = remaining.ToString();
                }
                item.SetInteractable(remaining > 0);
            }
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
            ClearInventory();
        }
    }
}