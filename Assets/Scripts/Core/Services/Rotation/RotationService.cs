using System;
using DG.Tweening;
using UniRx;
using UnityEngine;
using Zenject;
using Game.Data;
using Game.Services.Input;

namespace Game.Services.Rotation
{
    public interface IRotationService
    {
        IObservable<int> OnRotationCompleted { get; }
        Vector3Int[] CurrentInitialBlocks { get; }
    }

    public class RotationService : IRotationService, IInitializable, IDisposable
    {
        public IObservable<int> OnRotationCompleted => _onRotationCompleted;
        public Vector3Int[] CurrentInitialBlocks => _currentInitialBlocks;

        private const int GridSize = 5;
        private readonly Subject<int> _onRotationCompleted = new();
        private readonly IInputService _inputService;
        private readonly RotationConfig _config;
        private readonly Transform _pivot;
        private readonly CompositeDisposable _disposables = new();
        private Vector3Int[] _currentInitialBlocks;
        private Tween _rotationTween;
        private bool _isRotating;
        private int _lastAngle;

        public RotationService(
            IInputService inputService,
            RotationConfig config,
            [Inject(Id = "RotationPivot")] Transform pivot,
            LevelConfig levelConfig)
        {
            _inputService = inputService;
            _config = config;
            _pivot = pivot;
            _currentInitialBlocks = (Vector3Int[])levelConfig.InitialBlocks.Clone();
        }

        public void Initialize()
        {
            _inputService.OnRotateLeft
                .Subscribe(_ => Rotate(-90))
                .AddTo(_disposables);
            _inputService.OnRotateRight
                .Subscribe(_ => Rotate(90))
                .AddTo(_disposables);
        }

        private void Rotate(int angle)
        {
            if (_isRotating) return;
            _isRotating = true;
            _lastAngle = angle;
            RotateInitialBlocks(angle);
            var targetRotation = _pivot.eulerAngles + new Vector3(0f, angle, 0f);
            _rotationTween = _pivot.DORotate(targetRotation, _config.Duration)
                .SetEase(Ease.Linear)
                .SetAutoKill(true)
                .OnComplete(OnRotationComplete);
        }

        private void RotateInitialBlocks(int angle)
        {
            for (var i = 0; i < _currentInitialBlocks.Length; i++)
            {
                var block = _currentInitialBlocks[i];
                _currentInitialBlocks[i] = angle == 90
                    ? new Vector3Int(block.z, block.y, GridSize - 1 - block.x)
                    : new Vector3Int(GridSize - 1 - block.z, block.y, block.x);
            }
        }

        private void OnRotationComplete()
        {
            _isRotating = false;
            _onRotationCompleted.OnNext(_lastAngle);
        }

        public void Dispose()
        {
            _disposables?.Dispose();
            _rotationTween?.Kill();
        }
    }
}