using System;
using UnityEngine;
using System.Collections.Generic;


namespace Plugins.models
{
    [Serializable]
    public class BoneMaps
    {
        public Texture2D essence;
        public Dictionary<int, Texture2D> invisible = new();
        public Dictionary<int, Texture2D> displayed = new();
    }

    [Serializable]
    public class Bonemark
    {
        public int type;
        public int value;
        public string name;
        public string color;
        public int planeValue;
        public float uvx;
        public float uvy;
        public float cameraPositionX;
        public float cameraPositionY;
        public float cameraPositionZ;
        public float cameraRotationX;
        public float cameraRotationY;
        public float cameraRotationZ;
    }

    [Serializable]
    public class Bone
    {
        public int gender;
        public string[] family;
        public List<Bonemark> bonemarks;
    }

    public sealed class BoneFactory : TextAssetFactory<Bone>
    {
        protected override string FilePrefix => "Bonemark_";

        protected override string GetDefaultAssetName(Bone item)
        {
            return item.family[0];
        }

        private static readonly BoneFactory Instance = new();

        public static bool Load(string assetName, out Bone item)
        {
            return Instance.LoadAsset(assetName, out item);
        }

        public static void Save(Bone item, string assetName = null)
        {
            Instance.SaveAsset(item, assetName);
        }
    }
}
