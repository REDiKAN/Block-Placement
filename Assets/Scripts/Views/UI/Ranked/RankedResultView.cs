using System;
using System.Collections;
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
        [field: SerializeField] private RectTransform WindowRectTransform { get; set; }
        [field: SerializeField] private CanvasGroup CanvasGroup { get; set; }
        [field: SerializeField] private Image FillImage { get; set; }
        [field: SerializeField] private TextMeshProUGUI MinMmrText { get; set; }
        [field: SerializeField] private TextMeshProUGUI MaxMmrText { get; set; }
        [field: SerializeField] private Button NewMatchButton { get; set; }
        [field: SerializeField] private Button ReturnButton { get; set; }

        [Inject] private readonly IRankedModeService _rankedModeService;
        [Inject] private readonly RankedResultUIConfig _config;
        [Inject] private readonly IInputContextService _contextService;

        private readonly CompositeDisposable _disposables = new();
        private Sequence _showSequence;
        private Sequence _hideSequence;
        private Tween _fillTween;
        private Coroutine _mmrAnimationCoroutine;
        private RankedMmrRange _displayedRange;

        private void Awake()
        {
            if (RootObject is not null)
                RootObject.SetActive(false);

            if (FillImage is not null)
            {
                FillImage.type = Image.Type.Filled;
                FillImage.fillMethod = Image.FillMethod.Horizontal;
                FillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            }

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
            ApplyRange(RankedMmrScale.GetRange(result.OldMmr), result.OldMmr);

            PlayShowAnimation()
                .OnComplete(() =>
                {
                    if (_mmrAnimationCoroutine != null)
                        StopCoroutine(_mmrAnimationCoroutine);

                    _mmrAnimationCoroutine = StartCoroutine(AnimateMmrChangeCoroutine(
                        result.OldMmr,
                        result.NewMmr,
                        () => SetButtonsInteractable(true)));
                });
        }

        private IEnumerator AnimateMmrChangeCoroutine(int oldMmr, int newMmr, Action onComplete)
        {
            var finished = false;

            try
            {
                var direction = newMmr >= oldMmr ? 1 : -1;
                var currentMmr = oldMmr;

                while (currentMmr != newMmr)
                {
                    var range = RankedMmrScale.GetSegmentRange(currentMmr, direction);
                    var target = ResolveSegmentTarget(currentMmr, newMmr, direction);

                    if (target == currentMmr) break;

                    yield return AnimateFill(currentMmr, target, range);
                    currentMmr = target;

                    if (currentMmr == newMmr) break;

                    yield return new WaitForSeconds(_config.MmrTextSwapDuration);
                    ApplyRange(RankedMmrScale.GetSegmentRange(currentMmr, direction), currentMmr);
                }

                yield return EnsureFinalRange(newMmr);
                finished = true;
            }
            finally
            {
                if (!finished)
                    UnityEngine.Debug.LogWarning("[RankedResultView] MMR animation aborted unexpectedly");

                onComplete?.Invoke();
            }
        }

        private static int ResolveSegmentTarget(int currentMmr, int newMmr, int direction)
        {
            if (!RankedMmrScale.TryGetBreakpoint(currentMmr, direction, out var breakpoint))
                return newMmr;

            return direction > 0
                ? breakpoint <= newMmr ? breakpoint : newMmr
                : breakpoint >= newMmr ? breakpoint : newMmr;
        }

        private IEnumerator EnsureFinalRange(int mmr)
        {
            var range = RankedMmrScale.GetRange(mmr);

            if (_displayedRange.Min == range.Min && _displayedRange.Max == range.Max)
                yield break;

            yield return new WaitForSeconds(_config.MmrTextSwapDuration);
            ApplyRange(range, mmr);
        }

        private IEnumerator AnimateFill(int fromMmr, int toMmr, RankedMmrRange range)
        {
            if (FillImage is null) yield break;

            var fromFill = range.CalculateFill(fromMmr);
            var toFill = range.CalculateFill(toMmr);
            var currentFill = fromFill;

            FillImage.fillAmount = fromFill;

            _fillTween = DOTween.To(
                () => currentFill,
                x =>
                {
                    currentFill = x;
                    FillImage.fillAmount = x;
                },
                toFill,
                _config.MmrPhaseDuration)
                .SetEase(_config.MmrEase);

            yield return _fillTween.WaitForCompletion();
        }

        private void ApplyRange(RankedMmrRange range, int mmr)
        {
            _displayedRange = range;

            if (MinMmrText is not null)
                MinMmrText.text = range.Min.ToString();

            if (MaxMmrText is not null)
                MaxMmrText.text = range.FormatMax();

            if (FillImage is not null)
                FillImage.fillAmount = range.CalculateFill(mmr);
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
            _fillTween?.Kill();

            if (_mmrAnimationCoroutine != null)
                StopCoroutine(_mmrAnimationCoroutine);

            _disposables?.Dispose();
        }
    }
}