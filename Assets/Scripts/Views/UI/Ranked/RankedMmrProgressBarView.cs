using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Game.Data.Ranked;

namespace Game.Views.UI.Ranked
{
    public class RankedMmrProgressBarView : MonoBehaviour
    {
        [field: SerializeField] private Image FillImage { get; set; }
        [field: SerializeField] private TextMeshProUGUI MinMmrText { get; set; }
        [field: SerializeField] private TextMeshProUGUI MaxMmrText { get; set; }

        private Coroutine _animationCoroutine;
        private Tween _currentTween;

        private void Awake()
        {
            if (FillImage is not null)
            {
                FillImage.type = Image.Type.Filled;
                FillImage.fillMethod = Image.FillMethod.Horizontal;
                FillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            }
        }

        public void SetInitialMmr(int mmr)
        {
            var range = RankedMmrScale.GetRange(mmr);
            UpdateMmrText(range);
            if (FillImage is not null)
                FillImage.fillAmount = range.CalculateFill(mmr);
        }

        public void UpdateCurrentMmr(int mmr)
        {
            _currentTween?.Kill();
            _currentTween = null;

            if (_animationCoroutine is not null)
            {
                StopCoroutine(_animationCoroutine);
                _animationCoroutine = null;
            }

            var range = RankedMmrScale.GetRange(mmr);
            UpdateMmrText(range);

            if (FillImage is not null)
                FillImage.fillAmount = range.CalculateFill(mmr);
        }

        public void AnimateMmrChange(
            int oldMmr,
            int newMmr,
            float phaseDuration,
            float textSwapDuration,
            Ease ease,
            System.Action onComplete)
        {
            if (_animationCoroutine != null)
                StopCoroutine(_animationCoroutine);

            _animationCoroutine = StartCoroutine(AnimateMmrChangeCoroutine(
                oldMmr, newMmr, phaseDuration, textSwapDuration, ease, onComplete));
        }

        private IEnumerator AnimateMmrChangeCoroutine(
            int oldMmr,
            int newMmr,
            float phaseDuration,
            float textSwapDuration,
            Ease ease,
            System.Action onComplete)
        {
            var currentMmr = oldMmr;
            var direction = newMmr > oldMmr ? 1 : (newMmr < oldMmr ? -1 : 0);

            if (direction == 0)
            {
                onComplete?.Invoke();
                yield break;
            }

            while (true)
            {
                var currentRange = RankedMmrScale.GetSegmentRange(currentMmr, direction);
                var targetRange = RankedMmrScale.GetRange(newMmr);

                if (currentRange.Min == targetRange.Min && currentRange.Max == targetRange.Max)
                {
                    yield return AnimateWithinRange(currentMmr, newMmr, currentRange, phaseDuration, ease);
                    break;
                }

                if (RankedMmrScale.TryGetBreakpoint(currentMmr, direction, out var breakpoint))
                {
                    yield return AnimateWithinRange(currentMmr, breakpoint, currentRange, phaseDuration, ease);
                    currentMmr = breakpoint;

                    yield return new WaitForSeconds(textSwapDuration);
                    UpdateMmrText(RankedMmrScale.GetSegmentRange(currentMmr, direction));
                }
                else
                {
                    break;
                }
            }

            onComplete?.Invoke();
        }

        private IEnumerator AnimateWithinRange(int fromMmr, int toMmr, RankedMmrRange range, float duration, Ease ease)
        {
            if (FillImage is null) yield break;

            var fromFill = range.CalculateFill(fromMmr);
            var toFill = range.CalculateFill(toMmr);

            FillImage.fillAmount = fromFill;
            var currentFill = fromFill;

            _currentTween = DOTween.To(
                () => currentFill,
                x =>
                {
                    currentFill = x;
                    FillImage.fillAmount = x;
                },
                toFill,
                duration)
                .SetEase(ease);

            yield return _currentTween.WaitForCompletion();
        }

        private void UpdateMmrText(RankedMmrRange range)
        {
            if (MinMmrText is not null)
                MinMmrText.text = range.Min.ToString();

            if (MaxMmrText is not null)
                MaxMmrText.text = range.FormatMax();
        }

        private void OnDestroy()
        {
            _currentTween?.Kill();
            if (_animationCoroutine != null)
                StopCoroutine(_animationCoroutine);
        }
    }
}