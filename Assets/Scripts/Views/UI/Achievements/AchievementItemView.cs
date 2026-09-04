using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace Game.Views.UI.Achievements
{
    public class AchievementItemView : MonoBehaviour
    {
        [field: SerializeField] private TextMeshProUGUI TitleText { get; set; }
        [field: SerializeField] private TextMeshProUGUI DescriptionText { get; set; }
        [field: SerializeField] private Image ProgressBarFill { get; set; }
        [field: SerializeField] private TextMeshProUGUI ProgressText { get; set; }
        [field: SerializeField] private GameObject CompletedOverlay { get; set; }

        private readonly CompositeDisposable _disposables = new();

        public void Bind(Services.Achievements.AchievementRuntimeData data)
        {
            _disposables.Clear();

            if (TitleText is not null) TitleText.text = data.Config.Title;
            if (DescriptionText is not null) DescriptionText.text = data.Config.Description;

            if (ProgressBarFill is not null)
            {
                ProgressBarFill.type = Image.Type.Filled;
                ProgressBarFill.fillMethod = Image.FillMethod.Horizontal;
            }

            data.CurrentProgress
                .Subscribe(progress => UpdateProgressUI(progress, data.Config.TargetValue))
                .AddTo(_disposables);

            data.IsCompleted
                .Subscribe(isCompleted =>
                {
                    if (CompletedOverlay is not null) CompletedOverlay.SetActive(isCompleted);
                })
                .AddTo(_disposables);
        }

        private void UpdateProgressUI(int current, int target)
        {
            var ratio = target > 0 ? (float)current / target : 0f;
            if (ProgressBarFill is not null) ProgressBarFill.fillAmount = ratio;
            if (ProgressText is not null) ProgressText.text = $"{current}/{target}";
        }

        public void ResetState()
        {
            _disposables.Clear();
            if (TitleText is not null) TitleText.text = string.Empty;
            if (DescriptionText is not null) DescriptionText.text = string.Empty;
            if (ProgressBarFill is not null) ProgressBarFill.fillAmount = 0f;
            if (ProgressText is not null) ProgressText.text = string.Empty;
            if (CompletedOverlay is not null) CompletedOverlay.SetActive(false);
        }

        private void OnDestroy() => _disposables?.Dispose();
    }
}