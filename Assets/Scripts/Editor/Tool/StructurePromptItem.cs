using System;
using Game.Data;

namespace Game.Editor.Tool
{
    [Serializable]
    public class StructurePromptItem
    {
        public StructureConfig Config;
        public int MaxCount = -1;
    }
}