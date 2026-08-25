using System;
using UnityEngine;
using System.Collections.Generic;


namespace Plugins.models
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

        public Vector3 Position {
            get => new(cameraPositionX, cameraPositionY, cameraPositionZ);
            set {
                cameraPositionX = value.x;
                cameraPositionY = value.y;
                cameraPositionZ = value.z;
            }
        }
        public Vector3 Rotation {
            get => new(cameraRotationX, cameraRotationY, cameraRotationZ);
            set {
                cameraRotationX = value.x;
                cameraRotationY = value.y;
                cameraRotationZ = value.z;
            }
        }
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

        public Vector3 Position {
            get => new(cameraPositionX, cameraPositionY, cameraPositionZ);
            set {
                cameraPositionX = value.x;
                cameraPositionY = value.y;
                cameraPositionZ = value.z;
            }
        }
        public Vector3 Rotation {
            get => new(cameraRotationX, cameraRotationY, cameraRotationZ);
            set {
                cameraRotationX = value.x;
                cameraRotationY = value.y;
                cameraRotationZ = value.z;
            }
        }
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

        public Vector3 Point {
            get => new(pointPositionX, pointPositionY, pointPositionZ);
            set {
                pointPositionX = value.x;
                pointPositionY = value.y;
                pointPositionZ = value.z;
            }
        }
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
                _ => throw new ArgumentOutOfRangeException()
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
                        throw new ArgumentOutOfRangeException();
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

    public sealed class AtlasFactory : TextAssetFactory<AtlasItem>
    {
        // 专属内容重写
        protected override string FilePrefix => "Atlas_";
        protected override string ClassFolder => "AtlasData";

        protected override string GetDefaultAssetName(AtlasItem item)
        {
            return item.name;
        }

        protected override void ApplyUniformName(string assetName, AtlasItem item)
        {
            item.name = assetName;
        }

        private static readonly AtlasFactory Instance = new ();

        public static bool Load(string assetName, out AtlasItem item)
        {
            return Instance.LoadAsset(assetName, out item);
        }

        public static void Save(AtlasItem item, string assetName = null)
        {
            Instance.SaveAsset(item, assetName);
        }
    }
}
