using UnityEngine;
using Game.Attributes;

namespace Game.Data
{
    [CreateAssetMenu(fileName = nameof(FpsLimitConfig), menuName = "Game/" + nameof(FpsLimitConfig))]
    public class FpsLimitConfig : ScriptableObject
    {
        [field: Title("FPS Presets", CustomColor.Cyan, CustomColor.Blue)]
        [field: SerializeField] public int[] Presets { get; private set; }

        [field: Title("Default Index", CustomColor.Green, CustomColor.DarkGreen)]
        [field: SerializeField] public int DefaultIndex { get; private set; }
    }
}