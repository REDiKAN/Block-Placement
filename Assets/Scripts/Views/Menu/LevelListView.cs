using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using UniRx;
using Game.Data;
using Game.Services.Menu;
using Game.Core;
using UnityEngine.SceneManagement;

namespace Game.Views.Menu
{
    public class LevelListView : MonoBehaviour
    {
        [field: SerializeField] private Transform Content { get; set; }
        [field: SerializeField] private LevelButtonView ButtonPrefab { get; set; }
        [field: SerializeField] private Button BackButton { get; set; }

        [Inject] private LevelCatalog _catalog;
        [Inject] private IMenuNavigationService _navigationService;
        [Inject] private IPreviewService _previewService;

        private readonly List<LevelButtonView> _buttons = new();
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
                .Subscribe(view => gameObject.SetActive(view == MenuView.LevelList))
                .AddTo(_disposables);

            PopulateLevels();
        }

        private void PopulateLevels()
        {
            if (_catalog?.Levels is null || ButtonPrefab is null || Content is null) return;

            for (var i = 0; i < _catalog.Levels.Length; i++)
            {
                var config = _catalog.Levels[i];
                if (config is null) continue;

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
            LevelContext.SelectedLevelId = index;
            SceneManager.LoadScene("GameScene");
        }

        private void OnDestroy() => _disposables?.Dispose();
    }
}