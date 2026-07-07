using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(fileName = nameof(ShadowLevelConfig), menuName = "Game/" + nameof(ShadowLevelConfig))]
    public class ShadowLevelConfig : ScriptableObject
    {
        [field: SerializeField] public Vector3Int[] InitialBlocks { get; private set; }
        [field: SerializeField] public bool[] FloorMatrix { get; private set; }
    }
}