namespace Game.Views.Effects
{
    public interface IEffectView
    {
        float Probability { get; }
        bool IsVisible { get; }
        void Show();
        void Hide();
    }
}