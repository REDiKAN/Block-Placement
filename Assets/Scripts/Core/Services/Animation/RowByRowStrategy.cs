using System;
using System.Linq;
using DG.Tweening;
using UniRx;
using UnityEngine;
using Zenject;
using Game.Views;

namespace Game.Services.Animation
{
    public class RowByRowStrategy : ILevelIntroAnimationStrategy
    {
        public string Id => "RowByRow";
        public float Duration => 2.5f;

        private const float CellScaleDuration = 0.3f;
        private const float CellStaggerDelay = 0.02f;
        private const float CameraMoveDuration = 0.8f;
        private const float CameraReturnDuration = 0.6f;
        private const float CameraOffsetDistance = 3f;
        private const float CameraTiltAngle = 15f;

        private readonly Camera _camera;
        private readonly FloorGridView _floorGridView;
        private readonly WallView[] _wallViews;

        private Vector3 _originalCameraPosition;
        private Quaternion _originalCameraRotation;
        private float _originalCameraSize;
        private bool _isOrthographic;

        public RowByRowStrategy(
            [Inject(Id = "GameCamera")] Camera camera,
            FloorGridView floorGridView,
            WallView[] wallViews)
        {
            _camera = camera;
            _floorGridView = floorGridView;
            _wallViews = wallViews;
        }

        public IObservable<Unit> Execute(Action onComplete)
        {
            var subject = new Subject<Unit>();
            HideAllElements();
            StoreCameraState();

            var sequence = DOTween.Sequence();
            sequence.Append(CameraMoveOut());
            sequence.Join(AnimateFloorCells());
            sequence.Join(AnimateWallCells());
            sequence.Append(CameraReturn());
            sequence.OnComplete(() =>
            {
                onComplete?.Invoke();
                subject.OnNext(Unit.Default);
                subject.OnCompleted();
            });

            return subject;
        }

        private void HideAllElements()
        {
            if (_floorGridView?.Cells is not null)
            {
                foreach (var cell in _floorGridView.Cells)
                {
                    if (cell is not null)
                    {
                        cell.transform.localScale = Vector3.zero;
                        cell.gameObject.SetActive(false);
                    }
                }
            }

            if (_wallViews is not null)
            {
                foreach (var wall in _wallViews)
                {
                    if (wall?.Cells is not null)
                    {
                        foreach (var cell in wall.Cells)
                        {
                            if (cell is not null)
                            {
                                cell.transform.localScale = Vector3.zero;
                                cell.gameObject.SetActive(false);
                            }
                        }
                    }
                }
            }
        }

        private void StoreCameraState()
        {
            _originalCameraPosition = _camera.transform.position;
            _originalCameraRotation = _camera.transform.rotation;
            _isOrthographic = _camera.orthographic;
            _originalCameraSize = _isOrthographic ? _camera.orthographicSize : _camera.fieldOfView;
        }

        private Tween CameraMoveOut()
        {
            var targetPosition = _originalCameraPosition + Vector3.up * CameraOffsetDistance;
            var targetRotation = Quaternion.Euler(
                _originalCameraRotation.eulerAngles.x + CameraTiltAngle,
                _originalCameraRotation.eulerAngles.y,
                _originalCameraRotation.eulerAngles.z);

            var sequence = DOTween.Sequence();
            sequence.Join(_camera.transform.DOMove(targetPosition, CameraMoveDuration).SetEase(Ease.OutCubic));

            var sizeTween = _isOrthographic
                ? _camera.DOOrthoSize(_originalCameraSize + 2f, CameraMoveDuration)
                : _camera.DOFieldOfView(_originalCameraSize + 10f, CameraMoveDuration);

            sequence.Join(sizeTween.SetEase(Ease.OutCubic));
            sequence.Join(_camera.transform.DORotate(targetRotation.eulerAngles, CameraMoveDuration).SetEase(Ease.OutCubic));

            return sequence;
        }

        private Tween CameraReturn()
        {
            var sequence = DOTween.Sequence();
            sequence.Join(_camera.transform.DOMove(_originalCameraPosition, CameraReturnDuration).SetEase(Ease.OutCubic));

            var sizeTween = _isOrthographic
                ? _camera.DOOrthoSize(_originalCameraSize, CameraReturnDuration)
                : _camera.DOFieldOfView(_originalCameraSize, CameraReturnDuration);

            sequence.Join(sizeTween.SetEase(Ease.OutCubic));
            sequence.Join(_camera.transform.DORotate(_originalCameraRotation.eulerAngles, CameraReturnDuration).SetEase(Ease.OutCubic));

            return sequence;
        }

        private Tween AnimateFloorCells()
        {
            var sequence = DOTween.Sequence();
            if (_floorGridView?.Cells is null) return sequence;

            var cells = _floorGridView.Cells.Where(c => c is not null).ToArray();
            var sortedCells = cells.OrderBy(c => c.transform.localPosition.z).ThenBy(c => c.transform.localPosition.x).ToArray();

            for (var i = 0; i < sortedCells.Length; i++)
            {
                var cell = sortedCells[i];
                var capturedIndex = i;
                sequence.AppendCallback(() => AnimateCellScale(cell, capturedIndex));
            }

            return sequence;
        }

        private Tween AnimateWallCells()
        {
            var sequence = DOTween.Sequence();
            if (_wallViews is null) return sequence;

            foreach (var wall in _wallViews)
            {
                if (wall?.Cells is null) continue;

                var cells = wall.Cells.Where(c => c is not null).ToArray();
                var sortedCells = cells.OrderBy(c => c.transform.localPosition.y).ThenBy(c => c.transform.localPosition.x).ToArray();

                for (var i = 0; i < sortedCells.Length; i++)
                {
                    var cell = sortedCells[i];
                    var capturedIndex = i;
                    sequence.AppendCallback(() => AnimateCellScale(cell, capturedIndex));
                }
            }

            return sequence;
        }

        private void AnimateCellScale(Component cellView, int index)
        {
            if (cellView is null) return;

            cellView.transform.localScale = Vector3.zero;
            cellView.gameObject.SetActive(true);

            var delay = index * CellStaggerDelay;

            cellView.transform.DOScale(Vector3.one, CellScaleDuration)
                .SetEase(Ease.OutQuad)
                .SetDelay(delay)
                .SetAutoKill(true);
        }
    }
}