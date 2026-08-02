using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace Game.Services.Input
{
    public interface IInputService
    {
        IObservable<Vector2> OnMouseMoved { get; }
        IObservable<Vector2> OnPrimaryClick { get; }
        IObservable<Vector2> OnSecondaryClick { get; }
        IObservable<Unit> OnRotateLeft { get; }
        IObservable<Unit> OnRotateRight { get; }
        IObservable<Unit> OnNextLevelRequested { get; }
    }

    public class InputService : IInputService, ITickable, IDisposable
    {
        private readonly Subject<Vector2> _onMouseMoved = new();
        private readonly Subject<Vector2> _onPrimaryClick = new();
        private readonly Subject<Vector2> _onSecondaryClick = new();
        private readonly Subject<Unit> _onRotateLeft = new();
        private readonly Subject<Unit> _onRotateRight = new();
        private readonly Subject<Unit> _onNextLevelRequested = new();
        private readonly CompositeDisposable _disposables = new();

        private Vector3 _lastMousePosition;

        public IObservable<Vector2> OnMouseMoved => _onMouseMoved;
        public IObservable<Vector2> OnPrimaryClick => _onPrimaryClick;
        public IObservable<Vector2> OnSecondaryClick => _onSecondaryClick;
        public IObservable<Unit> OnRotateLeft => _onRotateLeft;
        public IObservable<Unit> OnRotateRight => _onRotateRight;
        public IObservable<Unit> OnNextLevelRequested => _onNextLevelRequested;

        public void Tick()
        {
            var currentPosition = UnityEngine.Input.mousePosition;

            if (currentPosition != _lastMousePosition)
            {
                _onMouseMoved.OnNext((Vector2)currentPosition);
                _lastMousePosition = currentPosition;
            }

            if (UnityEngine.Input.GetMouseButtonDown(0))
                _onPrimaryClick.OnNext((Vector2)currentPosition);

            if (UnityEngine.Input.GetMouseButtonDown(1))
                _onSecondaryClick.OnNext((Vector2)currentPosition);

            if (UnityEngine.Input.GetKeyDown(KeyCode.A))
                _onRotateLeft.OnNext(Unit.Default);

            if (UnityEngine.Input.GetKeyDown(KeyCode.D))
                _onRotateRight.OnNext(Unit.Default);

            if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
                _onNextLevelRequested.OnNext(Unit.Default);
        }

        public void Dispose() => _disposables?.Dispose();
    }
}