using UnityEngine;
using Game.Views;
using Game.Attributes;

namespace Game.Data
{
    [CreateAssetMenu(fileName = nameof(BlockConfig), menuName = "Game/" + nameof(BlockConfig))]
    public class BlockConfig : ScriptableObject
    {
        [field: Title("Basic Settings", CustomColor.Cyan, CustomColor.Blue)]
        [field: SerializeField] public string DisplayName { get; private set; }
        [field: SerializeField] public BlockView Prefab { get; private set; }

        [field: Title("Audio", CustomColor.Yellow, CustomColor.Orange)]
        [field: SerializeField] public AudioClip PlaceClip { get; private set; }
        [field: SerializeField] public AudioClip RemoveClip { get; private set; }
    }
}