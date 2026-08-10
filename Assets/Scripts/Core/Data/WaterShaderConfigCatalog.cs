using UnityEngine;
using Game.Attributes;

namespace Game.Data
{
    [CreateAssetMenu(fileName = nameof(WaterShaderConfigCatalog), menuName = "Game/" + nameof(WaterShaderConfigCatalog))]
    public class WaterShaderConfigCatalog : ScriptableObject
    {
        [field: SerializeField] public WaterShaderConfig[] Configs { get; private set; }

        [field: Title("Transitions", CustomColor.Green, CustomColor.DarkGreen)]
        [field: SerializeField, Range(0f, 10f)] public float TransitionDuration { get; private set; } = 3f;
    }
}