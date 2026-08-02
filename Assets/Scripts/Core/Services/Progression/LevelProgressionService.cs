using System;
using UniRx;
using Zenject;
using Game.Core;
using Game.Data;
using Game.Services.Animation;
using Game.Services.Generation;
using Game.Services.Input;
using Game.Services.Placement;
using Game.Services.Shadow;
using Game.Services.Time;

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
        private readonly ITimeLimitService _timeLimitService;
        private readonly IProgressionService _progressionService;
        private readonly LevelCatalog _catalog;
        private readonly IGenerationContext _generationContext;
        private readonly IEndlessGeneratorService _endlessGenerator;
        private readonly IBlockPlacementService _placementService;
        private readonly ILevelIntroAnimationService _levelIntroAnimationService;
        private readonly bool _isDeveloperMode;

        private bool _isLevelReady;

        private const string NextLevelMessage = "Press Space to continue to the next level";
        private const string CatalogCompletedMessage = "Press Space to return to the main menu, you have completely completed the level catalog";
        private const string TimeExpiredMessage = "Time's up, press Space to restart";
        private const string EndlessLevelCompletedMessage = "Press Space to generate next level";

        public LevelProgressionService(
            IShadowValidationService validationService,
            IInputService inputService,
            IInputContextService contextService,
            ITimeLimitService timeLimitService,
            IProgressionService progressionService,
            LevelCatalog catalog,
            IGenerationContext generationContext,
            IEndlessGeneratorService endlessGenerator,
            IBlockPlacementService placementService,
            ILevelIntroAnimationService levelIntroAnimationService,
            [Inject(Id = "IsDeveloperMode")] bool isDeveloperMode)
        {
            _validationService = validationService;
            _inputService = inputService;
            _contextService = contextService;
            _timeLimitService = timeLimitService;
            _progressionService = progressionService;
            _catalog = catalog;
            _generationContext = generationContext;
            _endlessGenerator = endlessGenerator;
            _placementService = placementService;
            _levelIntroAnimationService = levelIntroAnimationService;
            _isDeveloperMode = isDeveloperMode;
        }

        public void Initialize()
        {
            if (_isDeveloperMode)
            {
                _isLevelReady = true;
                return;
            }

            _validationService.OnLevelCompleted
                .Subscribe(_ => HandleLevelCompleted())
                .AddTo(_disposables);

            _timeLimitService.OnTimeExpired
                .Subscribe(_ => HandleTimeExpired())
                .AddTo(_disposables);

            _inputService.OnNextLevelRequested
                .Subscribe(_ => HandleTransitionRequest())
                .AddTo(_disposables);

            _levelIntroAnimationService.OnAnimationCompleted
                .Subscribe(_ =>
                {
                    _isLevelReady = true;
                    _validationService.ForceRevalidate();
                })
                .AddTo(_disposables);

            _levelIntroAnimationService.Play();
        }

        public void RequestRestart() =>
            _onTransitionRequested.OnNext(new LevelTransitionData("GameScene", LevelContext.SelectedLevelId));

        private void HandleLevelCompleted()
        {
            if (!_isLevelReady) return;

            _contextService.SetContext(InputContext.LevelCompleted);

            if (_generationContext.IsEndlessModeActive.Value)
            {
                _onLevelCompletedMessage.OnNext(EndlessLevelCompletedMessage);
                return;
            }

            var category = GetActiveCategory();
            var isLastLevel = category is null || category.Levels is null || LevelContext.SelectedLevelId >= category.Levels.Length - 1;
            _onLevelCompletedMessage.OnNext(isLastLevel ? CatalogCompletedMessage : NextLevelMessage);
        }

        private void HandleTimeExpired()
        {
            if (!_isLevelReady) return;

            _contextService.SetContext(InputContext.TimeExpired);
            _onLevelCompletedMessage.OnNext(TimeExpiredMessage);
        }

        private void HandleTransitionRequest()
        {
            var currentContext = _contextService.CurrentContext.Value;

            if (currentContext == InputContext.TimeExpired)
            {
                RequestRestart();
                return;
            }

            if (currentContext != InputContext.LevelCompleted) return;

            if (_generationContext.IsEndlessModeActive.Value)
            {
                _placementService.ClearAll();
                _endlessGenerator.GenerateNext();
                _validationService.ForceRevalidate();
                _contextService.SetContext(InputContext.None);
                return;
            }

            _progressionService.MarkLevelCompleted(LevelContext.SelectedCategoryId, LevelContext.SelectedLevelId);

            var category = GetActiveCategory();
            var isLastLevel = category is null || category.Levels is null || LevelContext.SelectedLevelId >= category.Levels.Length - 1;

            if (isLastLevel)
            {
                LevelContext.SelectedCategoryId = 0;
                LevelContext.SelectedLevelId = 0;
                _onTransitionRequested.OnNext(new LevelTransitionData("MenuScene", -1));
            }
            else
            {
                _onTransitionRequested.OnNext(new LevelTransitionData("GameScene", LevelContext.SelectedLevelId + 1));
            }
        }

        private CategoryConfig GetActiveCategory()
        {
            if (_catalog?.Categories is null ||
                LevelContext.SelectedCategoryId < 0 ||
                LevelContext.SelectedCategoryId >= _catalog.Categories.Length)
                return null;

            return _catalog.Categories[LevelContext.SelectedCategoryId];
        }

        public void Dispose() => _disposables?.Dispose();
    }
}