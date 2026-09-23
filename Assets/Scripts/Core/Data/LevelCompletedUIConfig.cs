using DG.Tweening;
using UnityEngine;
using Game.Attributes;

namespace Game.Data
{
    [CreateAssetMenu(fileName = nameof(LevelCompletedUIConfig), menuName = "Game/" + nameof(LevelCompletedUIConfig))]
    public class LevelCompletedUIConfig : ScriptableObject
    {
        [field: Title("Bar Settings", CustomColor.Cyan, CustomColor.Blue)]
        [field: SerializeField, Range(0.01f, 1f)] public float BarFillDuration { get; private set; } = 0.1f;

        [field: Title("Text Animation", CustomColor.Yellow, CustomColor.Orange)]
        [field: SerializeField, Range(0.01f, 0.5f)] public float LetterDuration { get; private set; } = 0.15f;
        [field: SerializeField, Range(0f, 0.2f)] public float StaggerDelay { get; private set; } = 0.03f;
        [field: SerializeField, Range(0f, 100f)] public float OffsetY { get; private set; } = 15f;
        [field: SerializeField] public Ease ScaleEase { get; private set; } = Ease.OutBack;
        [field: SerializeField] public Ease PositionEase { get; private set; } = Ease.OutCubic;

        [field: Title("UI Settings", CustomColor.Green, CustomColor.DarkGreen)]
        [field: SerializeField, Range(0.1f, 2f)] public float FadeDuration { get; private set; } = 0.4f;
    }
}