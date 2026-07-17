using System;
using UniRx;
using Zenject;
using Game.Core;
using Game.Data;
using Game.Services.Input;
using Game.Services.Shadow;

namespace Game.Services.Progression
{
    public class LevelProgressionService : ILevelProgressionService, IInitializable, IDisposable
    {
        public IObservable<string> OnLevelCompletedMessage => _onLevelCompletedMessage;
        public IObservable<LevelTransitionData> OnTransitionRequested => _onTransitionRequested;

        private readonly Subject<string> _onLevelCompletedMessage = new();
        private readonly Subject<LevelTransitionData> _onTransitionRequested = new();
        private readonly CompositeDisposable _disposables = new();

        private readonly IShadowValidationService _validationService;
        private readonly IInputService _inputService;
        private readonly IInputContextService _contextService;
        private readonly LevelCatalog _catalog;
        private readonly bool _isDeveloperMode;

        private const string NextLevelMessage = "Press Space to continue to the next level";
        private const string CatalogCompletedMessage = "Press Space to return to the main menu, you have completely completed the level catalog";

        public LevelProgressionService(
            IShadowValidationService validationService,
            IInputService inputService,
            IInputContextService contextService,
            LevelCatalog catalog,
            [Inject(Id = "IsDeveloperMode")] bool isDeveloperMode)
        {
            _validationService = validationService;
            _inputService = inputService;
            _contextService = contextService;
            _catalog = catalog;
            _isDeveloperMode = isDeveloperMode;
        }

        public void Initialize()
        {
            if (_isDeveloperMode) return;

            _validationService.OnLevelCompleted
                .Subscribe(_ => HandleLevelCompleted())
                .AddTo(_disposables);

            _inputService.OnNextLevelRequested
                .Subscribe(_ => HandleTransitionRequest())
                .AddTo(_disposables);
        }

        private void HandleLevelCompleted()
        {
            _contextService.SetContext(InputContext.LevelCompleted);

            var isLastLevel = _catalog is null || _catalog.Levels is null ||
                              LevelContext.SelectedLevelId >= _catalog.Levels.Length - 1;

            var message = isLastLevel ? CatalogCompletedMessage : NextLevelMessage;
            _onLevelCompletedMessage.OnNext(message);
        }

        private void HandleTransitionRequest()
        {
            if (_contextService.CurrentContext.Value != InputContext.LevelCompleted) return;

            var isLastLevel = _catalog is null || _catalog.Levels is null ||
                              LevelContext.SelectedLevelId >= _catalog.Levels.Length - 1;

            if (isLastLevel)
            {
                _onTransitionRequested.OnNext(new LevelTransitionData("MenuScene", -1));
            }
            else
            {
                var nextId = LevelContext.SelectedLevelId + 1;
                _onTransitionRequested.OnNext(new LevelTransitionData("GameScene", nextId));
            }
        }

        public void Dispose() => _disposables?.Dispose();
    }
}