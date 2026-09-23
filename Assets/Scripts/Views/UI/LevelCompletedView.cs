using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Zenject;
using Game.Data;
using Game.Services.Progression;
using Game.Services.Input;

namespace Game.Views.UI
{
    public class LevelCompletedView : MonoBehaviour
    {
        [field: SerializeField] private CanvasGroup CanvasGroup { get; set; }
        [field: SerializeField] private RectTransform RectTransform { get; set; }
        [field: SerializeField] private TextMeshProUGUI MessageText { get; set; }
        [field: SerializeField] private Image LeftBarImage { get; set; }
        [field: SerializeField] private Image RightBarImage { get; set; }

        [Inject] private ILevelProgressionService _progressionService;
        [Inject] private IInputContextService _contextService;
        [Inject] private readonly LevelCompletedUIConfig _config;

        private readonly CompositeDisposable _disposables = new();
        private Sequence _animationSequence;
        private Tween _textAnimationTween;

        private Vector3[][] _originalVertices;
        private Vector3[] _originalCenters;
        private int _characterCount;
        private float _currentTime;
        private bool _isHiding;

        private void Start()
        {
            if (CanvasGroup is not null)
            {
                CanvasGroup.alpha = 0f;
                CanvasGroup.blocksRaycasts = false;
            }

            if (RectTransform is not null)
                RectTransform.localScale = Vector3.zero;

            if (MessageText is not null)
                MessageText.text = string.Empty;

            if (LeftBarImage is not null)
                LeftBarImage.fillAmount = 0f;

            if (RightBarImage is not null)
                RightBarImage.fillAmount = 0f;

            _progressionService.OnLevelCompletedMessage
                .Subscribe(ShowMessage)
                .AddTo(_disposables);

            _contextService.CurrentContext
                .Subscribe(ctx =>
                {
                    if (ctx != InputContext.LevelCompleted && ctx != InputContext.TimeExpired)
                        PlayHideAnimation();
                })
                .AddTo(_disposables);
        }

        private void ShowMessage(string message)
        {
            _isHiding = false;
            gameObject.SetActive(true);

            if (CanvasGroup is not null)
            {
                CanvasGroup.alpha = 1f;
                CanvasGroup.blocksRaycasts = true;
            }

            if (RectTransform is not null)
                RectTransform.localScale = Vector3.one;

            if (MessageText is not null)
            {
                MessageText.text = message;
                MessageText.ForceMeshUpdate();
                CacheVertices();

                _currentTime = 0f;
                UpdateVertices();
            }

            _animationSequence?.Kill();
            _textAnimationTween?.Kill();
            _animationSequence = DOTween.Sequence();

            if (LeftBarImage is not null)
            {
                LeftBarImage.fillAmount = 0f;
                _animationSequence.Join(LeftBarImage.DOFillAmount(1f, _config.BarFillDuration));
            }

            if (RightBarImage is not null)
            {
                RightBarImage.fillAmount = 0f;
                _animationSequence.Join(RightBarImage.DOFillAmount(1f, _config.BarFillDuration));
            }

            _animationSequence.AppendCallback(StartTextAnimation);
        }

        private void PlayHideAnimation()
        {
            if (_isHiding) return;
            if (!gameObject.activeInHierarchy) return;
            if (CanvasGroup is not null && CanvasGroup.alpha <= 0f) return;

            _isHiding = true;

            _animationSequence?.Kill();
            _textAnimationTween?.Kill();

            if (CanvasGroup is not null)
                CanvasGroup.blocksRaycasts = false;

            if (MessageText is null || _characterCount == 0)
            {
                HideInstantly();
                return;
            }

            _animationSequence = DOTween.Sequence();

            var totalTextDuration = (_characterCount - 1) * _config.HideTextStaggerDelay + _config.HideTextDuration;
            if (totalTextDuration <= 0f) totalTextDuration = 0.01f;

            _currentTime = totalTextDuration;

            var hideTween = DOTween.To(
                () => _currentTime,
                x =>
                {
                    _currentTime = x;
                    UpdateHideVertices();
                },
                0f,
                totalTextDuration
            ).SetEase(Ease.Linear).SetUpdate(true);

            _animationSequence.Append(hideTween);

            _animationSequence.AppendCallback(() =>
            {
                var barSequence = DOTween.Sequence();

                if (LeftBarImage is not null)
                    barSequence.Join(LeftBarImage.DOFillAmount(0f, _config.HideBarDuration).SetEase(_config.HideBarEase));

                if (RightBarImage is not null)
                    barSequence.Join(RightBarImage.DOFillAmount(0f, _config.HideBarDuration).SetEase(_config.HideBarEase));

                barSequence.OnComplete(HideInstantly);
            });
        }

