using System.Collections.Generic;
using UnityEngine;
using Zenject;
using UniRx;
using Game.Services.Achievements;

namespace Game.Views.UI.Achievements
{
    public class AchievementListView : MonoBehaviour
    {
        [field: SerializeField] private Transform Content { get; set; }
        [field: SerializeField] private AchievementItemView ItemPrefab { get; set; }

        [Inject] private IAchievementService _achievementService;

        private readonly Queue<AchievementItemView> _itemPool = new();
        private readonly List<AchievementItemView> _activeItems = new();
        private readonly CompositeDisposable _disposables = new();

        private void Start()
        {
            if (_achievementService is null) return;

            _achievementService.Achievements.ObserveAdd()
                .Subscribe(_ => RebuildList())
                .AddTo(_disposables);

            _achievementService.Achievements.ObserveRemove()
                .Subscribe(_ => RebuildList())
                .AddTo(_disposables);

            RebuildList();
        }

        private void RebuildList()
        {
            if (Content is null) return;

            foreach (var item in _activeItems)
            {
                item.ResetState();
                item.gameObject.SetActive(false);
                _itemPool.Enqueue(item);
            }
            _activeItems.Clear();

            foreach (var data in _achievementService.Achievements)
            {
                var item = GetPooledItem(data.Config.UIPrefab);
                if (item is null) continue;

                item.Bind(data);
                item.transform.SetParent(Content, false);
                item.gameObject.SetActive(true);
                _activeItems.Add(item);
            }
        }

        private AchievementItemView GetPooledItem(AchievementItemView prefab)
        {
            var targetPrefab = prefab ?? ItemPrefab;
            if (targetPrefab is null) return null;

            if (_itemPool.Count > 0)
            {
                return _itemPool.Dequeue();
            }

            return Instantiate(targetPrefab, Content);
        }

        private void OnDestroy() => _disposables?.Dispose();
    }
}