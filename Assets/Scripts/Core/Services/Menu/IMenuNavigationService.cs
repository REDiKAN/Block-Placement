using System;
using UniRx;

namespace Game.Services.Menu
{
    public enum MenuView
    {
        MainMenu,
        LevelList
    }

    public interface IMenuNavigationService
    {
        IReadOnlyReactiveProperty<MenuView> CurrentView { get; }
        void NavigateTo(MenuView view);
    }
}