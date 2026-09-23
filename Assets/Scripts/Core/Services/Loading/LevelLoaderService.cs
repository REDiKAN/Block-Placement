using System;
using UniRx;
using Zenject;
using Game.Data;
using Game.Services.Grid;
using Game.Services.Placement;
using Game.Services.Rotation;
using Game.Services.Shadow;
using Game.Services.Time;
using Game.Services.Input;

namespace Game.Services.Loading
{
    public class LevelLoaderService : ILevelLoaderService, IDisposable
    {
        private readonly IBlockPlacementService _blockPlacement;
        private readonly IStructurePlacementService _structurePlacement;
        private readonly IGridService _gridService;
        private readonly IRotationService _rotationService;
        private readonly ITargetDensityProjectionService _densityProjection;
        private readonly ITimeLimitService _timeLimit;
        private readonly IShadowValidationService _shadowValidation;
        private readonly IInputContextService _inputContext;
        private readonly CompositeDisposable _disposables = new();

        public LevelLoaderService(
            IBlockPlacementService blockPlacement,
            IStructurePlacementService structurePlacement,
            IGridService gridService,
            IRotationService rotationService,
            ITargetDensityProjectionService densityProjection,
            ITimeLimitService timeLimit,
            IShadowValidationService shadowValidation,
            IInputContextService inputContext)
        {
            _blockPlacement = blockPlacement;
            _structurePlacement = structurePlacement;
            _gridService = gridService;
            _rotationService = rotationService;
            _densityProjection = densityProjection;
            _timeLimit = timeLimit;
            _shadowValidation = shadowValidation;
            _inputContext = inputContext;
        }

        public IObservable<Unit> LoadLevel(LevelConfig config)
        {
            _inputContext.SetContext(InputContext.Generating);

            _blockPlacement.ClearAll();
            _structurePlacement.ClearAll();

            _gridService.LoadLevel(config);
            _rotationService.LoadLevel(config);
            _densityProjection.LoadLevel(config);
            _timeLimit.LoadLevel(config);
            _blockPlacement.LoadLevel(config);
            _structurePlacement.LoadLevel(config);

            _shadowValidation.ForceRevalidate();

            return Observable.Return(Unit.Default);
        }

        public void Dispose() => _disposables?.Dispose();
    }
}