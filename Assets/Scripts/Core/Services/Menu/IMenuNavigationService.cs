using UniRx;

namespace Game.Services.Menu
{
    public enum MenuView
    {
        MainMenu,
        CategoryList,
        LevelList,
        CustomSettings
    }

    public interface IMenuNavigationService
    {
        IReadOnlyReactiveProperty<MenuView> CurrentView { get; }
        void NavigateTo(MenuView view);
    }
}