using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Game.Core;
using Game.Data.Ranked;
using Game.Services.Input;
using Game.Services.Ranked;
using UnityEngine.SceneManagement;

namespace Game.Views.UI.Ranked
{
    public class RankedResultView : MonoBehaviour
    {
        [field: SerializeField] private GameObject RootObject { get; set; }
        [field: SerializeField] private CanvasGroup CanvasGroup { get; set; }
        [field: SerializeField] private TextMeshProUGUI CurrentMmrText { get; set; }
        [field: SerializeField] private RankedTransactionListView TransactionListView { get; set; }
        [field: SerializeField] private RankedMmrProgressBarView ProgressBarView { get; set; }
        [field: SerializeField] private Button NewMatchButton { get; set; }
        [field: SerializeField] private Button ReturnButton { get; set; }

        [Inject] private readonly IRankedModeService _rankedModeService;
        [Inject] private readonly RankedResultUIConfig _config;
        [Inject] private readonly IInputContextService _contextService;

        private readonly CompositeDisposable _disposables = new();
        private Sequence _showSequence;
        private Sequence _hideSequence;

        private void Awake()
        {
            if (RootObject is not null)
                RootObject.SetActive(false);

            if (CanvasGroup is not null)
            {
                CanvasGroup.alpha = 0f;
                CanvasGroup.blocksRaycasts = false;
                CanvasGroup.interactable = false;
            }

            if (NewMatchButton is not null)
            {
                NewMatchButton.OnClickAsObservable()
                    .Subscribe(_ => OnNewMatchClicked())
                    .AddTo(_disposables);
            }
            if (ReturnButton is not null)
            {
                ReturnButton.OnClickAsObservable()
                    .Subscribe(_ => OnReturnToMenuClicked())
                    .AddTo(_disposables);
            }

            _rankedModeService.OnMatchEnded
                .Subscribe(ShowResults)
                .AddTo(_disposables);
        }

        private void ShowResults(RankedMatchResult result)
        {
            if (RootObject is not null)
                RootObject.SetActive(true);

            SetButtonsInteractable(false);

            if (CurrentMmrText is not null)
                CurrentMmrText.text = result.OldMmr.ToString();

            if (ProgressBarView is not null)
                ProgressBarView.SetInitialMmr(result.OldMmr);

            if (TransactionListView is not null)
                TransactionListView.Initialize(result.OldMmr, _rankedModeService.LevelResults.Value, result);

            PlayShowAnimation()
                .OnComplete(() =>
                {
                    if (TransactionListView is null)
                    {
                        SetButtonsInteractable(true);
                        return;
                    }

                    TransactionListView.PlayCascadeAnimation()
                        .Subscribe(
                            currentMmr =>
                            {
                                if (CurrentMmrText is not null)
                                    CurrentMmrText.text = currentMmr.ToString();
                                if (ProgressBarView is not null)
                                    ProgressBarView.UpdateCurrentMmr(currentMmr);
                            },
                            () => SetButtonsInteractable(true))
                        .AddTo(_disposables);
                });
        }

        private Sequence PlayShowAnimation()
        {
            _showSequence?.Kill();
            if (CanvasGroup is not null)
            {
                CanvasGroup.alpha = 0f;
                CanvasGroup.blocksRaycasts = true;
                CanvasGroup.interactable = true;
            }
            _showSequence = DOTween.Sequence();
            if (CanvasGroup is not null)
            {
                _showSequence.Append(CanvasGroup.DOFade(1f, _config.ShowDuration)
                    .SetEase(_config.ShowEase));
                _showSequence.OnComplete(() =>
                {
                    if (CanvasGroup is null) return;
                    CanvasGroup.blocksRaycasts = true;
                    CanvasGroup.interactable = true;
                });
            }
            return _showSequence;
        }

        private Sequence PlayHideAnimation()
        {
            _hideSequence?.Kill();
            if (CanvasGroup is not null)
            {
                CanvasGroup.blocksRaycasts = false;
                CanvasGroup.interactable = false;
            }
            _hideSequence = DOTween.Sequence();
            if (CanvasGroup is not null)
            {
                _hideSequence.Append(CanvasGroup.DOFade(0f, _config.HideDuration)
                    .SetEase(_config.HideEase));
            }
            _hideSequence.OnComplete(() =>
            {
                if (RootObject is not null)
                    RootObject.SetActive(false);
            });
            return _hideSequence;
        }

        private void SetButtonsInteractable(bool interactable)
        {
            if (NewMatchButton is not null)
                NewMatchButton.interactable = interactable;
            if (ReturnButton is not null)
                ReturnButton.interactable = interactable;
        }

        private void OnNewMatchClicked()
        {
            SetButtonsInteractable(false);
            PlayHideAnimation()
                .OnComplete(() =>
                {
                    _contextService.SetContext(InputContext.Generating);
                    RankedContext.Reset();
                    _rankedModeService.StartMatch();
                });
        }

        private void OnReturnToMenuClicked()
        {
            SetButtonsInteractable(false);
            PlayHideAnimation()
                .OnComplete(() =>
                {
                    _contextService.SetContext(InputContext.None);
                    RankedContext.Reset();
                    SceneManager.LoadScene("MenuScene");
                });
        }

        private void OnDestroy()
        {
            _showSequence?.Kill();
            _hideSequence?.Kill();
            _disposables?.Dispose();
        }
    }
}