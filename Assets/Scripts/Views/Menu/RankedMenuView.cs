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
        [InjectOptional] private readonly RankedBlocksConfig _blocksConfig;
        [InjectOptional] private readonly RankedStructuresConfig _structuresConfig;

        private readonly CompositeDisposable _disposables = new();

        private void Start()
        {
            if (_navigationService is not null)
            {
                _navigationService.CurrentView
                    .Subscribe(view => gameObject.SetActive(view == MenuView.RankedMenu))
                    .AddTo(_disposables);
            }

            var blocksMmr = _blocksConfig?.StartingMmr ?? 1000;
            var structsMmr = _structuresConfig?.StartingMmr ?? 1000;
            var blocksKey = _blocksConfig?.MmrPlayerPrefsKey ?? "ranked_mmr";
            var structsKey = _structuresConfig?.MmrPlayerPrefsKey ?? "ranked_mmr_structures";

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
                blocksMmr = PlayerPrefs.GetInt(blocksKey, blocksMmr);
                structsMmr = PlayerPrefs.GetInt(structsKey, structsMmr);
            }

            UpdateMmrDisplay(MmrBlocksText, blocksMmr);
            UpdateMmrDisplay(MmrStructuresText, structsMmr);

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
                UpdateMmrDisplay(MmrBlocksText, mmr);
            else
                UpdateMmrDisplay(MmrStructuresText, mmr);
        }

        private void UpdateMmrDisplay(TextMeshProUGUI textComponent, int mmr)
        {
            if (textComponent is not null)
                textComponent.text = $"MMR: {mmr}";
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