using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(fileName = nameof(RaycastConfig), menuName = "Game/" + nameof(RaycastConfig))]
    public class RaycastConfig : ScriptableObject
    {
        [field: SerializeField] public LayerMask FloorMask { get; private set; }
        [field: SerializeField] public LayerMask BlockMask { get; private set; }
        [field: SerializeField] public float MaxDistance { get; private set; } = 100f;
    }
}