using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(fileName = nameof(RotationConfig), menuName = "Game/" + nameof(RotationConfig))]
    public class RotationConfig : ScriptableObject
    {
        [field: SerializeField] public float Duration { get; private set; } = 0.3f;
    }
}