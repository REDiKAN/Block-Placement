using UniRx;
using Game.Data;

namespace Game.Services.Menu
{
    public class PreviewService : IPreviewService
    {
        public IReadOnlyReactiveProperty<LevelConfig> PreviewLevel => _previewLevel;
        private readonly ReactiveProperty<LevelConfig> _previewLevel = new();

        public void ShowLevelPreview(LevelConfig level) => _previewLevel.Value = level;
        public void ClearPreview() => _previewLevel.Value = null;
    }
}