using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Game.Services.Input;
using Game.Services.Ranked;

namespace Game.Views.UI.Ranked
{
    public class RankedPreRoundView : MonoBehaviour
    {
        [field: SerializeField] private GameObject RootObject { get; set; }
        [field: SerializeField] private Image LeftBarImage { get; set; }
        [field: SerializeField] private Image RightBarImage { get; set; }
        [field: SerializeField] private CanvasGroup ContentCanvasGroup { get; set; }
        [field: SerializeField] private Image[] CircleImages { get; set; }
        [field: SerializeField] private TextMeshProUGUI[] CircleTexts { get; set; }
        [field: SerializeField] private TextMeshProUGUI FormulaText { get; set; }

        [field: SerializeField] private Color WinColor { get; set; } = new(0.2f, 0.8f, 0.2f, 1f);
        [field: SerializeField] private Color LossColor { get; set; } = new(0.9f, 0.2f, 0.2f, 1f);
        [field: SerializeField] private Color PendingColor { get; set; } = new(0.5f, 0.5f, 0.5f, 0.6f);
        [field: SerializeField] private Color DefaultCircleColor { get; set; } = new(1f, 1f, 1f, 1f);

        [field: SerializeField] private float BarFillDuration { get; set; } = 0.3f;
        [field: SerializeField] private Ease BarFillEase { get; set; } = Ease.OutQuad;
        [field: SerializeField] private float BarHideDuration { get; set; } = 0.2f;
        [field: SerializeField] private Ease BarHideEase { get; set; } = Ease.InQuad;

        [Inject] private readonly IRankedModeService _rankedService;
        [Inject] private readonly IInputService _inputService;
        [Inject] private readonly IInputContextService _contextService;

        private readonly CompositeDisposable _disposables = new();
        private Sequence _barSequence;
        private bool _isVisible;
        private int _lastShownLevel = -1;

        private void Start()
        {
            if (RootObject is not null) RootObject.SetActive(false);
            if (LeftBarImage is not null) LeftBarImage.fillAmount = 0f;
            if (RightBarImage is not null) RightBarImage.fillAmount = 0f;

            if (ContentCanvasGroup is not null)
            {
                ContentCanvasGroup.alpha = 0f;
                ContentCanvasGroup.blocksRaycasts = false;
            }

            _rankedService.OnMatchStarted
                .Subscribe(_ => HandleMatchStarted())
                .AddTo(_disposables);

            _rankedService.LevelResults
                .CombineLatest(_rankedService.CurrentMatchLevel, (results, level) => (results, level))
                .Subscribe(data => HandleStateUpdate(data.results, data.level))
                .AddTo(_disposables);

            _rankedService.OnMatchEnded
                .Subscribe(_ => Hide())
                .AddTo(_disposables);

            _inputService.OnPrimaryClick
                .Subscribe(_ => HandleClick())
                .AddTo(_disposables);
        }

        private void HandleMatchStarted()
        {
            _lastShownLevel = -1;
            ShowSummary(0, _rankedService.LevelResults.Value);
        }

        private void HandleStateUpdate(IReadOnlyList<RankedLevelResult> results, int currentLevel)
        {
            if (currentLevel > _lastShownLevel && _rankedService.IsMatchActive.Value)
                ShowSummary(currentLevel - 1, results);
        }

        private void ShowSummary(int completedLevels, IReadOnlyList<RankedLevelResult> results)
        {
            if (_isVisible) return;
            _lastShownLevel = completedLevels;
            _contextService.SetContext(InputContext.RankedResult);
            UpdateUI(completedLevels, results);
            PlayShowAnimation();
        }

