using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Zenject;
using TMPro;
using Game.Core;
using Game.Data;
using Game.Services.Menu;
using Game.Services.Ranked;
using Game.Data.Ranked;

namespace Game.Views.Menu
{
    public class RankedMenuView : MonoBehaviour
    {
        [field: SerializeField] private Button PlayRankedBlocksButton { get; set; }
        [field: SerializeField] private Button PlayRankedStructuresButton { get; set; }
        [field: SerializeField] private TextMeshProUGUI MmrBlocksText { get; set; }
        [field: SerializeField] private TextMeshProUGUI MmrStructuresText { get; set; }
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

            var blocksMmr = 1000;
            var structsMmr = 1000;
            var blocksKey = "ranked_mmr";
            var structsKey = "ranked_mmr_structures";
            var defaultMmr = 1000;

            if (_rankedConfig is not null)
            {
                blocksKey = string.IsNullOrEmpty(_rankedConfig.MmrPlayerPrefsKey) ? "ranked_mmr" : _rankedConfig.MmrPlayerPrefsKey;
                structsKey = string.IsNullOrEmpty(_rankedConfig.MmrStructuresPlayerPrefsKey) ? "ranked_mmr_structures" : _rankedConfig.MmrStructuresPlayerPrefsKey;
                defaultMmr = _rankedConfig.StartingMmr;
            }

            if (_rankedModeService is not null)
            {
                RankedContext.CurrentMode = GameMode.Blocks;
                blocksMmr = _rankedModeService.LoadSavedMmr();

                RankedContext.CurrentMode = GameMode.Structures;
                structsMmr = _rankedModeService.LoadSavedMmr();

                _rankedModeService.CurrentMmr
                    .Subscribe(UpdateCurrentModeMmr)
                    .AddTo(_disposables);
            }
            else
            {
                blocksMmr = PlayerPrefs.GetInt(blocksKey, defaultMmr);
                structsMmr = PlayerPrefs.GetInt(structsKey, defaultMmr);
            }

            UpdateMmrDisplay(MmrBlocksText, blocksMmr, TypeRankedMode.Blocks);
            UpdateMmrDisplay(MmrStructuresText, structsMmr, TypeRankedMode.Structs);

            if (PlayRankedBlocksButton is not null)
            {
                PlayRankedBlocksButton.OnClickAsObservable()
                    .Subscribe(_ => StartMatch(GameMode.Blocks))
                    .AddTo(_disposables);
            }

            if (PlayRankedStructuresButton is not null)
            {
                PlayRankedStructuresButton.OnClickAsObservable()
                    .Subscribe(_ => StartMatch(GameMode.Structures))
                    .AddTo(_disposables);
            }

            if (BackButton is not null && _navigationService is not null)
            {
                BackButton.OnClickAsObservable()
                    .Subscribe(_ => _navigationService.NavigateTo(MenuView.CategoryList))
                    .AddTo(_disposables);
            }
        }

        private void UpdateCurrentModeMmr(int mmr)
        {
            if (RankedContext.CurrentMode == GameMode.Blocks)
                UpdateMmrDisplay(MmrBlocksText, mmr, TypeRankedMode.Blocks);
            else
                UpdateMmrDisplay(MmrStructuresText, mmr, TypeRankedMode.Structs);
        }

        private void UpdateMmrDisplay(TextMeshProUGUI textComponent, int mmr, TypeRankedMode typeRanked)
        {
            if (typeRanked == TypeRankedMode.Blocks)
                textComponent.text = $"Ranked Blocks MMR: {mmr}";
            else 
                textComponent.text = $"Ranked Structs MMR: {mmr}";
        }

        private void StartMatch(GameMode mode)
        {
            RankedContext.CurrentMode = mode;
            RankedContext.IsRankedModeActive = true;
            EndlessContext.IsEndlessModeActive = false;
            LevelContext.SelectedLevelConfig = null;
            SceneManager.LoadScene("GameScene");
        }

        private void OnDestroy() => _disposables?.Dispose();
    }
}

public enum TypeRankedMode
{
    Blocks, Structs
}