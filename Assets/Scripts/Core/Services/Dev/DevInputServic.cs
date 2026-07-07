using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace Game.Services.Dev
{
    public interface IDevInputService
    {
        IObservable<Unit> OnExportRequested { get; }
    }

    public class DevInputService : IDevInputService, ITickable
    {
        private readonly Subject<Unit> _onExportRequested = new();

        public IObservable<Unit> OnExportRequested => _onExportRequested;

        public void Tick()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.N))
            {
                Debug.Log("[DevInputService] Export hotkey 'N' detected. Triggering export event.");
                _onExportRequested.OnNext(Unit.Default);
            }
        }
    }
}