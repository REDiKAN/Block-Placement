using UniRx;

namespace Game.Services.Menu
{
    public enum MenuView
    {
        MainMenu,
        CategoryList,
        LevelList
    }

    public interface IMenuNavigationService
    {
        IReadOnlyReactiveProperty<MenuView> CurrentView { get; }
        void NavigateTo(MenuView view);
    }
}