using DG.Tweening;
using UnityEngine;
using Game.Attributes;

namespace Game.Data
{
    [CreateAssetMenu(fileName = nameof(StructureInventoryAnimationConfig), menuName = "Game/" + nameof(StructureInventoryAnimationConfig))]
    public class StructureInventoryAnimationConfig : ScriptableObject
    {
        [field: Title("Intro Animation", CustomColor.Cyan, CustomColor.Blue)]
        [field: SerializeField, Range(0.1f, 1f)] public float IntroDuration { get; private set; } = 0.4f;
        [field: SerializeField, Range(0f, 0.2f)] public float IntroStaggerDelay { get; private set; } = 0.05f;
        [field: SerializeField, Range(-500f, 0f)] public float IntroOffsetY { get; private set; } = -200f;
        [field: SerializeField] public Ease IntroEase { get; private set; } = Ease.OutBack;

        [field: Title("Outro Animation", CustomColor.Yellow, CustomColor.Orange)]
        [field: SerializeField, Range(0.1f, 1f)] public float OutroDuration { get; private set; } = 0.3f;
        [field: SerializeField, Range(0f, 0.2f)] public float OutroStaggerDelay { get; private set; } = 0.03f;
        [field: SerializeField, Range(-500f, 0f)] public float OutroOffsetY { get; private set; } = -200f;
        [field: SerializeField] public Ease OutroEase { get; private set; } = Ease.InBack;

        [field: Title("Selection Animation", CustomColor.Green, CustomColor.DarkGreen)]
        [field: SerializeField, Range(0.1f, 0.5f)] public float SelectionDuration { get; private set; } = 0.2f;
        [field: SerializeField, Range(0f, 50f)] public float SelectionOffsetY { get; private set; } = 20f;
        [field: SerializeField] public Ease SelectionEase { get; private set; } = Ease.OutCubic;
    }
}