using System;
using UniRx;
using Zenject;

namespace Game.Services.Input
{
    public enum InputContext
    {
        None,
        PlaceBlock,
        PlaceFloor,
        RemoveFloor,
        LevelCompleted,
        Generating,
        Paused
    }

    public interface IInputContextService
    {
        IReadOnlyReactiveProperty<InputContext> CurrentContext { get; }
        void SetContext(InputContext context);
    }

    public class InputContextService : IInputContextService
    {
        public IReadOnlyReactiveProperty<InputContext> CurrentContext => _currentContext;
        private readonly ReactiveProperty<InputContext> _currentContext = new(InputContext.None);

        public void SetContext(InputContext context) => _currentContext.Value = context;
    }
}