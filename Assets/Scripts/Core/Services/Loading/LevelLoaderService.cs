using System;
using UniRx;
using Game.Data;
using Game.Services.Animation;
using Game.Services.Grid;
using Game.Services.Input;
using Game.Services.Placement;
using Game.Services.Rotation;
using Game.Services.Shadow;
using Game.Services.Time;

namespace Game.Services.Loading
{
    public class LevelLoaderService : ILevelLoaderService, IDisposable
    {
        private readonly ILevelTransitionAnimationService _transitionAnimation;
        private readonly IBlockPlacementService _blockPlacement;
        private readonly IStructurePlacementService _structurePlacement;
        private readonly IGridService _gridService;
        private readonly IRotationService _rotationService;
        private readonly ITargetDensityProjectionService _densityProjection;
        private readonly ITimeLimitService _timeLimit;
        private readonly IInputContextService _inputContext;
        private readonly CompositeDisposable _disposables = new();

        public LevelLoaderService(
            ILevelTransitionAnimationService transitionAnimation,
            IBlockPlacementService blockPlacement,
            IStructurePlacementService structurePlacement,
            IGridService gridService,
            IRotationService rotationService,
            ITargetDensityProjectionService densityProjection,
            ITimeLimitService timeLimit,
            IInputContextService inputContext)
        {
            _transitionAnimation = transitionAnimation;
            _blockPlacement = blockPlacement;
            _structurePlacement = structurePlacement;
            _gridService = gridService;
            _rotationService = rotationService;
            _densityProjection = densityProjection;
            _timeLimit = timeLimit;
            _inputContext = inputContext;
        }

        public IObservable<Unit> LoadLevel(LevelConfig config)
        {
            _inputContext.SetContext(InputContext.Generating);

            return _transitionAnimation.PlayDisappearAnimation()
                .Concat(Observable.Defer(() =>
                {
                    ApplyLevelData(config);
                    return _transitionAnimation.PlayAppearAnimation();
                }))
                .Do(_ => _inputContext.SetContext(InputContext.PlaceBlock));
        }

        private void ApplyLevelData(LevelConfig config)
        {
            _gridService.LoadLevel(config);
            _rotationService.LoadLevel(config);
            _densityProjection.LoadLevel(config);
            _timeLimit.LoadLevel(config);
            _blockPlacement.LoadLevel(config);
            _structurePlacement.LoadLevel(config);
        }

        public void Dispose() => _disposables?.Dispose();
    }
}