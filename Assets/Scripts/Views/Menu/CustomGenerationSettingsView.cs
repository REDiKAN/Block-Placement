using System;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Game.Core;
using Game.Data;
using Game.Services.Generation;
using Game.Services.Menu;
using UnityEngine.SceneManagement;

namespace Game.Views.Menu
{
    public class CustomGenerationSettingsView : MonoBehaviour
    {
        [field: SerializeField] private Toggle HasFloorHolesToggle { get; set; }
        [field: SerializeField] private Toggle UseDensityToggle { get; set; }
        [field: SerializeField] private Toggle IsSymmetricalToggle { get; set; }
        [field: SerializeField] private Slider DifficultySlider { get; set; }
        [field: SerializeField] private TextMeshProUGUI DifficultyLabel { get; set; }
        [field: SerializeField] private Button BackButton { get; set; }
        [field: SerializeField] private Button PlayButton { get; set; }

        [Inject] private IMenuNavigationService _navigationService;

        private readonly CompositeDisposable _disposables = new();

        private void Start()
        {
            _navigationService.CurrentView
                .Subscribe(view => gameObject.SetActive(view == MenuView.CustomSettings))
                .AddTo(_disposables);

            if (DifficultySlider is not null)
            {
                DifficultySlider.minValue = 0;
                DifficultySlider.maxValue = 10;
                DifficultySlider.wholeNumbers = true;
                DifficultySlider.OnValueChangedAsObservable()
                    .Subscribe(val =>
                    {
                        if (DifficultyLabel is not null) DifficultyLabel.text = $"Difficulty: {(int)val}";
                    })
                    .AddTo(_disposables);
            }

            if (BackButton is not null)
            {
                BackButton.OnClickAsObservable()
                    .Subscribe(_ => _navigationService.NavigateTo(MenuView.CategoryList))
                    .AddTo(_disposables);
            }

            if (PlayButton is not null)
            {
                PlayButton.OnClickAsObservable()
                    .Subscribe(_ => Play())
                    .AddTo(_disposables);
            }
        }

        private void Play()
        {
            var settings = new CustomGenerationSettings(
                HasFloorHolesToggle is not null && HasFloorHolesToggle.isOn,
                UseDensityToggle is not null && UseDensityToggle.isOn,
                IsSymmetricalToggle is not null && IsSymmetricalToggle.isOn,
                DifficultySlider is not null ? (int)DifficultySlider.value : 5
            );

            EndlessContext.Settings = settings;
            EndlessContext.IsEndlessModeActive = true;
            SceneManager.LoadScene("GameScene");
        }

        private void OnDestroy() => _disposables?.Dispose();
    }
}