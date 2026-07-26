using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using UniRx;
using Game.Data;
using Game.Services.Menu;

namespace Game.Views.Menu
{
    public class CategoryListView : MonoBehaviour
    {
        [field: SerializeField] private Transform Content { get; set; }
        [field: SerializeField] private CategoryButtonView ButtonPrefab { get; set; }
        [field: SerializeField] private Button BackButton { get; set; }

        [Inject] private LevelCatalog _catalog;
        [Inject] private IMenuNavigationService _navigationService;
        [Inject] private ICategoryContextService _categoryContextService;

        private readonly List<CategoryButtonView> _buttons = new();
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
            if (_catalog?.Categories is null || ButtonPrefab is null || Content is null) return;

            foreach (var config in _catalog.Categories)
            {
                if (config is null) continue;

                var button = Instantiate(ButtonPrefab, Content);
                button.Initialize(config);
                _buttons.Add(button);

                var capturedConfig = config;
                button.OnClick
                    .Subscribe(_ => SelectCategory(capturedConfig))
                    .AddTo(_disposables);
            }
        }

        private void SelectCategory(CategoryConfig config)
        {
            _categoryContextService.SetCategory(config);
            _navigationService.NavigateTo(MenuView.LevelList);
        }

        private void OnDestroy() => _disposables?.Dispose();
    }
}