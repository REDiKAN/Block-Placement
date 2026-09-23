using DG.Tweening;
using UnityEngine;
using Game.Attributes;

namespace Game.Data
{
    [CreateAssetMenu(fileName = nameof(LevelTransitionAnimationConfig), menuName = "Game/" + nameof(LevelTransitionAnimationConfig))]
    public class LevelTransitionAnimationConfig : ScriptableObject
    {
        [field: Title("Disappear Phase", CustomColor.Cyan, CustomColor.Blue)]
        [field: SerializeField, Range(0.5f, 5f)] public float DisappearDuration { get; private set; } = 2f;
        [field: SerializeField, Range(0.01f, 0.5f)] public float BlockStaggerDelay { get; private set; } = 0.1f;
        [field: SerializeField, Range(10f, 100f)] public float BlockFlyHeight { get; private set; } = 50f;
        [field: SerializeField] public Ease BlockFlyEase { get; private set; } = Ease.InCubic;

        [field: Title("Appear Phase", CustomColor.Yellow, CustomColor.Orange)]
        [field: SerializeField, Range(0.5f, 5f)] public float AppearDuration { get; private set; } = 2f;
        [field: SerializeField, Range(0.01f, 0.2f)] public float CellStaggerDelay { get; private set; } = 0.02f;
    }
}