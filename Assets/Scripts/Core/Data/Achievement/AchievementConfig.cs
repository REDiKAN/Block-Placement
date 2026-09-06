using UnityEngine;
using Game.Attributes;
using Game.Views.UI.Achievements;

namespace Game.Data
{
    [CreateAssetMenu(fileName = nameof(AchievementConfig), menuName = "Game/" + nameof(AchievementConfig))]
    public class AchievementConfig : ScriptableObject
    {
        [field: Title("Basic Settings", CustomColor.Cyan, CustomColor.Blue)]
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public string Title { get; private set; }
        [field: SerializeField] public string Description { get; private set; }
        [field: SerializeField] public AchievementItemView UIPrefab { get; private set; }

        [field: Title("Visuals", CustomColor.Yellow, CustomColor.Orange)]
        [field: SerializeField] public Sprite Icon { get; private set; }

        [field: Title("Condition", CustomColor.Green, CustomColor.DarkGreen)]
        [field: SerializeField] public AchievementConditionType ConditionType { get; private set; }
        [field: SerializeField, Min(1)] public int TargetValue { get; private set; } = 1;
    }
}