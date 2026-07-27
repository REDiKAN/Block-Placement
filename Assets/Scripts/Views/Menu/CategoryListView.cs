using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using UniRx;
using Game.Data;
using Game.Services.Menu;
using Game.Services.Progression;

namespace Game.Views.Menu
{
    public class CategoryListView : MonoBehaviour
    {
        [field: SerializeField] private Transform Content { get; set; }
        [field: SerializeField] private CategoryButtonView ButtonPrefab { get; set; }
        [field: SerializeField] private CategoryButtonProgressView ButtonProgressPrefab { get; set; }
        [field: SerializeField] private Button BackButton { get; set; }

        [Inject] private LevelCatalog _catalog;
        [Inject] private IMenuNavigationService _navigationService;
        [Inject] private ICategoryContextService _categoryContextService;
        [Inject] private IProgressionService _progressionService;

        private readonly List<MonoBehaviour> _buttons = new();
        private readonly CompositeDisposable _disposables = new();

        private void Start()
        {
            if (BackButton is not null)
            {
                BackButton.OnClickAsObservable()
                    .Subscribe(_ => _navigationService.NavigateTo(MenuView.MainMenu))
                    .AddTo(_disposables);
            }

            _navigationService.CurrentView
                .Subscribe(view => gameObject.SetActive(view == MenuView.CategoryList))
                .AddTo(_disposables);

            PopulateCategories();
        }

        private void PopulateCategories()
        {
            if (_catalog?.Categories is null || Content is null) return;

            for (var i = 0; i < _catalog.Categories.Length; i++)
            {
                var config = _catalog.Categories[i];
                if (config is null) continue;

                var capturedIndex = i;

                if (config.IsSequential && ButtonProgressPrefab is not null)
                {
                    var button = Instantiate(ButtonProgressPrefab, Content);
                    button.Initialize(config, capturedIndex, _progressionService);
                    _buttons.Add(button);
                    button.OnClick
                        .Subscribe(_ => SelectCategory(config))
                        .AddTo(_disposables);
                }
                else if (ButtonPrefab is not null)
                {
                    var button = Instantiate(ButtonPrefab, Content);
                    button.Initialize(config);
                    _buttons.Add(button);
                    button.OnClick
                        .Subscribe(_ => SelectCategory(config))
                        .AddTo(_disposables);
                }
            }
        }

        private void SelectCategory(CategoryConfig config)
        {
            if (config.IsCustomGenerator)
            {
                _navigationService.NavigateTo(MenuView.CustomSettings);
                return;
            }
            _categoryContextService.SetCategory(config);
            _navigationService.NavigateTo(MenuView.LevelList);
        }

        private void OnDestroy() => _disposables?.Dispose();
    }
}