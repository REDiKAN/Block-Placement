using DG.Tweening;
using UnityEngine;
using Game.Attributes;

namespace Game.Data
{
    [CreateAssetMenu(fileName = nameof(DialogueBackgroundUIConfig), menuName = "Game/" + nameof(DialogueBackgroundUIConfig))]
    public class DialogueBackgroundUIConfig : ScriptableObject
    {
        [field: Title("Bar Fill Animation", CustomColor.Cyan, CustomColor.Blue)]
        [field: SerializeField, Range(0.01f, 1f)] public float BarFillDuration { get; private set; } = 0.1f;
        [field: SerializeField] public Ease BarFillEase { get; private set; } = Ease.OutQuad;

        [field: Title("Bar Hide Animation", CustomColor.Yellow, CustomColor.Orange)]
        [field: SerializeField, Range(0.01f, 1f)] public float BarHideDuration { get; private set; } = 0.2f;
        [field: SerializeField] public Ease BarHideEase { get; private set; } = Ease.InQuad;
    }
}