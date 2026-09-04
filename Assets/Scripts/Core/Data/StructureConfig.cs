using UnityEngine;
using Game.Views;
using Game.Attributes;

namespace Game.Data
{
    [CreateAssetMenu(fileName = nameof(StructureConfig), menuName = "Game/" + nameof(StructureConfig))]
    public class StructureConfig : ScriptableObject
    {
        [field: Title("Basic Settings", CustomColor.Cyan, CustomColor.Blue)]
        [field: SerializeField] public string DisplayName { get; private set; }
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField] public StructureView Prefab { get; private set; }
        [field: SerializeField] public Vector3Int[] LocalCoordinates { get; private set; }

        [field: Title("Audio", CustomColor.Yellow, CustomColor.Orange)]
        [field: SerializeField] public AudioClip PlaceClip { get; private set; }
        [field: SerializeField] public AudioClip RemoveClip { get; private set; }
    }
}