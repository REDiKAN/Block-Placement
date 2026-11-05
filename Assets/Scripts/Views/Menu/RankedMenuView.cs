using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Zenject;
using Game.Core;
using Game.Data.Ranked;
using Game.Services.Menu;

namespace Game.Views.Menu
{
    public class RankedMenuView : MonoBehaviour
    {
        [field: SerializeField] private Button PlayRankedButton { get; set; }
        [field: SerializeField] private TextMeshProUGUI MmrText { get; set; }
        [field: SerializeField] private Button BackButton { get; set; }
        [field: SerializeField] private RankedConfig RankedConfig { get; set; }

        [Inject] private readonly IMenuNavigationService _navigationService;

        private readonly CompositeDisposable _disposables = new();

        private void Start()
        {
            UpdateMmrDisplay();

            if (PlayRankedButton is not null)
            {
                PlayRankedButton.OnClickAsObservable()
                    .Subscribe(_ => StartRankedMatch())
                    .AddTo(_disposables);
            }

            if (BackButton is not null && _navigationService is not null)
            {
                BackButton.OnClickAsObservable()
                    .Subscribe(_ => _navigationService.NavigateTo(MenuView.MainMenu))
                    .AddTo(_disposables);
            }
        }

        private void UpdateMmrDisplay()
        {
            var key = RankedConfig is not null ? RankedConfig.MmrPlayerPrefsKey : "ranked_mmr";
            var defaultMmr = RankedConfig is not null ? RankedConfig.StartingMmr : 1000;
            var mmr = PlayerPrefs.GetInt(key, defaultMmr);

            if (MmrText is not null)
                MmrText.text = $"MMR: {mmr}";
        }

        private void StartRankedMatch()
        {
            var key = RankedConfig is not null ? RankedConfig.MmrPlayerPrefsKey : "ranked_mmr";
            var defaultMmr = RankedConfig is not null ? RankedConfig.StartingMmr : 1000;

            RankedContext.IsRankedModeActive = true;
            RankedContext.PlayerMmr = PlayerPrefs.GetInt(key, defaultMmr);
            EndlessContext.IsEndlessModeActive = false;
            LevelContext.SelectedLevelConfig = null;

            SceneManager.LoadScene("GameScene");
        }

        private void OnDestroy() => _disposables?.Dispose();
    }
}