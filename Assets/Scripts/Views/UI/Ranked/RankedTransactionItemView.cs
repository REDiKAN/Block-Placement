using System;
using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;

namespace Game.Views.UI.Ranked
{
    public class RankedTransactionItemView : MonoBehaviour
    {
        [field: SerializeField] private TextMeshProUGUI LabelText { get; set; }
        [field: SerializeField] private TextMeshProUGUI PointsText { get; set; }
        [field: SerializeField] private CanvasGroup CanvasGroup { get; set; }
        [field: SerializeField] private RectTransform RectTransform { get; set; }

        public int OriginalPoints { get; private set; }

        private Sequence _transferSequence;

        public void Initialize(string label, int points, Color color)
        {
            OriginalPoints = points;
            if (LabelText is not null)
            {
                LabelText.text = label;
                LabelText.color = color;
            }
            if (PointsText is not null)
            {
                PointsText.text = points.ToString();
                PointsText.color = color;
            }
            if (CanvasGroup is not null)
            {
                CanvasGroup.alpha = 1f;
                CanvasGroup.blocksRaycasts = true;
            }
            if (RectTransform is not null)
                RectTransform.localScale = Vector3.one;
        }

        public IObservable<int> AnimateTransfer(float pointsPerSecond, float minDuration, Ease ease, float fadeOutDuration)
        {
            return Observable.Create<int>(observer =>
            {
                _transferSequence?.Kill();
                var absPoints = Mathf.Abs(OriginalPoints);
                var duration = Mathf.Max(absPoints / Mathf.Max(pointsPerSecond, 1f), minDuration);
                var currentDisplayValue = (float)OriginalPoints;

                _transferSequence = DOTween.Sequence();
                var valueTween = DOTween.To(
                    () => currentDisplayValue,
                    x =>
                    {
                        currentDisplayValue = x;
                        var rounded = Mathf.RoundToInt(currentDisplayValue);
                        observer.OnNext(rounded);

                        if (PointsText is not null)
                            PointsText.text = rounded.ToString();
                    },
                    0f,
                    duration).SetEase(ease);
                _transferSequence.Join(valueTween);

                _transferSequence.AppendCallback(() =>
                {
                    var fadeSequence = DOTween.Sequence();
                    if (CanvasGroup is not null)
                        fadeSequence.Join(CanvasGroup.DOFade(0f, fadeOutDuration));
                    if (RectTransform is not null)
                        fadeSequence.Join(RectTransform.DOScale(Vector3.zero, fadeOutDuration).SetEase(Ease.InBack));
                    fadeSequence.OnComplete(() =>
                    {
                        observer.OnCompleted();
                        gameObject.SetActive(false);
                    });
                });

                return Disposable.Create(() => _transferSequence?.Kill());
            });
        }

        public void ResetState()
        {
            _transferSequence?.Kill();
            OriginalPoints = 0;

            LabelText.text = string.Empty;
            LabelText.color = Color.white;

            PointsText.text = string.Empty;
            PointsText.color = Color.white;

            CanvasGroup.alpha = 1f;
            CanvasGroup.blocksRaycasts = true;

            RectTransform.localScale = Vector3.one;

            gameObject.SetActive(false);
        }

        private void OnDestroy() => _transferSequence?.Kill();
    }
}