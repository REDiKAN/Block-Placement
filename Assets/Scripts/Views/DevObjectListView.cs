using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;
using Game.Services.Registry;

namespace Game.Views
{
    public class DevObjectListView : MonoBehaviour
    {
        [field: SerializeField] private Transform _content;
        [field: SerializeField] private DevObjectListItemView _itemPrefab;

        [Inject] private IObjectRegistryService _registryService;
        private readonly Queue<DevObjectListItemView> _itemPool = new();
        private readonly List<DevObjectListItemView> _activeItems = new();
        private readonly CompositeDisposable _disposables = new();

        private void Start()
        {
            if (_itemPrefab is null || _content is null)
            {
                Debug.LogError($"[{nameof(DevObjectListView)}] Item Prefab or Content Transform is not assigned in the Inspector.");
                return;
            }

            _registryService.Objects.ObserveAdd()
                .Subscribe(_ => RebuildList())
                .AddTo(_disposables);

            _registryService.Objects.ObserveRemove()
                .Subscribe(_ => RebuildList())
                .AddTo(_disposables);

            RebuildList();
        }

        private void RebuildList()
        {
            if (_itemPrefab is null || _content is null) return;

            foreach (var item in _activeItems)
            {
                item.gameObject.SetActive(false);
                _itemPool.Enqueue(item);
            }
            _activeItems.Clear();

            foreach (var data in _registryService.Objects)
            {
                var item = GetPooledItem();
                if (item is null) continue;

                item.SetData(data);
                item.transform.SetParent(_content, false);
                item.gameObject.SetActive(true);
                _activeItems.Add(item);
            }
        }

        private DevObjectListItemView GetPooledItem()
        {
            if (_itemPool.Count > 0) return _itemPool.Dequeue();
            return Instantiate(_itemPrefab, _content);
        }

        private void OnDestroy() => _disposables.Dispose();
    }
}