using DG.Tweening;
using TMPro;
using UnityEngine;
using UniRx;
using Zenject;
using Game.Services.Progression;
using Game.Services.Input;
using Game.Core;
using UnityEngine.SceneManagement;

namespace Game.Views.UI
{
    public class LevelCompletedView : MonoBehaviour
    {
        [field: SerializeField] private CanvasGroup CanvasGroup { get; set; }
        [field: SerializeField] private RectTransform RectTransform { get; set; }
        [field: SerializeField] private TextMeshProUGUI MessageText { get; set; }

        [Inject] private ILevelProgressionService _progressionService;
        [Inject] private IInputContextService _contextService;

        private readonly CompositeDisposable _disposables = new();
        private Sequence _animationSequence;

        private const float FadeDuration = 0.4f;
        private const float ScaleDuration = 0.5f;
        private const float TargetScale = 1.1f;

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

            _progressionService.OnLevelCompletedMessage
                .Subscribe(ShowMessage)
                .AddTo(_disposables);

            _progressionService.OnTransitionRequested
                .Subscribe(ExecuteTransition)
                .AddTo(_disposables);

            _contextService.CurrentContext
                .Subscribe(ctx =>
                {
                    if (ctx != InputContext.LevelCompleted && ctx != InputContext.TimeExpired)
                        HideMessage();
                })
                .AddTo(_disposables);
        }

        private void ShowMessage(string message)
        {
            if (MessageText is not null)
                MessageText.text = message;

            _animationSequence?.Kill();
            _animationSequence = DOTween.Sequence();

            if (CanvasGroup is not null)
            {
                CanvasGroup.blocksRaycasts = true;
                _animationSequence.Append(CanvasGroup.DOFade(1f, FadeDuration));
            }
            if (RectTransform is not null)
            {
                _animationSequence.Join(RectTransform.DOScale(TargetScale, ScaleDuration).SetEase(Ease.OutBack));
                _animationSequence.Append(RectTransform.DOScale(Vector3.one, 0.1f));
            }
        }

        private void HideMessage()
        {
            if (CanvasGroup is not null && CanvasGroup.alpha <= 0f) return;

            if (MessageText is not null)
                MessageText.text = string.Empty;

            _animationSequence?.Kill();
            _animationSequence = DOTween.Sequence();

            if (CanvasGroup is not null)
            {
                CanvasGroup.blocksRaycasts = false;
                _animationSequence.Append(CanvasGroup.DOFade(0f, FadeDuration));
            }
            if (RectTransform is not null)
            {
                _animationSequence.Join(RectTransform.DOScale(Vector3.zero, FadeDuration).SetEase(Ease.InBack));
            }
        }

        private void ExecuteTransition(LevelTransitionData data)
        {
            LevelContext.SelectedLevelId = data.NextLevelId;
            SceneManager.LoadScene(data.SceneName);
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
            _animationSequence?.Kill();
        }
    }
}