        private void HideInstantly()
        {
            _isHiding = false;

            if (MessageText is not null)
                MessageText.text = string.Empty;

            if (LeftBarImage is not null)
                LeftBarImage.fillAmount = 0f;

            if (RightBarImage is not null)
                RightBarImage.fillAmount = 0f;

            if (CanvasGroup is not null)
            {
                CanvasGroup.alpha = 0f;
                CanvasGroup.blocksRaycasts = false;
            }

            gameObject.SetActive(false);
        }

        private void CacheVertices()
        {
            if (MessageText is null || MessageText.textInfo is null) return;
            var textInfo = MessageText.textInfo;
            _characterCount = textInfo.characterCount;
            _originalVertices = new Vector3[_characterCount][];
            _originalCenters = new Vector3[_characterCount];

            for (var i = 0; i < _characterCount; i++)
            {
                var charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible)
                {
                    _originalVertices[i] = null;
                    continue;
                }

                var matIndex = charInfo.materialReferenceIndex;
                var vIndex = charInfo.vertexIndex;
                var meshVerts = textInfo.meshInfo[matIndex].vertices;

                _originalVertices[i] = new Vector3[4];
                var center = Vector3.zero;

                for (var v = 0; v < 4; v++)
                {
                    _originalVertices[i][v] = meshVerts[vIndex + v];
                    center += meshVerts[vIndex + v];
                }

                _originalCenters[i] = center / 4f;
            }
        }

        private void StartTextAnimation()
        {
            if (MessageText is null || _characterCount == 0) return;

            var totalDuration = (_characterCount - 1) * _config.StaggerDelay + _config.LetterDuration;
            if (totalDuration <= 0f) totalDuration = 0.01f;

            _currentTime = 0f;
            _textAnimationTween = DOTween.To(
                () => _currentTime,
                x => _currentTime = x,
                totalDuration,
                totalDuration
            ).SetEase(Ease.Linear).SetUpdate(true).OnUpdate(UpdateVertices);
        }

        private void UpdateVertices()
        {
            if (MessageText is null || MessageText.textInfo is null) return;
            var textInfo = MessageText.textInfo;

            for (var i = 0; i < _characterCount; i++)
            {
                if (_originalVertices[i] is null) continue;

                var charInfo = textInfo.characterInfo[i];
                var matIndex = charInfo.materialReferenceIndex;
                var vIndex = charInfo.vertexIndex;
                var meshVerts = textInfo.meshInfo[matIndex].vertices;

                var letterStartTime = i * _config.StaggerDelay;
                var localProgress = Mathf.Clamp01((_currentTime - letterStartTime) / _config.LetterDuration);

                var scale = DOVirtual.EasedValue(0f, 1f, localProgress, _config.ScaleEase);
                var yOffset = DOVirtual.EasedValue(_config.OffsetY, 0f, localProgress, _config.PositionEase);

                var center = _originalCenters[i];

                for (var v = 0; v < 4; v++)
                {
                    var vert = _originalVertices[i][v];
                    vert = center + (vert - center) * scale;
                    vert.y += yOffset;
                    meshVerts[vIndex + v] = vert;
                }
            }

            MessageText.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
        }

        private void UpdateHideVertices()
        {
            if (MessageText is null || MessageText.textInfo is null) return;
            var textInfo = MessageText.textInfo;

            for (var i = 0; i < _characterCount; i++)
            {
                if (_originalVertices[i] is null) continue;

                var charInfo = textInfo.characterInfo[i];
                var matIndex = charInfo.materialReferenceIndex;
                var vIndex = charInfo.vertexIndex;
                var meshVerts = textInfo.meshInfo[matIndex].vertices;

                var reverseIndex = _characterCount - 1 - i;
                var letterEndTime = reverseIndex * _config.HideTextStaggerDelay;
                var localProgress = Mathf.Clamp01((_currentTime - letterEndTime) / _config.HideTextDuration);

                var scale = DOVirtual.EasedValue(0f, 1f, localProgress, _config.HideTextEase);
                var yOffset = DOVirtual.EasedValue(_config.OffsetY, 0f, localProgress, _config.HideTextPositionEase);

                var center = _originalCenters[i];

                for (var v = 0; v < 4; v++)
                {
                    var vert = _originalVertices[i][v];
                    vert = center + (vert - center) * scale;
                    vert.y += yOffset;
                    meshVerts[vIndex + v] = vert;
                }
            }

            MessageText.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
            _animationSequence?.Kill();
            _textAnimationTween?.Kill();
        }
    }
}