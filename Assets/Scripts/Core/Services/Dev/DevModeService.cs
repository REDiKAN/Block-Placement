using System;
using UniRx;
using Zenject;
using Game.Data;
using Game.Services.Input;

namespace Game.Services.Dev
{
    public interface IDevModeService
    {
        IReadOnlyReactiveProperty<BlockConfig> ActiveBlockConfig { get; }
        void SetActiveBlockConfig(BlockConfig config);
    }

    public class DevModeService : IDevModeService
    {
        public IReadOnlyReactiveProperty<BlockConfig> ActiveBlockConfig => _activeBlockConfig;
        private readonly ReactiveProperty<BlockConfig> _activeBlockConfig = new();
        private readonly IInputContextService _contextService;

        public DevModeService(IInputContextService contextService, [InjectOptional] BlockConfig[] configs)
        {
            _contextService = contextService;
            if (configs is not null && configs.Length > 0)
                _activeBlockConfig.Value = configs[0];
        }

        public void SetActiveBlockConfig(BlockConfig config)
        {
            _activeBlockConfig.Value = config;
            _contextService.SetContext(InputContext.PlaceBlock);
        }
    }
}