using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(fileName = nameof(LevelCatalog), menuName = "Game/" + nameof(LevelCatalog))]
    public class LevelCatalog : ScriptableObject
    {
        [field: SerializeField] public CategoryConfig[] Categories { get; private set; }
    }
}