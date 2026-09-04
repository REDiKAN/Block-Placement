using UnityEngine;
using UnityEngine.UI;
using Zenject;
using UniRx;
using Game.Services.Menu;

namespace Game.Views.Menu
{
    public class MainMenuView : MonoBehaviour
    {
        [field: SerializeField] private Button PlayButton { get; set; }
        [field: SerializeField] private Button AchievementsButton { get; set; }
        [field: SerializeField] private Button SettingsButton { get; set; }
        [field: SerializeField] private Button DevModeButton { get; set; }
        [field: SerializeField] private Button FeedbackButton { get; set; }
        [field: SerializeField] private string FeedbackUrl { get; set; } = "https://docs.google.com/forms/";

        [Inject] private IMenuNavigationService _navigationService;

        private readonly CompositeDisposable _disposables = new();

        private void Start()
        {
            if (DevModeButton is not null)
            {
                DevModeButton.interactable = false;
                var colors = DevModeButton.colors;
                colors.normalColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                DevModeButton.colors = colors;
            }

            if (AchievementsButton is not null && _navigationService is not null)
            {
                AchievementsButton.OnClickAsObservable()
                    .Subscribe(_ => _navigationService.NavigateTo(MenuView.Achievements))
                    .AddTo(_disposables);
            }

            if (PlayButton is not null && _navigationService is not null)
            {
                PlayButton.OnClickAsObservable()
                    .Subscribe(_ => _navigationService.NavigateTo(MenuView.CategoryList))
                    .AddTo(_disposables);
            }

            if (SettingsButton is not null && _navigationService is not null)
            {
                SettingsButton.OnClickAsObservable()
                    .Subscribe(_ => _navigationService.NavigateTo(MenuView.Settings))
                    .AddTo(_disposables);
            }

            if (FeedbackButton is not null && !string.IsNullOrWhiteSpace(FeedbackUrl))
            {
                FeedbackButton.OnClickAsObservable()
                    .Subscribe(_ => Application.OpenURL(FeedbackUrl))
                    .AddTo(_disposables);
            }

            if (_navigationService is not null)
            {
                _navigationService.CurrentView
                    .Subscribe(view => gameObject.SetActive(view == MenuView.MainMenu))
                    .AddTo(_disposables);
            }
        }

        private void OnDestroy() => _disposables?.Dispose();
    }
}