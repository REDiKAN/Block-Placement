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
    }

    public class InputService : IInputService, ITickable, IDisposable
    {
        private readonly Subject<Vector2> _onMouseMoved = new();
        private readonly Subject<Vector2> _onPrimaryClick = new();
        private readonly Subject<Vector2> _onSecondaryClick = new();
        private readonly CompositeDisposable _disposables = new();

        public IObservable<Vector2> OnMouseMoved => _onMouseMoved;
        public IObservable<Vector2> OnPrimaryClick => _onPrimaryClick;
        public IObservable<Vector2> OnSecondaryClick => _onSecondaryClick;

        public void Tick()
        {
            _onMouseMoved.OnNext(UnityEngine.Input.mousePosition);

            if (UnityEngine.Input.GetMouseButtonDown(0))
                _onPrimaryClick.OnNext(UnityEngine.Input.mousePosition);

            if (UnityEngine.Input.GetMouseButtonDown(1))
                _onSecondaryClick.OnNext(UnityEngine.Input.mousePosition);
        }

        public void Dispose() => _disposables?.Dispose();
    }
}