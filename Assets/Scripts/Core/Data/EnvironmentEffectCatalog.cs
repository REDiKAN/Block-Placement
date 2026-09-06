using UnityEngine;
using Game.Attributes;

namespace Game.Data
{
    [CreateAssetMenu(fileName = nameof(EnvironmentEffectCatalog), menuName = "Game/" + nameof(EnvironmentEffectCatalog))]
    public class EnvironmentEffectCatalog : ScriptableObject
    {
        [field: Title("Effects", CustomColor.Cyan, CustomColor.Blue)]
        [field: SerializeField] public EnvironmentEffectConfig[] Configs { get; private set; }
    }
}