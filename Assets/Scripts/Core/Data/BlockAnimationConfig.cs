using UnityEngine;
using Game.Attributes;

namespace Game.Data
{
    public enum BlockAnimationEase
    {
        OutQuad,
        OutCubic,
        OutBack,
        InQuad,
        InCubic,
        InBack
    }

    [CreateAssetMenu(fileName = nameof(BlockAnimationConfig), menuName = "Game/" + nameof(BlockAnimationConfig))]
    public class BlockAnimationConfig : ScriptableObject
    {
        [field: Title("Spawn Settings", CustomColor.Cyan, CustomColor.Blue)]
        [field: SerializeField, Range(0.1f, 0.5f)] public float SpawnDuration { get; private set; } = 0.15f;

        [field: SerializeField, Range(1.0f, 1.5f)] public float SpawnScale { get; private set; } = 1.15f;

        [field: SerializeField] public BlockAnimationEase SpawnEase { get; private set; } = BlockAnimationEase.OutCubic;

        [field: Title("Despawn Settings", CustomColor.Yellow, CustomColor.Orange)]
        [field: SerializeField, Range(0.1f, 0.5f)] public float DespawnDuration { get; private set; } = 0.15f;

        [field: SerializeField] public BlockAnimationEase DespawnEase { get; private set; } = BlockAnimationEase.InCubic;
    }
}