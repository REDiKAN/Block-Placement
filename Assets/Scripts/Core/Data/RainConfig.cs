using System;
using UnityEngine;
using Game.Attributes;

namespace Game.Data
{
    [Serializable]
    public struct RainState
    {
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public float TargetRate { get; private set; }
    }

    [CreateAssetMenu(fileName = nameof(RainConfig), menuName = "Game/" + nameof(RainConfig))]
    public class RainConfig : ScriptableObject
    {
        [field: Title("Rain States", CustomColor.Cyan, CustomColor.Blue)]
        [field: SerializeField] public RainState[] States { get; private set; }

        [field: Title("Animation", CustomColor.Yellow, CustomColor.Orange)]
        [field: SerializeField] public float FadeDuration { get; private set; } = 5f;
    }
}