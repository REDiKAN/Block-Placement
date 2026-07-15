using UnityEngine;

namespace Game.Attributes
{
    public enum CustomColor
    {
        None,
        Cyan,
        Blue,
        Green,
        DarkGreen,
        Yellow,
        Orange,
        Red,
        White,
        Gray
    }

    public class TitleAttribute : PropertyAttribute
    {
        public string Text { get; }
        public CustomColor Color1 { get; }
        public CustomColor Color2 { get; }

        public TitleAttribute(string text, CustomColor color1 = CustomColor.None, CustomColor color2 = CustomColor.None)
        {
            Text = text;
            Color1 = color1;
            Color2 = color2;
        }
    }
}