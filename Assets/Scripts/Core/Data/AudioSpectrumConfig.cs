using UnityEngine;
using Game.Attributes;

namespace Game.Data
{
    [CreateAssetMenu(fileName = nameof(AudioSpectrumConfig), menuName = "Game/" + nameof(AudioSpectrumConfig))]
    public class AudioSpectrumConfig : ScriptableObject
    {
        [field: Title("Spectrum Settings", CustomColor.Cyan, CustomColor.Blue)]
        [field: SerializeField, Range(16, 128)] public int BarCount { get; private set; } = 32;
        [field: SerializeField] public FFTWindow WindowType { get; private set; } = FFTWindow.Rectangular;

        [field: Title("Algorithm Settings", CustomColor.Yellow, CustomColor.Orange)]
        [field: SerializeField, Range(0.0000001f, 0.1f)] public float RefValue { get; private set; } = 0.00005f;
        [field: SerializeField, Range(0.01f, 2f)] public float ReleaseTime { get; private set; } = 0.2f;
        [field: SerializeField, Range(0.1f, 10f)] public float ScaleMultiplier { get; private set; } = 3f;

        [field: Title("UI Settings", CustomColor.Green, CustomColor.DarkGreen)]
        [field: SerializeField, Range(0f, 50f)] public float Spacing { get; private set; } = 5f;
        [field: SerializeField, Range(0f, 50f)] public float MinHeight { get; private set; } = 10f;
        [field: SerializeField, Range(50f, 500f)] public float MaxHeight { get; private set; } = 200f;
        [field: SerializeField, Range(10f, 100f)] public float MaxInputValue { get; private set; } = 40f;
        [field: SerializeField] public Color BarColor { get; private set; } = new(0f, 1f, 1f, 1f);
    }
}