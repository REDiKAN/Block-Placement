using UnityEngine;
using Game.Views;

namespace Game.Data
{
    [CreateAssetMenu(fileName = nameof(BlockConfig), menuName = "Game/" + nameof(BlockConfig))]
    public class BlockConfig : ScriptableObject
    {
        [field: SerializeField] public string DisplayName { get; private set; }
        [field: SerializeField] public BlockView Prefab { get; private set; }
    }
}