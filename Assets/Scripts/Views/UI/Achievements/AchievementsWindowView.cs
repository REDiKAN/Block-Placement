using UnityEngine;
using UnityEngine.UI;
using Zenject;
using UniRx;
using Game.Services.Menu;
using TMPro;

namespace Game.Views.UI.Achievements
{
    public class AchievementsWindowView : MonoBehaviour
    {
        [field: SerializeField] private GameObject WindowRoot { get; set; }
        [field: SerializeField] private Button BackButton { get; set; }
        [field: SerializeField] private Button CompletedAchievementsButton { get; set; }
        [field: SerializeField] private AchievementListView ListView { get; set; }
        [field: SerializeField] private TextMeshProUGUI SharedDescriptionText { get; set; }

        [Inject] private IMenuNavigationService _navigationService;

        private readonly CompositeDisposable _disposables = new();
        private bool _isDescriptionVisible;

        private void Start()
        {
            if (WindowRoot is not null) WindowRoot.SetActive(false);

            if (BackButton is not null && _navigationService is not null)
            {
                BackButton.OnClickAsObservable()
                    .Subscribe(_ => _navigationService.NavigateTo(MenuView.MainMenu))
                    .AddTo(_disposables);
            }

            if (CompletedAchievementsButton is not null && _navigationService is not null)
            {
                CompletedAchievementsButton.OnClickAsObservable()
                    .Subscribe(_ => _navigationService.NavigateTo(MenuView.CompletedAchievements))
                    .AddTo(_disposables);
            }

            if (ListView is not null)
            {
                ListView.OnItemSelected
                    .Subscribe(data =>
                    {
                        if (SharedDescriptionText is not null && data?.Config is not null)
                        {
                            SharedDescriptionText.text = data.Config.Description;
                            SharedDescriptionText.gameObject.SetActive(true);
                            _isDescriptionVisible = true;
                        }
                    })
                    .AddTo(_disposables);
            }

            if (_navigationService is not null)
            {
                _navigationService.CurrentView
                    .Subscribe(view =>
                    {
                        if (WindowRoot is not null)
                        {
                            var isActive = view == MenuView.Achievements;
                            WindowRoot.SetActive(isActive);

                            if (isActive)
                            {
                                SharedDescriptionText.gameObject.SetActive(false);
                                _isDescriptionVisible = false;
                            }
                        }
                    })
                    .AddTo(_disposables);
            }
        }

        private void OnDestroy() => _disposables?.Dispose();
    }
}
