using UnityEngine;
using Game.Attributes;

namespace Game.Data
{
    [CreateAssetMenu(fileName = nameof(CategoryConfig), menuName = "Game/" + nameof(CategoryConfig))]
    public class CategoryConfig : ScriptableObject
    {
        [field: Title("Category Settings", CustomColor.Cyan, CustomColor.Blue)]
        [field: SerializeField] public string Title { get; private set; }
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField] public LevelConfig[] Levels { get; private set; }

        [field: Title("Progression", CustomColor.Green, CustomColor.DarkGreen)]
        [field: SerializeField] public bool IsSequential { get; private set; }
        [field: SerializeField] public bool IsCustomGenerator { get; private set; }
    }
}