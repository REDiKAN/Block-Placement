using UnityEngine;

namespace Game.Data
{
    [System.Serializable]
    public class DialogueReplica
    {
        [field: SerializeField, TextArea(1, 5)] public string Text { get; private set; }
        [field: SerializeField] public AudioClip Audio { get; private set; }
    }
}