using DG.Tweening;
using Game.Data.Ranked;
using Game.Services.Ranked;
using System;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Game.Views.UI.Ranked
{
    public class RankedTransactionListView : MonoBehaviour
    {
        [field: SerializeField] private CanvasGroup CanvasGroup { get; set; }
        [field: SerializeField] private ScrollRect ScrollRect { get; set; }
        [field: SerializeField] private RectTransform Content { get; set; }
        [field: SerializeField] private Transform SpawnParent { get; set; }
        [field: SerializeField] private TextMeshProUGUI StartingMmrText { get; set; }

        [field: SerializeField] private RankedTransactionItemView WinItemPrefab { get; set; }
        [field: SerializeField] private RankedTransactionItemView LossItemPrefab { get; set; }
        [field: SerializeField] private RankedTransactionItemView BonusItemPrefab { get; set; }

        [Inject] private readonly RankedResultUIConfig _config;

        private readonly Queue<RankedTransactionItemView> _winPool = new();
        private readonly Queue<RankedTransactionItemView> _lossPool = new();
        private readonly Queue<RankedTransactionItemView> _bonusPool = new();
        private readonly List<RankedTransactionItemView> _activeItems = new();

        private int _startMmr;
        private readonly Color _winColor = new(0.2f, 0.8f, 0.2f, 1f);
        private readonly Color _lossColor = new(0.9f, 0.2f, 0.2f, 1f);
        private readonly Color _bonusColor = new(1f, 0.85f, 0.2f, 1f);

        public void Initialize(int startMmr, IReadOnlyList<RankedLevelResult> results, RankedMatchResult matchResult)
        {
            _startMmr = startMmr;
            ClearItems();

            if (StartingMmrText is not null)
                StartingMmrText.text = $"Starting MMR: {startMmr}";

            if (results is not null)
            {
                foreach (var result in results)
                {
                    var isWin = result.IsCompleted;
                    var status = isWin ? "Win" : "Defeat";
                    var label = $"{status} lvl {result.MatchLevel}";
                    var color = isWin ? _winColor : _lossColor;
                    var prefab = isWin ? WinItemPrefab : LossItemPrefab;
                    var type = isWin ? TransactionType.Win : TransactionType.Loss;

                    AddItem(prefab, type, label, result.PointsDelta, color);
                }
            }

            if (matchResult.Level5Bonus > 0)
                AddItem(BonusItemPrefab, TransactionType.Bonus, "Level 5 Bonus", matchResult.Level5Bonus, _bonusColor);

            if (matchResult.PerfectRunBonus > 0)
                AddItem(BonusItemPrefab, TransactionType.Bonus, "Perfect Run Bonus", matchResult.PerfectRunBonus, _bonusColor);
        }

        public IObservable<int> PlayCascadeAnimation()
        {
            return Observable.Create<int>(observer =>
            {
                if (CanvasGroup is not null)
                    CanvasGroup.DOFade(1f, _config.ListFadeInDuration);

                var currentMmr = _startMmr;
                var index = 0;
                var compositeDisposable = new CompositeDisposable();

                Action processNext = null;
                processNext = () =>
                {
                    if (index >= _activeItems.Count)
                    {
                        observer.OnCompleted();
                        return;
                    }

                    var item = _activeItems[index];
                    item.AnimateTransfer(
                        _config.PointsPerSecond,
                        _config.MinElementDuration,
                        _config.CascadeEase,
                        _config.ElementFadeOutDuration)
                        .Subscribe(
                            remainingDelta => observer.OnNext(currentMmr + item.OriginalPoints - remainingDelta),
                            () =>
                            {
                                currentMmr += item.OriginalPoints;
                                observer.OnNext(currentMmr);
                                ReturnToPool(item);
                                if (Content is not null)
                                    LayoutRebuilder.ForceRebuildLayoutImmediate(Content);
                                processNext();
                            })
                        .AddTo(compositeDisposable);
                };

                processNext();
                return compositeDisposable;
            });
        }

        private void AddItem(RankedTransactionItemView prefab, TransactionType type, string label, int points, Color color)
        {
            if (prefab is null) return;

            var item = GetPooledItem(prefab, type);
            if (item is null) return;

            item.Initialize(label, points, color);
            item.gameObject.SetActive(true);

            var parent = SpawnParent != null ? SpawnParent : Content;
            if (parent is not null && item.transform.parent != parent)
                item.transform.SetParent(parent, false);

            _activeItems.Add(item);
        }

        private void ReturnToPool(RankedTransactionItemView item)
        {
            if (item is null) return;
            item.ResetState();
            _activeItems.Remove(item);
            GetPool(item.Type).Enqueue(item);
        }

        private RankedTransactionItemView GetPooledItem(RankedTransactionItemView prefab, TransactionType type)
        {
            var pool = GetPool(type);
            if (pool.Count > 0)
                return pool.Dequeue();

            var newItem = Instantiate(prefab, SpawnParent);
            newItem.ResetState();
            return newItem;
        }

        private Queue<RankedTransactionItemView> GetPool(TransactionType type) => type switch
        {
            TransactionType.Win => _winPool,
            TransactionType.Loss => _lossPool,
            TransactionType.Bonus => _bonusPool,
            _ => _winPool
        };

        private void ClearItems()
        {
            foreach (var item in _activeItems)
            {
                if (item is null) continue;
                item.ResetState();
                GetPool(item.Type).Enqueue(item);
            }
            _activeItems.Clear();

            if (CanvasGroup is not null)
                CanvasGroup.alpha = 0f;
        }

        public void ResetState() => ClearItems();
    }
}