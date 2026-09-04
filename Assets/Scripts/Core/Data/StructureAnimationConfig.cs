using DG.Tweening;
using UnityEngine;
using Game.Attributes;

namespace Game.Data
{
    [CreateAssetMenu(fileName = nameof(StructureAnimationConfig), menuName = "Game/" + nameof(StructureAnimationConfig))]
    public class StructureAnimationConfig : ScriptableObject
    {
        [field: Title("Spawn Settings", CustomColor.Cyan, CustomColor.Blue)]
        [field: SerializeField, Range(0.05f, 1f)] public float SpawnDuration { get; private set; } = 0.25f;
        [field: SerializeField] public Ease SpawnEase { get; private set; } = Ease.OutBack;

        [field: Title("Despawn Settings", CustomColor.Yellow, CustomColor.Orange)]
        [field: SerializeField, Range(0.05f, 1f)] public float DespawnDuration { get; private set; } = 0.2f;
        [field: SerializeField] public Ease DespawnEase { get; private set; } = Ease.InBack;
    }
}