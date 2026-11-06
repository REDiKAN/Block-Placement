using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Zenject;
using TMPro;
using Game.Core;
using Game.Services.Menu;
using Game.Services.Ranked;
using Game.Data.Ranked;

namespace Game.Views.Menu
{
    public class RankedMenuView : MonoBehaviour
    {
        [field: SerializeField] private Button PlayRankedButton { get; set; }
        [field: SerializeField] private TextMeshProUGUI MmrText { get; set; }
        [field: SerializeField] private Button BackButton { get; set; }

        [Inject] private readonly IMenuNavigationService _navigationService;
        [InjectOptional] private readonly IRankedModeService _rankedModeService;
        [InjectOptional] private readonly RankedConfig _rankedConfig;

        private readonly CompositeDisposable _disposables = new();

        private void Start()
        {
            if (_navigationService is not null)
            {
                _navigationService.CurrentView
                    .Subscribe(view => gameObject.SetActive(view == MenuView.RankedMenu))
                    .AddTo(_disposables);
            }

            int currentMmr = 1000;
            string mmrKey = "ranked_mmr";
            int defaultMmr = 1000;

            if (_rankedConfig is not null)
            {
                mmrKey = string.IsNullOrEmpty(_rankedConfig.MmrPlayerPrefsKey) ? "ranked_mmr" : _rankedConfig.MmrPlayerPrefsKey;
                defaultMmr = _rankedConfig.StartingMmr;
            }

            if (_rankedModeService is not null)
            {
                currentMmr = _rankedModeService.LoadSavedMmr();
                _rankedModeService.CurrentMmr
                    .Subscribe(UpdateMmrDisplay)
                    .AddTo(_disposables);
            }
            else
            {
                currentMmr = PlayerPrefs.GetInt(mmrKey, defaultMmr);
            }

            UpdateMmrDisplay(currentMmr);

            if (PlayRankedButton is not null)
            {
                PlayRankedButton.OnClickAsObservable()
                    .Subscribe(_ => StartRankedMatch())
                    .AddTo(_disposables);
            }

            if (BackButton is not null && _navigationService is not null)
            {
                BackButton.OnClickAsObservable()
                    .Subscribe(_ => _navigationService.NavigateTo(MenuView.CategoryList))
                    .AddTo(_disposables);
            }
        }

        private void UpdateMmrDisplay(int mmr)
        {
            if (MmrText is not null)
                MmrText.text = $"MMR: {mmr}";
        }

        private void StartRankedMatch()
        {
            RankedContext.IsRankedModeActive = true;
            EndlessContext.IsEndlessModeActive = false;
            LevelContext.SelectedLevelConfig = null;
            SceneManager.LoadScene("GameScene");
        }

        private void OnDestroy() => _disposables?.Dispose();
    }
}