using DG.Tweening;
using UnityEngine;
using Game.Attributes;

namespace Game.Data
{
    [CreateAssetMenu(fileName = nameof(StaggeredAnimationConfig), menuName = "Game/" + nameof(StaggeredAnimationConfig))]
    public class StaggeredAnimationConfig : ScriptableObject
    {
        [field: Title("Animation Settings", CustomColor.Cyan, CustomColor.Blue)]
        [field: SerializeField, Range(0.05f, 2f)] public float Duration { get; private set; } = 0.4f;
        [field: SerializeField, Range(0f, 0.5f)] public float StaggerDelay { get; private set; } = 0.05f;
        [field: SerializeField] public float OffsetX { get; private set; } = 800f;
        [field: SerializeField, Range(0f, 1f)] public float InitialScale { get; private set; } = 0.8f;

        [field: Title("Easing", CustomColor.Yellow, CustomColor.Orange)]
        [field: SerializeField] public Ease PositionEase { get; private set; } = Ease.OutCubic;
        [field: SerializeField] public Ease ScaleEase { get; private set; } = Ease.OutBack;
        [field: SerializeField] public Ease FadeEase { get; private set; } = Ease.OutQuad;
    }
}