using UnityEngine;
using Game.Attributes;

namespace Game.Data
{
    [CreateAssetMenu(fileName = nameof(DialogueConfig), menuName = "Game/" + nameof(DialogueConfig))]
    public class DialogueConfig : ScriptableObject
    {
        [field: Title("Dialogue Replicas", CustomColor.Cyan, CustomColor.Blue)]
        [field: SerializeField, TextArea(3, 10)] public string[] Replicas { get; private set; }
    }
}