using System;
using System.Collections.Generic;

namespace Plugins.C_.models
{
    [Serializable]
    public class AtlasLableGroup
    {
        public List<AtlasLable> lables;
        public float cameraPositionX;
        public float cameraPositionY;
        public float cameraPositionZ;
        public float cameraRotationX;
        public float cameraRotationY;
        public float cameraRotationZ;
    }
}
