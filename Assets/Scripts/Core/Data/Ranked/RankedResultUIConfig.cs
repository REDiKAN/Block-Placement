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

        [field: Title("MMR Animation", CustomColor.Red, CustomColor.Orange)]
        [field: SerializeField] public float MmrPhaseDuration { get; private set; } = 0.5f;
        [field: SerializeField] public float MmrTextSwapDuration { get; private set; } = 0.1f;
        [field: SerializeField] public Ease MmrEase { get; private set; } = Ease.OutCubic;
    }
}