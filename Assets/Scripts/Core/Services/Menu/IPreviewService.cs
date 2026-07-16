using System;
using UniRx;
using Game.Data;

namespace Game.Services.Menu
{
    public interface IPreviewService
    {
        IReadOnlyReactiveProperty<LevelConfig> PreviewLevel { get; }
        void ShowLevelPreview(LevelConfig level);
        void ClearPreview();
    }
}