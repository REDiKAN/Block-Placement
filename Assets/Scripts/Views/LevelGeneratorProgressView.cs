using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Views
{
    public class LevelGeneratorProgressView : MonoBehaviour
    {
        [field: SerializeField] private CanvasGroup CanvasGroup { get; set; }
        [field: SerializeField] private Slider ProgressBar { get; set; }
        [field: SerializeField] private TextMeshProUGUI StatusText { get; set; }

        private Tween _progressTween;

        public void Show()
        {
            if (CanvasGroup is not null)
            {
                CanvasGroup.alpha = 1f;
                CanvasGroup.blocksRaycasts = true;
            }
            UpdateProgress(0, 1);
        }

        public void Hide()
        {
            if (CanvasGroup is not null)
            {
                CanvasGroup.alpha = 0f;
                CanvasGroup.blocksRaycasts = false;
            }
            _progressTween?.Kill();
        }

        public void UpdateProgress(int current, int total)
        {
            if (StatusText is not null)
                StatusText.text = $"Processed {current} / {total} blocks";

            if (ProgressBar is not null)
            {
                _progressTween?.Kill();
                var targetValue = total > 0 ? (float)current / total : 0f;
                _progressTween = DOTween.To(() => ProgressBar.value, x => ProgressBar.value = x, targetValue, 0.2f)
                    .SetAutoKill(true);
            }
        }
    }
}