        private void UpdateUI(int completedLevels, IReadOnlyList<RankedLevelResult> results)
        {
            if (CircleImages is null || CircleTexts is null) return;
            var sum = 0;
            var parts = new List<string>();
            for (var i = 0; i < 5; i++)
            {
                if (i < completedLevels && i < results.Count)
                {
                    var points = results[i].PointsDelta;
                    sum += points;
                    if (CircleImages[i] is not null)
                        CircleImages[i].color = points >= 0 ? WinColor : LossColor;
                    if (CircleTexts[i] is not null)
                    {
                        CircleTexts[i].text = points.ToString();
                        CircleTexts[i].color = Color.white;
                    }
                    parts.Add(points >= 0 ? $"+{points}" : points.ToString());
                }
                else
                {
                    if (CircleImages[i] is not null)
                        CircleImages[i].color = DefaultCircleColor;
                    if (CircleTexts[i] is not null)
                    {
                        CircleTexts[i].text = $"lvl{i + 1}";
                        CircleTexts[i].color = PendingColor;
                    }
                    parts.Add($"lvl{i + 1}");
                }
            }
            if (FormulaText is not null)
            {
                var formulaParts = new List<string>();
                for (var i = 0; i < parts.Count; i++)
                {
                    var p = parts[i];
                    if (i == 0)
                    {
                        formulaParts.Add(p.StartsWith("+") ? p.Substring(1) : p);
                    }
                    else
                    {
                        if (p.StartsWith("-"))
                            formulaParts.Add($"- {p.Substring(1)}");
                        else if (p.StartsWith("+"))
                            formulaParts.Add($"+ {p.Substring(1)}");
                        else
                            formulaParts.Add($"+ {p}");
                    }
                }
                FormulaText.text = $"{string.Join(" ", formulaParts)} = {sum}";
            }
        }

        private void PlayShowAnimation()
        {
            _barSequence?.Kill();
            if (RootObject is not null) RootObject.SetActive(true);
            _barSequence = DOTween.Sequence();

            var barsSequence = DOTween.Sequence();
            if (LeftBarImage is not null)
            {
                LeftBarImage.fillAmount = 0f;
                barsSequence.Join(LeftBarImage.DOFillAmount(1f, BarFillDuration).SetEase(BarFillEase));
            }
            if (RightBarImage is not null)
            {
                RightBarImage.fillAmount = 0f;
                barsSequence.Join(RightBarImage.DOFillAmount(1f, BarFillDuration).SetEase(BarFillEase));
            }
            _barSequence.Append(barsSequence);

            if (ContentCanvasGroup is not null)
            {
                ContentCanvasGroup.alpha = 0f;
                ContentCanvasGroup.blocksRaycasts = false;
                _barSequence.Append(ContentCanvasGroup.DOFade(1f, BarFillDuration).SetEase(BarFillEase));
            }

            _barSequence.OnComplete(() =>
            {
                _isVisible = true;
                if (ContentCanvasGroup is not null) ContentCanvasGroup.blocksRaycasts = true;
            });
        }

        private void PlayHideAnimation()
        {
            _barSequence?.Kill();
            _barSequence = DOTween.Sequence();

            if (ContentCanvasGroup is not null)
                _barSequence.Append(ContentCanvasGroup.DOFade(0f, BarHideDuration).SetEase(BarHideEase));

            var hideBarsSequence = DOTween.Sequence();
            if (LeftBarImage is not null)
                hideBarsSequence.Join(LeftBarImage.DOFillAmount(0f, BarHideDuration).SetEase(BarHideEase));
            if (RightBarImage is not null)
                hideBarsSequence.Join(RightBarImage.DOFillAmount(0f, BarHideDuration).SetEase(BarHideEase));

            _barSequence.Append(hideBarsSequence);

            _barSequence.OnComplete(() =>
            {
                if (RootObject is not null) RootObject.SetActive(false);
                _isVisible = false;
                if (ContentCanvasGroup is not null) ContentCanvasGroup.blocksRaycasts = false;
                _rankedService.ConfirmPreRoundSummary();
            });
        }

        private void HandleClick()
        {
            if (_isVisible) PlayHideAnimation();
        }

        private void Hide()
        {
            if (_isVisible)
            {
                PlayHideAnimation();
            }
            else if (RootObject is not null)
            {
                RootObject.SetActive(false);
                if (ContentCanvasGroup is not null)
                {
                    ContentCanvasGroup.alpha = 0f;
                    ContentCanvasGroup.blocksRaycasts = false;
                }
            }

            _isVisible = false;
        }

        private void OnDestroy()
        {
            _barSequence?.Kill();
            _disposables?.Dispose();
        }
    }
}