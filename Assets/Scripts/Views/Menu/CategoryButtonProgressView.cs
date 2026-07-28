using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
using Game.Data;
using Game.Services.Progression;

namespace Game.Views.Menu
{
    public class CategoryButtonProgressView : MonoBehaviour
    {
        [field: SerializeField] private TextMeshProUGUI Label { get; set; }
        [field: SerializeField] private Image IconImage { get; set; }
        [field: SerializeField] private Button Button { get; set; }
        [field: SerializeField] private Image ProgressImage { get; set; }
        [field: SerializeField] private TextMeshProUGUI ProgressLabel { get; set; }

        private readonly Subject<CategoryConfig> _onClick = new();
        private readonly Subject<bool> _onHover = new();
        private readonly Subject<float> _onProgress = new();

        public IObservable<CategoryConfig> OnClick => _onClick;

        private readonly CompositeDisposable _disposables = new();
        private int _categoryId;
        private IProgressionService _progressionService;

        public void Initialize(CategoryConfig config, int categoryId, IProgressionService progressionService)
        {
            _categoryId = categoryId;
            _progressionService = progressionService;

            if (Label is not null) Label.text = config.Title;

            if (IconImage is not null)
            {
                if (config.Icon is not null)
                {
                    IconImage.sprite = config.Icon;
                    IconImage.gameObject.SetActive(true);
                }
                else
                {
                    IconImage.gameObject.SetActive(false);
                }
            }

            if (Button is not null)
            {
                Button.OnClickAsObservable()
                    .Subscribe(_ => _onClick.OnNext(config))
                    .AddTo(_disposables);

                Button.OnPointerEnterAsObservable()
                    .Subscribe(_ => _onHover.OnNext(true))
                    .AddTo(_disposables);

                Button.OnPointerExitAsObservable()
                    .Subscribe(_ => _onHover.OnNext(false))
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
            var fillAmount = data.ProgressPercent / 100f;

            if (ProgressImage is not null)
                ProgressImage.fillAmount = fillAmount;

            if (ProgressLabel is not null)
                ProgressLabel.text = $"{data.ProgressPercent:F0}%";

            _onProgress.OnNext(fillAmount);
        }

        private void OnDestroy() => _disposables?.Dispose();
    }
}