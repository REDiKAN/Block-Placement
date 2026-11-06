using DG.Tweening;
using UnityEngine;
using Game.Attributes;

namespace Game.Data.Ranked
{
    [CreateAssetMenu(fileName = nameof(RankedResultUIConfig), menuName = "Game/Ranked/" + nameof(RankedResultUIConfig))]
    public class RankedResultUIConfig : ScriptableObject
    {
        [field: Title("Background", CustomColor.Cyan, CustomColor.Blue)]
        [field: SerializeField, Range(0f, 1f)] public float BackgroundAlpha { get; private set; } = 0.7f;
        [field: SerializeField, Range(0f, 1f)] public float WindowAlpha { get; private set; } = 0.85f;

        [field: Title("Show Animation", CustomColor.Green, CustomColor.DarkGreen)]
        [field: SerializeField] public float ShowDuration { get; private set; } = 0.6f;
        [field: SerializeField] public Ease ShowEase { get; private set; } = Ease.OutBack;
        [field: SerializeField] public float ShowOffsetY { get; private set; } = 1000f;

        [field: Title("Hide Animation", CustomColor.Yellow, CustomColor.Orange)]
        [field: SerializeField] public float HideDuration { get; private set; } = 0.5f;
        [field: SerializeField] public Ease HideEase { get; private set; } = Ease.InCubic;

        [field: Title("Cascade Animation", CustomColor.Green, CustomColor.DarkGreen)]
        [field: SerializeField, Range(10f, 200f)] public float PointsPerSecond { get; private set; } = 50f;
        [field: SerializeField, Range(0.1f, 1f)] public float MinElementDuration { get; private set; } = 0.3f;
        [field: SerializeField] public Ease CascadeEase { get; private set; } = Ease.Linear;
        [field: SerializeField, Range(0.1f, 0.5f)] public float ElementFadeOutDuration { get; private set; } = 0.2f;
        [field: SerializeField, Range(0.1f, 0.5f)] public float ListFadeInDuration { get; private set; } = 0.3f;
    }
}