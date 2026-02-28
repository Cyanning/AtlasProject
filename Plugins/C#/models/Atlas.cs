using System;
using System.Collections.Generic;


namespace Plugins.C_.models.Atlas
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

    [Serializable]
    public class Row
    {
        public string left;
        public string right;

        public string this[int location]
        {
            get => location switch
            {
                0 => left,
                1 => right,
                _ => throw new KeyNotFoundException()
            };
            set
            {
                switch (location)
                {
                    case 0:
                        left = value;
                        break;
                    case 1:
                        right = value;
                        break;
                    default:
                        throw new KeyNotFoundException();
                }
            }
        }

        public bool leftState;
        public bool rightState;

        public bool GetState(int location)
        {
            return location switch
            {
                0 => leftState,
                1 => rightState,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public void SetState(int location, bool state)
        {
            switch (location)
            {
                case 0:
                    leftState = state;
                    break;
                case 1:
                    rightState = state;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
