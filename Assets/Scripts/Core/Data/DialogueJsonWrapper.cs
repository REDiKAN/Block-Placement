using System;

namespace Game.Data
{
    [Serializable]
    public class DialogueJsonWrapper
    {
        public DialogueReplicaWrapper[] Replicas;
    }

    [Serializable]
    public class DialogueReplicaWrapper
    {
        public string Text;
        public string AudioClipName;
    }
}