using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Game.Data;
using Game.Services.Progression;

namespace Game.Views.Menu
{
    public class CategoryButtonProgressView : MonoBehaviour
    {
        [field: SerializeField] private TextMeshProUGUI Label { get; set; }
        [field: SerializeField] private Button Button { get; set; }
        [field: SerializeField] private Image ProgressImage { get; set; }
        [field: SerializeField] private TextMeshProUGUI ProgressLabel { get; set; }

        private readonly Subject<CategoryConfig> _onClick = new();
        public IObservable<CategoryConfig> OnClick => _onClick;

        private readonly CompositeDisposable _disposables = new();
        private int _categoryId;
        private IProgressionService _progressionService;

        public void Initialize(CategoryConfig config, int categoryId, IProgressionService progressionService)
        {
            _categoryId = categoryId;
            _progressionService = progressionService;

            if (Label is not null) Label.text = config.Title;

            if (Button is not null)
            {
                Button.OnClickAsObservable()
                    .Subscribe(_ => _onClick.OnNext(config))
                    .AddTo(_disposables);
            }

            if (_progressionService is not null)
            {
                _progressionService.OnProgressionChanged
                    .Where(data => data.CategoryId == _categoryId)
                    .Subscribe(UpdateProgress)
                    .AddTo(_disposables);

                UpdateProgress(_progressionService.GetProgression(_categoryId));
            }
        }

        private void UpdateProgress(ProgressionData data)
        {
            if (ProgressImage is not null)
                ProgressImage.fillAmount = data.ProgressPercent / 100f;
            if (ProgressLabel is not null)
                ProgressLabel.text = $"{data.ProgressPercent:F0}%";
        }

        private void OnDestroy() => _disposables?.Dispose();
    }
}