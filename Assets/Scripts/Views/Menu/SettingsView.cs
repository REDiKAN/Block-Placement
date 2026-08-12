using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Game.Services.Menu;
using Game.Services.Settings;
using Game.Services.Water;

namespace Game.Views.Menu
{
    public class SettingsView : MonoBehaviour
    {
        [field: SerializeField] private Button QualityButton { get; set; }
        [field: SerializeField] private TextMeshProUGUI QualityText { get; set; }
        [field: SerializeField] private Button ResolutionButton { get; set; }
        [field: SerializeField] private TextMeshProUGUI ResolutionText { get; set; }
        [field: SerializeField] private Button FullscreenButton { get; set; }
        [field: SerializeField] private TextMeshProUGUI FullscreenText { get; set; }
        [field: SerializeField] private Button WaterButton { get; set; }
        [field: SerializeField] private TextMeshProUGUI WaterText { get; set; }
        [field: SerializeField] private Button PreviewButton { get; set; }
        [field: SerializeField] private TextMeshProUGUI PreviewText { get; set; }
        [field: SerializeField] private Button BackButton { get; set; }

        [Inject] private ISettingsService _settingsService;
        [Inject] private IMenuNavigationService _navigationService;
        [Inject] private IWaterShaderService _waterShaderService;

        private readonly CompositeDisposable _disposables = new();
        private static readonly string[] QualityNames = { "Low", "Medium", "High" };

        private void Start()
        {
            _navigationService.CurrentView
                .Subscribe(view => gameObject.SetActive(view == MenuView.Settings))
                .AddTo(_disposables);

            if (QualityButton is not null)
                QualityButton.OnClickAsObservable()
                    .Subscribe(_ => _settingsService.CycleQuality())
                    .AddTo(_disposables);

            if (ResolutionButton is not null)
                ResolutionButton.OnClickAsObservable()
                    .Subscribe(_ => _settingsService.CycleResolution())
                    .AddTo(_disposables);

            if (FullscreenButton is not null)
                FullscreenButton.OnClickAsObservable()
                    .Subscribe(_ => _settingsService.CycleFullscreen())
                    .AddTo(_disposables);

            if (WaterButton is not null)
                WaterButton.OnClickAsObservable()
                    .Subscribe(_ => _waterShaderService.CycleConfig())
                    .AddTo(_disposables);

            if (PreviewButton is not null)
                PreviewButton.OnClickAsObservable()
                    .Subscribe(_ => _settingsService.CyclePreview())
                    .AddTo(_disposables);

            if (BackButton is not null)
                BackButton.OnClickAsObservable()
                    .Subscribe(_ => _navigationService.NavigateTo(MenuView.MainMenu))
                    .AddTo(_disposables);

            _settingsService.CurrentQualityLevel
                .Subscribe(level =>
                {
                    if (QualityText is not null)
                        QualityText.text = $"Quality: {QualityNames[level]}";
                })
                .AddTo(_disposables);

            _settingsService.CurrentResolution
                .Subscribe(resolution =>
                {
                    if (ResolutionText is not null && resolution is not null)
                        ResolutionText.text = $"Resolution: {resolution}";
                })
                .AddTo(_disposables);

            _settingsService.IsFullscreen
                .Subscribe(isFullscreen =>
                {
                    if (FullscreenText is not null)
                        FullscreenText.text = $"Fullscreen: {(isFullscreen ? "On" : "Off")}";
                })
                .AddTo(_disposables);

            _waterShaderService.CurrentConfig
                .Subscribe(config =>
                {
                    if (WaterText is not null && config is not null)
                        WaterText.text = $"Water: {config.DisplayName}";
                })
                .AddTo(_disposables);

            _settingsService.IsPreviewEnabled
                .Subscribe(isEnabled =>
                {
                    if (PreviewText is not null)
                        PreviewText.text = $"Preview: {(isEnabled ? "On" : "Off")}";
                })
                .AddTo(_disposables);
        }

        private void OnDestroy() => _disposables?.Dispose();
    }
}