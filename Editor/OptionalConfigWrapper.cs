using System;

namespace Editor
{
    [Serializable]
    public class AtlasTypes
    {
        public string[] systemClasses;
        public string[] regionClasses;
    }

    [Serializable]
    public class OptionalConfig
    {
        public string[] boneMarkClasses;
        public AtlasTypes atlasTypes;
    }
}
