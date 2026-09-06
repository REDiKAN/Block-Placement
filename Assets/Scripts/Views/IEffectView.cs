namespace Game.Views.Effects
{
    public interface IEffectView
    {
        float Probability { get; }
        void Show();
        void Hide();
    }
}