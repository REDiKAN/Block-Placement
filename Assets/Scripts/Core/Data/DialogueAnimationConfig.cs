using DG.Tweening;
using UnityEngine;
using Game.Attributes;

namespace Game.Data
{
    [CreateAssetMenu(fileName = nameof(DialogueAnimationConfig), menuName = "Game/" + nameof(DialogueAnimationConfig))]
    public class DialogueAnimationConfig : ScriptableObject
    {
        [field: Title("Animation Settings", CustomColor.Cyan, CustomColor.Blue)]
        [field: SerializeField, Range(0.01f, 0.5f)] public float LetterDuration { get; private set; } = 0.15f;

        [field: SerializeField, Range(0f, 0.2f)] public float StaggerDelay { get; private set; } = 0.03f;

        [field: SerializeField, Range(0f, 100f)] public float OffsetY { get; private set; } = 15f;

        [field: Title("Easing", CustomColor.Yellow, CustomColor.Orange)]
        [field: SerializeField] public Ease ScaleEase { get; private set; } = Ease.OutBack;

        [field: SerializeField] public Ease PositionEase { get; private set; } = Ease.OutCubic;
    }
}