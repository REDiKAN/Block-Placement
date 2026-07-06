using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(fileName = nameof(ShadowLevelConfig), menuName = "Game/" + nameof(ShadowLevelConfig))]
    public class ShadowLevelConfig : ScriptableObject
    {
        [field: SerializeField] public bool[] Wall1Target { get; private set; }
        [field: SerializeField] public bool[] Wall2Target { get; private set; }
    }
}