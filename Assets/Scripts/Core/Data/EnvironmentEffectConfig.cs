using UnityEngine;
using Game.Attributes;

namespace Game.Data
{
    [CreateAssetMenu(fileName = nameof(EnvironmentEffectConfig), menuName = "Game/" + nameof(EnvironmentEffectConfig))]
    public class EnvironmentEffectConfig : ScriptableObject
    {
        [field: Title("Effect Settings", CustomColor.Cyan, CustomColor.Blue)]
        [field: SerializeField] public GameObject Prefab { get; private set; }

        [field: Title("Probability", CustomColor.Green, CustomColor.DarkGreen)]
        [field: SerializeField, Range(0f, 1f)] public float Probability { get; private set; }
    }
}