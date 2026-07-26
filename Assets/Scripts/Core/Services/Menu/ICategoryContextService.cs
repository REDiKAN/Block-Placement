using UniRx;
using Game.Data;

namespace Game.Services.Menu
{
    public interface ICategoryContextService
    {
        IReadOnlyReactiveProperty<CategoryConfig> SelectedCategory { get; }
        void SetCategory(CategoryConfig category);
        void ClearCategory();
    }
}