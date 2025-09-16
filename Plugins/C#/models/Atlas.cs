using System;
using System.Collections.Generic;


namespace Plugins.C_.models
{
    [Serializable]
    public class Atlas
    {
        public int gender;
        public int boneMarkType;
        public List<string> modelDisplayed;
        public List<string> modelTranslucent;
        public float cameraPositionX;
        public float cameraPositionY;
        public float cameraPositionZ;
        public float cameraRotationX;
        public float cameraRotationY;
        public float cameraRotationZ;
    }
}
