using DG.Tweening;
using UnityEngine;
using Game.Attributes;

namespace Game.Data
{
    [CreateAssetMenu(fileName = nameof(DialogueAnimationConfig), menuName = "Game/" + nameof(DialogueAnimationConfig))]
    public class DialogueAnimationConfig : ScriptableObject
    {
        [field: Title("Text Animation", CustomColor.Cyan, CustomColor.Blue)]
        [field: SerializeField, Range(0.01f, 0.5f)] public float LetterDuration { get; private set; } = 0.15f;
        [field: SerializeField, Range(0f, 0.2f)] public float StaggerDelay { get; private set; } = 0.03f;
        [field: SerializeField, Range(0f, 100f)] public float OffsetY { get; private set; } = 15f;
        [field: SerializeField] public Ease ScaleEase { get; private set; } = Ease.OutBack;
        [field: SerializeField] public Ease PositionEase { get; private set; } = Ease.OutCubic;

        [field: Title("Indicator Animation", CustomColor.Yellow, CustomColor.Orange)]
        [field: SerializeField, Range(1f, 20f)] public float IndicatorBobAmplitude { get; private set; } = 5f;
        [field: SerializeField, Range(0.1f, 2f)] public float IndicatorBobDuration { get; private set; } = 0.4f;
        [field: SerializeField, Range(0f, 0.5f)] public float IndicatorStaggerDelay { get; private set; } = 0.1f;
        [field: SerializeField, Range(0.05f, 1f)] public float IndicatorRotationDuration { get; private set; } = 0.3f;
        [field: SerializeField] public Ease IndicatorRotationEase { get; private set; } = Ease.OutBack;
        [field: SerializeField, Range(0.05f, 1f)] public float IndicatorFadeDuration { get; private set; } = 0.2f;
    }
}