using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

        public void UpdateCurrentMmr(int mmr)
        {
            if (_currentTween is not null)
            {
                _currentTween.Kill();
                _currentTween = null;
            }
            if (_animationCoroutine is not null)
            {
                StopCoroutine(_animationCoroutine);
                _animationCoroutine = null;
            }
            var archetype = GetArchetypeIndex(mmr);
            var range = GetArchetypeRange(archetype);
            UpdateMmrText(range);
            if (FillImage is not null)
                FillImage.fillAmount = CalculateFillAmount(mmr, range.Min, range.Max);
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
            var oldArchetype = GetArchetypeIndex(oldMmr);
            var newArchetype = GetArchetypeIndex(newMmr);
            if (oldArchetype == newArchetype)
            {
                yield return AnimateWithinArchetype(oldMmr, newMmr, phaseDuration, ease);
            }
            else
            {
                var direction = newArchetype > oldArchetype ? 1 : -1;
                var currentArchetype = oldArchetype;
                var currentMmr = oldMmr;
                while (currentArchetype != newArchetype)
                {
                    var nextArchetype = currentArchetype + direction;
                    var targetMmr = GetArchetypeBoundary(currentArchetype, direction);
                    yield return AnimateWithinArchetype(currentMmr, targetMmr, phaseDuration, ease);
                    currentMmr = targetMmr;
                    yield return new WaitForSeconds(textSwapDuration);
                    UpdateMmrText(GetArchetypeRange(nextArchetype));
                    currentArchetype = nextArchetype;
                }
                yield return AnimateWithinArchetype(currentMmr, newMmr, phaseDuration, ease);
            }
            onComplete?.Invoke();
        }

        private IEnumerator AnimateWithinArchetype(int fromMmr, int toMmr, float duration, Ease ease)
        {
            if (FillImage is null) yield break;
            var range = GetArchetypeRange(GetArchetypeIndex(fromMmr));
            var fromFill = CalculateFillAmount(fromMmr, range.Min, range.Max);
            var toFill = CalculateFillAmount(toMmr, range.Min, range.Max);
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

        private void UpdateMmrText((int Min, int Max) range)
        {
            if (MinMmrText is not null)
                MinMmrText.text = range.Min.ToString();
            if (MaxMmrText is not null)
                MaxMmrText.text = range.Max.ToString();
        }

        private int GetArchetypeIndex(int mmr) => mmr switch
        {
            < 500 => 0,
            < 1000 => 1,
            < 1500 => 2,
            < 2000 => 3,
            < 2500 => 4,
            _ => 5
        };

        private (int Min, int Max) GetArchetypeRange(int archetypeIndex) => archetypeIndex switch
        {
            0 => (0, 500),
            1 => (500, 1000),
            2 => (1000, 1500),
            3 => (1500, 2000),
            4 => (2000, 2500),
            _ => (2500, 3000)
        };

        private int GetArchetypeBoundary(int archetypeIndex, int direction)
        {
            var range = GetArchetypeRange(archetypeIndex);
            return direction > 0 ? range.Max : range.Min;
        }

        private float CalculateFillAmount(int mmr, int min, int max)
        {
            var range = max - min;
            if (range <= 0) return 0f;
            return Mathf.Clamp01((float)(mmr - min) / range);
        }

        public void SetInitialMmr(int mmr)
        {
            var archetype = GetArchetypeIndex(mmr);
            var range = GetArchetypeRange(archetype);
            UpdateMmrText(range);
            if (FillImage is not null)
                FillImage.fillAmount = CalculateFillAmount(mmr, range.Min, range.Max);
        }

        private void OnDestroy()
        {
            _currentTween?.Kill();
            if (_animationCoroutine != null)
                StopCoroutine(_animationCoroutine);
        }
    }
}