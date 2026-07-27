using UniRx;
using Zenject;
using Game.Data;
using Game.Core;

namespace Game.Services.Generation
{
    public class GenerationContext : IGenerationContext, IInitializable
    {
        public IReadOnlyReactiveProperty<bool> IsEndlessModeActive => _isEndlessModeActive;
        public IReadOnlyReactiveProperty<CustomGenerationSettings> CurrentSettings => _currentSettings;

        private readonly ReactiveProperty<bool> _isEndlessModeActive = new();
        private readonly ReactiveProperty<CustomGenerationSettings> _currentSettings = new();

        public void Initialize()
        {
            _isEndlessModeActive.Value = EndlessContext.IsEndlessModeActive;
            _currentSettings.Value = EndlessContext.Settings;
        }
    }
}