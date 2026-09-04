using UnityEngine;
using UnityEngine.UI;
using Zenject;
using UniRx;
using Game.Services.Menu;

namespace Game.Views.UI.Achievements
{
    public class AchievementsWindowView : MonoBehaviour
    {
        [field: SerializeField] private GameObject WindowRoot { get; set; }
        [field: SerializeField] private Button BackButton { get; set; }

        [Inject] private IMenuNavigationService _navigationService;

        private readonly CompositeDisposable _disposables = new();

        private void Start()
        {
            if (WindowRoot is not null) WindowRoot.SetActive(false);

            if (BackButton is not null && _navigationService is not null)
            {
                BackButton.OnClickAsObservable()
                    .Subscribe(_ => _navigationService.NavigateTo(MenuView.MainMenu))
                    .AddTo(_disposables);
            }

            if (_navigationService is not null)
            {
                _navigationService.CurrentView
                    .Subscribe(view =>
                    {
                        if (WindowRoot is not null)
                            WindowRoot.SetActive(view == MenuView.Achievements);
                    })
                    .AddTo(_disposables);
            }
        }

        private void OnDestroy() => _disposables?.Dispose();
    }
}