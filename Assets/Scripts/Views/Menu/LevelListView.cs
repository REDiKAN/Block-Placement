using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using UniRx;
using Game.Data;
using Game.Services.Menu;
using Game.Services.Progression;
using Game.Core;
using UnityEngine.SceneManagement;
using TMPro;

namespace Game.Views.Menu
{
    public class LevelListView : MonoBehaviour
    {
        [field: SerializeField] private Transform Content { get; set; }
        [field: SerializeField] private LevelButtonView ButtonPrefab { get; set; }
        [field: SerializeField] private Button BackButton { get; set; }
        [field: SerializeField] private TextMeshProUGUI UnlockedLevelsLabel { get; set; }

        [Inject] private LevelCatalog _catalog;
        [Inject] private IMenuNavigationService _navigationService;
        [Inject] private IPreviewService _previewService;
        [Inject] private ICategoryContextService _categoryContextService;
        [Inject] private IProgressionService _progressionService;

        private readonly List<LevelButtonView> _buttons = new();
        private readonly CompositeDisposable _disposables = new();

        private void Start()
        {
            if (BackButton is not null)
            {
                BackButton.OnClickAsObservable()
                    .Subscribe(_ => _navigationService.NavigateTo(MenuView.CategoryList))
                    .AddTo(_disposables);
            }

            _navigationService.CurrentView
                .Subscribe(view => gameObject.SetActive(view == MenuView.LevelList))
                .AddTo(_disposables);

            _categoryContextService.SelectedCategory
                .Subscribe(PopulateLevels)
                .AddTo(_disposables);
        }

        private void PopulateLevels(CategoryConfig category)
        {
            foreach (var button in _buttons)
            {
                if (button is not null) Destroy(button.gameObject);
            }
            _buttons.Clear();

            if (category?.Levels is null || ButtonPrefab is null || Content is null) return;

            var categoryId = FindCategoryId(category);
            var unlockedCount = _progressionService.GetUnlockedCount(categoryId);
            var totalLevels = category.Levels.Length;

            if (UnlockedLevelsLabel is not null)
                UnlockedLevelsLabel.text = $"{unlockedCount} из {totalLevels} уровней открыто";

            for (var i = 0; i < category.Levels.Length; i++)
            {
                var config = category.Levels[i];
                if (config is null) continue;

                if (category.IsSequential && !_progressionService.IsLevelUnlocked(categoryId, i))
                    continue;

                var button = Instantiate(ButtonPrefab, Content);
                button.Initialize(config, i);
                _buttons.Add(button);

                var capturedIndex = i;
                button.OnClick
                    .Subscribe(_ => LoadLevel(capturedIndex))
                    .AddTo(_disposables);
                button.OnHover
                    .Subscribe(level => _previewService.ShowLevelPreview(level))
                    .AddTo(_disposables);
                button.OnHoverExit
                    .Subscribe(_ => _previewService.ClearPreview())
                    .AddTo(_disposables);
            }
        }

        private void LoadLevel(int index)
        {
            var category = _categoryContextService.SelectedCategory.Value;
            var categoryId = FindCategoryId(category);
            if (category is not null && category.IsSequential && !_progressionService.IsLevelUnlocked(categoryId, index))
                return;

            EndlessContext.IsEndlessModeActive = false;

            if (_catalog?.Categories is not null && category is not null)
            {
                for (var i = 0; i < _catalog.Categories.Length; i++)
                {
                    if (_catalog.Categories[i] == category)
                    {
                        LevelContext.SelectedCategoryId = i;
                        break;
                    }
                }
            }
            LevelContext.SelectedLevelId = index;
            SceneManager.LoadScene("GameScene");
        }

        private int FindCategoryId(CategoryConfig category)
        {
            if (_catalog?.Categories is null || category is null) return -1;
            for (var i = 0; i < _catalog.Categories.Length; i++)
            {
                if (_catalog.Categories[i] == category) return i;
            }
            return -1;
        }

        private void OnDestroy() => _disposables?.Dispose();
    }
}