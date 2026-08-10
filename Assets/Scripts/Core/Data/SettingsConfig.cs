using UnityEngine;
using Game.Attributes;

namespace Game.Data
{
    [CreateAssetMenu(fileName = nameof(SettingsConfig), menuName = "Game/" + nameof(SettingsConfig))]
    public class SettingsConfig : ScriptableObject
    {
        [field: Title("Resolutions", CustomColor.Cyan, CustomColor.Blue)]
        [field: SerializeField] public ResolutionData[] Resolutions { get; private set; }
    }

    [System.Serializable]
    public class ResolutionData
    {
        [field: SerializeField] public int Width { get; private set; }
        [field: SerializeField] public int Height { get; private set; }

        public override string ToString() => $"{Width}x{Height}";
    }
}