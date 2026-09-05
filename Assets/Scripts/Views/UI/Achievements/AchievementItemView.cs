using Game.Services.Achievements;
using System;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Views.UI.Achievements
{
    public class AchievementItemView : MonoBehaviour
    {
        [field: SerializeField] private TextMeshProUGUI TitleText { get; set; }
        [field: SerializeField] private Image ProgressBarFill { get; set; }
        [field: SerializeField] private TextMeshProUGUI ProgressText { get; set; }
        [field: SerializeField] private GameObject CompletedOverlay { get; set; }
        [field: SerializeField] private Button ClickArea { get; set; }

        public IObservable<AchievementRuntimeData> OnClick => _onClick;

        private readonly Subject<AchievementRuntimeData> _onClick = new();
        private readonly CompositeDisposable _disposables = new();
        private AchievementRuntimeData _boundData;

        private void Awake()
        {
            if (ClickArea is not null)
            {
                ClickArea.OnClickAsObservable()
                    .Subscribe(_ =>
                    {
                        if (_boundData is not null) _onClick.OnNext(_boundData);
                    })
                    .AddTo(this);
            }
        }

        public void Bind(AchievementRuntimeData data)
        {
            _disposables.Clear();
            _boundData = data;

            if (TitleText is not null) TitleText.text = data.Config.Title;

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
            _boundData = null;
            if (TitleText is not null) TitleText.text = string.Empty;
            if (ProgressBarFill is not null) ProgressBarFill.fillAmount = 0f;
            if (ProgressText is not null) ProgressText.text = string.Empty;
            if (CompletedOverlay is not null) CompletedOverlay.SetActive(false);
        }

        private void OnDestroy() => _disposables?.Dispose();
    }
}