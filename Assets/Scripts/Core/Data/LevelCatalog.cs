using UnityEngine;
using Game.Data;

namespace Game.Data
{
    [CreateAssetMenu(fileName = nameof(LevelCatalog), menuName = "Game/" + nameof(LevelCatalog))]
    public class LevelCatalog : ScriptableObject
    {
        [field: SerializeField] public LevelConfig[] Levels { get; private set; }
    }
}