using UnityEngine;
using Game.Attributes;

namespace Game.Data
{
    [CreateAssetMenu(fileName = nameof(WaterShaderConfig), menuName = "Game/" + nameof(WaterShaderConfig))]
    public class WaterShaderConfig : ScriptableObject
    {
        [field: Title("General", CustomColor.Cyan, CustomColor.Blue)]
        [field: SerializeField] public string DisplayName { get; private set; }
        [field: SerializeField] public float WaterDepth { get; private set; } = 0.2f;

        [field: Title("Colors", CustomColor.Green, CustomColor.DarkGreen)]
        [field: SerializeField] public Color ShallowWaterColor { get; private set; } = new(0.25f, 0.54f, 0.9f, 0.25f);
        [field: SerializeField] public Color DeepWater { get; private set; } = new(0.06f, 0.16f, 0.46f, 0.53f);
        [field: SerializeField] public Color FoamColor { get; private set; } = Color.white;

        [field: Title("Refraction", CustomColor.Yellow, CustomColor.Orange)]
        [field: SerializeField] public float RefractionSpeed { get; private set; } = 0.5f;
        [field: SerializeField] public float RefractionScale { get; private set; } = 1f;
        [field: SerializeField] public float RefractionStrength { get; private set; } = 0f;

        [field: Title("Foam", CustomColor.Yellow, CustomColor.Orange)]
        [field: SerializeField] public float FoamAmount { get; private set; } = 1f;
        [field: SerializeField] public float FoamCutOff { get; private set; } = 1f;
        [field: SerializeField] public float FoamSpeed { get; private set; } = 1f;
        [field: SerializeField] public float FoamScale { get; private set; } = 100f;

        [field: Title("Wave 1", CustomColor.Cyan, CustomColor.Blue)]
        [field: SerializeField] public Vector2 Wave1Dir { get; private set; } = new(1f, 0.3f);
        [field: SerializeField] public float Wave1Amp { get; private set; } = 0.03f;
        [field: SerializeField] public float Wave1Len { get; private set; } = 10f;
        [field: SerializeField, Range(0f, 1f)] public float Wave1Steep { get; private set; } = 0.1f;

        [field: Title("Wave 2", CustomColor.Cyan, CustomColor.Blue)]
        [field: SerializeField] public Vector2 Wave2Dir { get; private set; } = new(-0.3f, 1f);
        [field: SerializeField] public float Wave2Amp { get; private set; } = 0.02f;
        [field: SerializeField] public float Wave2Len { get; private set; } = 8f;
        [field: SerializeField, Range(0f, 1f)] public float Wave2Steep { get; private set; } = 0.08f;

        [field: Title("Waves Global", CustomColor.Green, CustomColor.DarkGreen)]
        [field: SerializeField] public float WaveSpeed { get; private set; } = 0.4f;
    }
}