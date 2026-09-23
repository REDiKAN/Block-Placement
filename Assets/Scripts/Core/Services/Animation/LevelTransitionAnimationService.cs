using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UniRx;
using UnityEngine;
using Game.Data;
using Game.Services.Placement;
using Game.Services.Shadow;
using Game.Views;

namespace Game.Services.Animation
{
    public class LevelTransitionAnimationService : ILevelTransitionAnimationService, IDisposable
    {
        private readonly LevelTransitionAnimationConfig _config;
        private readonly WallView[] _wallViews;
        private readonly IBlockPlacementService _blockPlacement;
        private readonly IStructurePlacementService _structurePlacement;
        private readonly IShadowValidationService _shadowValidation;
        private readonly ITargetDensityProjectionService _projectionService;
        private readonly CompositeDisposable _disposables = new();
        private Sequence _animationSequence;

        public LevelTransitionAnimationService(
            LevelTransitionAnimationConfig config,
            WallView[] wallViews,
            IBlockPlacementService blockPlacement,
            IStructurePlacementService structurePlacement,
            IShadowValidationService shadowValidation,
            ITargetDensityProjectionService projectionService)
        {
            _config = config;
            _wallViews = wallViews;
            _blockPlacement = blockPlacement;
            _structurePlacement = structurePlacement;
            _shadowValidation = shadowValidation;
            _projectionService = projectionService;
        }

        public IObservable<Unit> PlayDisappearAnimation()
        {
            var subject = new Subject<Unit>();
            _shadowValidation.SetSuppressed(true);

            _animationSequence?.Kill();
            _animationSequence = DOTween.Sequence();

            var blocks = GetSortedBlocks();
            var structures = GetSortedStructures();
            var objectIndex = 0;

            foreach (var block in blocks)
            {
                var delay = objectIndex * _config.BlockStaggerDelay;
                var duration = Mathf.Max(_config.DisappearDuration - delay, 0.1f);
                var targetPosition = block.transform.position + Vector3.up * _config.BlockFlyHeight;
                _animationSequence.Insert(delay, block.transform
                    .DOMove(targetPosition, duration)
                    .SetEase(_config.BlockFlyEase)
                    .OnComplete(() => block.gameObject.SetActive(false)));
                objectIndex++;
            }

            foreach (var structure in structures)
            {
                var delay = objectIndex * _config.BlockStaggerDelay;
                var duration = Mathf.Max(_config.DisappearDuration - delay, 0.1f);
                var targetPosition = structure.transform.position + Vector3.up * _config.BlockFlyHeight;
                _animationSequence.Insert(delay, structure.transform
                    .DOMove(targetPosition, duration)
                    .SetEase(_config.BlockFlyEase)
                    .OnComplete(() => structure.gameObject.SetActive(false)));
                objectIndex++;
            }

            AnimateWallsToState(ShadowCellState.Correct, _animationSequence);

            _animationSequence.OnComplete(() =>
            {
                _blockPlacement.ClearAll();
                _structurePlacement.ClearAll();
                subject.OnNext(Unit.Default);
                subject.OnCompleted();
            });

            return subject;
        }

        public IObservable<Unit> PlayAppearAnimation()
        {
            var subject = new Subject<Unit>();
            _shadowValidation.ForceRevalidate();

            _animationSequence?.Kill();
            _animationSequence = DOTween.Sequence();

            AnimateWallsToTargetState(_animationSequence);

            _animationSequence.OnComplete(() =>
            {
                ShowDensityIndicators();
                _shadowValidation.SetSuppressed(false);
                subject.OnNext(Unit.Default);
                subject.OnCompleted();
            });

            return subject;
        }

        private void AnimateWallsToState(ShadowCellState targetState, Sequence sequence)
        {
            if (_wallViews is null) return;
            foreach (var wall in _wallViews)
            {
                if (wall?.Cells is null) continue;
                for (var i = 0; i < wall.Cells.Length; i++)
                {
                    var cell = wall.Cells[i];
                    if (cell is null) continue;
                    var delay = i * _config.CellStaggerDelay;
                    sequence.Join(DOVirtual.DelayedCall(delay, () => cell.SetState(targetState)));
                }
            }
        }

        private void AnimateWallsToTargetState(Sequence sequence)
        {
            if (_wallViews is null) return;
            foreach (var wall in _wallViews)
            {
                if (wall?.Cells is null) continue;
                var wallIndex = wall.WallIndex;
                for (var i = 0; i < wall.Cells.Length; i++)
                {
                    var cell = wall.Cells[i];
                    if (cell is null) continue;
                    var targetState = _shadowValidation.GetCellState(wallIndex, i);
                    var delay = i * _config.CellStaggerDelay;
                    sequence.Join(DOVirtual.DelayedCall(delay, () => cell.SetState(targetState)));
                }
            }
        }

        private void ShowDensityIndicators()
        {
            if (_wallViews is null) return;
            foreach (var wall in _wallViews)
            {
                if (wall?.Cells is null) continue;
                var wallIndex = wall.WallIndex;
                var densities = _projectionService.GetCurrentDensities(wallIndex);
                if (densities is null) continue;
                for (var i = 0; i < wall.Cells.Length && i < densities.Length; i++)
                {
                    if (wall.Cells[i] is not null)
                        wall.Cells[i].SetTargetDensity(densities[i].TargetDensity, densities[i].IsDensityEnabled);
                }
            }
        }

        private List<BlockView> GetSortedBlocks() =>
            _blockPlacement.GetActiveBlocks()
                .OrderBy(b => b.transform.position.y)
                .ThenBy(b => b.transform.position.x)
                .ThenBy(b => b.transform.position.z)
                .ToList();

        private List<StructureView> GetSortedStructures() =>
            _structurePlacement.GetActiveStructures()
                .OrderBy(s => s.transform.position.y)
                .ThenBy(s => s.transform.position.x)
                .ThenBy(s => s.transform.position.z)
                .ToList();

        public void Dispose()
        {
            _animationSequence?.Kill();
            _disposables?.Dispose();
        }
    }
}