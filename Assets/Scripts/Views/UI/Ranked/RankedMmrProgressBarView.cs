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
        private int _barMin;
        private int _barMax;

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

            _barMin = bounds.CurrentMin;
            _barMax = bounds.NextArchetype.HasValue ? bounds.NextMin : bounds.CurrentMax;
            if (_barMax <= _barMin) _barMax = _barMin + 1;

            var lossToPrev = bounds.PrevArchetype.HasValue ? _currentMmr - bounds.PrevMax : 0;
            var gainToNext = bounds.NextArchetype.HasValue ? bounds.NextMin - _currentMmr : 0;

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
                RightInfoText.text = bounds.NextArchetype.HasValue
                    ? $"(+{gainToNext}) {bounds.NextMin} ({bounds.NextArchetype.Value})"
                    : string.Empty;
            }
        }

        private float CalculateFill(int mmr)
        {
            if (_barMax <= _barMin) return 0f;
            var clamped = Mathf.Clamp(mmr, _barMin, _barMax);
            return (float)(clamped - _barMin) / (_barMax - _barMin);
        }

        private void OnDestroy()
        {
            _currentTween?.Kill();
        }
    }
}