using System;
using System.Collections.Generic;


namespace Plugins.C_.models
{
    [Serializable]
    public class AtlasItem
    {
        public string name;
        public int gender;
        public int boneMarkType;
        public string[] modelDisplayed;
        public string[] modelTranslucent;
        public float cameraPositionX;
        public float cameraPositionY;
        public float cameraPositionZ;
        public float cameraRotationX;
        public float cameraRotationY;
        public float cameraRotationZ;
        public List<int> types;
        public List<AtlasGroup> groups;
    }

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

    [Serializable]
    public class AtlasLabel
    {
        public string name;
        public int value;
        public int location;
        public int orderNum;
        public float pointPositionX;
        public float pointPositionY;
        public float pointPositionZ;
    }
}
