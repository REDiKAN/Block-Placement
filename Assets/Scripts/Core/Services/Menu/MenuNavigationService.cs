using UniRx;

namespace Game.Services.Menu
{
    public class MenuNavigationService : IMenuNavigationService
    {
        public IReadOnlyReactiveProperty<MenuView> CurrentView => _currentView;
        private readonly ReactiveProperty<MenuView> _currentView = new(MenuView.MainMenu);

        public void NavigateTo(MenuView view) => _currentView.Value = view;
    }
}