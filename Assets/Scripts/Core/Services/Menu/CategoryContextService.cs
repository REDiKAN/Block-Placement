using UniRx;
using Game.Data;

namespace Game.Services.Menu
{
    public class CategoryContextService : ICategoryContextService
    {
        public IReadOnlyReactiveProperty<CategoryConfig> SelectedCategory => _selectedCategory;
        private readonly ReactiveProperty<CategoryConfig> _selectedCategory = new();

        public void SetCategory(CategoryConfig category) => _selectedCategory.Value = category;
        public void ClearCategory() => _selectedCategory.Value = null;
    }
}