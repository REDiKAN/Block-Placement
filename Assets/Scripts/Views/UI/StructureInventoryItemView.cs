using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Game.Data;

namespace Game.Views.UI
{
    public class StructureInventoryItemView : MonoBehaviour
    {
        [field: SerializeField] public Button Button { get; private set; }
        [field: SerializeField] public TextMeshProUGUI CountText { get; private set; }
        [field: SerializeField] public TextMeshProUGUI NameText { get; private set; }
        [field: SerializeField] public Image IconImage { get; private set; }

        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private Vector2 _baseAnchoredPosition;
        private Tween _introTween;
        private Tween _outroTween;
        private Tween _selectionTween;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup is null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        public void Initialize(Vector2 basePosition)
        {
            _baseAnchoredPosition = basePosition;
            _rectTransform.anchoredPosition = basePosition;
        }

        public void SetIcon(Sprite icon)
        {
            if (IconImage is null) return;
            if (icon is null)
            {
                IconImage.gameObject.SetActive(false);
                return;
            }
            IconImage.gameObject.SetActive(true);
            IconImage.sprite = icon;
        }

        public void PlayIntro(int index, StructureInventoryAnimationConfig config)
        {
            _introTween?.Kill();
            _canvasGroup.alpha = 0f;
            var startPos = _baseAnchoredPosition + new Vector2(0f, config.IntroOffsetY);
            _rectTransform.anchoredPosition = startPos;

            var delay = index * config.IntroStaggerDelay;

            var sequence = DOTween.Sequence();
            sequence.Join(_rectTransform.DOAnchorPos(_baseAnchoredPosition, config.IntroDuration).SetEase(config.IntroEase));
            sequence.Join(_canvasGroup.DOFade(1f, config.IntroDuration).SetEase(Ease.OutQuad));
            sequence.SetDelay(delay).SetAutoKill(true);
            _introTween = sequence;
        }

        public void PlayOutro(int index, StructureInventoryAnimationConfig config)
        {
            _outroTween?.Kill();
            var targetPos = _baseAnchoredPosition + new Vector2(0f, config.OutroOffsetY);
            var delay = index * config.OutroStaggerDelay;

            var sequence = DOTween.Sequence();
            sequence.Join(_rectTransform.DOAnchorPos(targetPos, config.OutroDuration).SetEase(config.OutroEase));
            sequence.Join(_canvasGroup.DOFade(0f, config.OutroDuration).SetEase(Ease.InQuad));
            sequence.SetDelay(delay).SetAutoKill(true);
            _outroTween = sequence;
        }

        public void Select(StructureInventoryAnimationConfig config)
        {
            _selectionTween?.Kill();
            var targetY = _baseAnchoredPosition.y + config.SelectionOffsetY;
            _selectionTween = _rectTransform.DOAnchorPosY(targetY, config.SelectionDuration)
                .SetEase(config.SelectionEase)
                .SetAutoKill(true);
        }

        public void Deselect(StructureInventoryAnimationConfig config)
        {
            _selectionTween?.Kill();
            _selectionTween = _rectTransform.DOAnchorPosY(_baseAnchoredPosition.y, config.SelectionDuration)
                .SetEase(config.SelectionEase)
                .SetAutoKill(true);
        }

        public void SetInteractable(bool isInteractable)
        {
            if (Button is null) return;
            Button.interactable = isInteractable;
            if (_canvasGroup is not null)
                _canvasGroup.alpha = isInteractable ? 1f : 0.5f;
        }

        private void OnDestroy()
        {
            _introTween?.Kill();
            _outroTween?.Kill();
            _selectionTween?.Kill();
        }
    }
}