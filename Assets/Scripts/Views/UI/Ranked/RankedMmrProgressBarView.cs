using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Game.Services.Ranked;
using Game.Data.Ranked;

namespace Game.Views.UI.Ranked
{
    public class RankedMmrProgressBarView : MonoBehaviour
    {
        [field: SerializeField] private TextMeshProUGUI LeftInfoText { get; set; }
        [field: SerializeField] private Image FillImage { get; set; }
        [field: SerializeField] private TextMeshProUGUI CurrentArchetypeText { get; set; }
        [field: SerializeField] private TextMeshProUGUI RightInfoText { get; set; }

        [Inject] private readonly IArchetypeResolver _archetypeResolver;

        private Tween _currentTween;
        private int _currentMmr;

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
            _currentMmr = mmr;
            UpdateBoundsAndText();
            if (FillImage is not null)
                FillImage.fillAmount = CalculateFill(mmr);
        }

        public void UpdateCurrentMmr(int mmr)
        {
            _currentTween?.Kill();
            _currentTween = null;
            _currentMmr = mmr;
            UpdateBoundsAndText();
            if (FillImage is not null)
                FillImage.fillAmount = CalculateFill(mmr);
        }

        private void UpdateBoundsAndText()
        {
            var bounds = _archetypeResolver.GetMmrBounds(_currentMmr);
            var isMaxRank = !bounds.NextArchetype.HasValue;

            var lossToPrev = bounds.PrevArchetype.HasValue ? _currentMmr - bounds.PrevMax : 0;
            var gainToNext = isMaxRank ? 0 : bounds.NextMin - _currentMmr;

            if (LeftInfoText is not null)
            {
                LeftInfoText.text = bounds.PrevArchetype.HasValue
                    ? $"({bounds.PrevArchetype.Value}) {bounds.PrevMax} (-{lossToPrev})"
                    : string.Empty;
            }

            if (CurrentArchetypeText is not null)
                CurrentArchetypeText.text = $"({bounds.CurrentArchetype})";

            if (RightInfoText is not null)
            {
                RightInfoText.text = isMaxRank
                    ? "MAX"
                    : $"(+{gainToNext}) {bounds.NextMin} ({bounds.NextArchetype.Value})";
            }
        }

        private float CalculateFill(int mmr)
        {
            var bounds = _archetypeResolver.GetMmrBounds(mmr);

            if (!bounds.NextArchetype.HasValue)
                return 1f;

            var barMin = bounds.CurrentMin;
            var barMax = bounds.NextMin;

            if (barMax <= barMin)
                return 1f;

            var clamped = Mathf.Clamp(mmr, barMin, barMax);
            return (float)(clamped - barMin) / (barMax - barMin);
        }

        private void OnDestroy()
        {
            _currentTween?.Kill();
        }
    }
}