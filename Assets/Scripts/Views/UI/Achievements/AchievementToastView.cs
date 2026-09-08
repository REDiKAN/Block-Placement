using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Game.Services.Achievements;

namespace Game.Views.UI.Achievements
{
    public class AchievementToastView : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private Image _progressFill;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private float _slideDuration = 0.5f;
        [SerializeField] private float _holdDuration = 3f;
        [SerializeField] private float _offscreenOffset = 800f;
        [SerializeField] private float _progressFillDuration = 1f;

        [Inject] private IAchievementService _achievementService;

        private readonly Queue<AchievementRuntimeData> _queue = new();
        private readonly CompositeDisposable _disposables = new();
        private Sequence _currentSequence;
        private bool _isAnimating;
        private Vector2 _originalAnchoredPosition;

        private void Start()
        {
            if (_rectTransform is not null)
            {
                _originalAnchoredPosition = _rectTransform.anchoredPosition;
            }

            if (_canvasGroup is not null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.blocksRaycasts = false;
            }

            if (_progressFill is not null)
            {
                _progressFill.fillAmount = 0f;
            }

            if (_achievementService is not null)
            {
                _achievementService.OnAchievementUnlocked
                    .Subscribe(Enqueue)
                    .AddTo(_disposables);
            }
        }

        private void Enqueue(AchievementRuntimeData data)
        {
            _queue.Enqueue(data);

            if (!_isAnimating)
            {
                ProcessNext();
            }
        }

        private void ProcessNext()
        {
            if (_queue.Count == 0)
            {
                _isAnimating = false;
                return;
            }

            _isAnimating = true;
            var data = _queue.Dequeue();
            PlaySequence(data);
        }

        private void PlaySequence(AchievementRuntimeData data)
        {
            _currentSequence?.Kill();

            if (_iconImage is not null)
            {
                _iconImage.sprite = data.Config.Icon;
            }

            if (_titleText is not null)
            {
                _titleText.text = data.Config.Title;
            }

            if (_canvasGroup is null || _rectTransform is null)
            {
                ProcessNext();
                return;
            }

            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = false;

            var startPos = _originalAnchoredPosition + new Vector2(_offscreenOffset, 0f);
            _rectTransform.anchoredPosition = startPos;
            transform.localScale = Vector3.zero;

            if (_progressFill is not null)
            {
                _progressFill.fillAmount = 0f;
            }

            _currentSequence = DOTween.Sequence();

            _currentSequence.Append(_rectTransform.DOAnchorPos(_originalAnchoredPosition, _slideDuration).SetEase(Ease.OutCubic));

            var scaleSequence = DOTween.Sequence();
            scaleSequence.Append(transform.DOScale(1.15f, _slideDuration * 0.6f).SetEase(Ease.OutCubic));
            scaleSequence.Append(transform.DOScale(Vector3.one, _slideDuration * 0.4f).SetEase(Ease.InOutSine));
            _currentSequence.Join(scaleSequence);

            if (_progressFill is not null)
            {
                _currentSequence.Join(_progressFill.DOFillAmount(1f, _progressFillDuration).SetEase(Ease.Linear));
            }

            _currentSequence.AppendInterval(_holdDuration);

            _currentSequence.Append(_rectTransform.DOAnchorPos(startPos, _slideDuration).SetEase(Ease.InCubic));
            _currentSequence.Join(transform.DOScale(0f, _slideDuration).SetEase(Ease.InBack));

            _currentSequence.OnComplete(ProcessNext);
        }

        private void OnDestroy()
        {
            _currentSequence?.Kill();
            _disposables?.Dispose();
        }
    }
}