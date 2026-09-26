using DG.Tweening;
using UnityEngine;
using Game.Attributes;

namespace Game.Data
{
    [CreateAssetMenu(fileName = nameof(FloatingDecorInteractionConfig), menuName = "Game/" + nameof(FloatingDecorInteractionConfig))]
    public class FloatingDecorInteractionConfig : ScriptableObject
    {
        [field: Title("Raycast Settings", CustomColor.Cyan, CustomColor.Blue)]
        [field: SerializeField] public LayerMask InteractableLayer { get; private set; }
        [field: SerializeField, Range(10f, 500f)] public float MaxDistance { get; private set; } = 100f;

        [field: Title("Dive Phase (Underwater)", CustomColor.Yellow, CustomColor.Orange)]
        [field: SerializeField, Range(-5f, 0f)] public float DiveDepth { get; private set; } = -0.5f;
        [field: SerializeField, Range(0.05f, 2f)] public float DiveDownDuration { get; private set; } = 0.2f;
        [field: SerializeField] public Ease DiveDownEase { get; private set; } = Ease.InQuad;

        [field: Title("Jump Phase (Out of Water)", CustomColor.Green, CustomColor.DarkGreen)]
        [field: SerializeField, Range(0.1f, 2f)] public float JumpHeight { get; private set; } = 0.4f;
        [field: SerializeField, Range(0.05f, 2f)] public float JumpUpDuration { get; private set; } = 0.25f;
        [field: SerializeField] public Ease JumpUpEase { get; private set; } = Ease.OutCubic;

        [field: Title("Settle Phase (Return to Surface)", CustomColor.Red, CustomColor.Orange)]
        [field: SerializeField, Range(0.05f, 2f)] public float SettleDownDuration { get; private set; } = 0.3f;
        [field: SerializeField] public Ease SettleDownEase { get; private set; } = Ease.InOutSine;
    }
}