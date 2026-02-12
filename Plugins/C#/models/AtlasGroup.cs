using System;
using System.Collections.Generic;

namespace Plugins.C_.models
{
    [Serializable]
    public class AtlasGroup
    {
        public float cameraPositionX;
        public float cameraPositionY;
        public float cameraPositionZ;
        public float cameraRotationX;
        public float cameraRotationY;
        public float cameraRotationZ;
        public List<AtlasLabel> labels;
    }
}
