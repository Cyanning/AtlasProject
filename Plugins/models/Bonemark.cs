using System;
using System.Collections.Generic;
using Plugins.models.orm;


namespace Plugins.models
{
    [Serializable]
    public class Bonemark
    {
        public int type;
        public int value;
        public string color;
        public int planeValue;
        public float uvx;
        public float uvy;
        public string name;
        public float cameraPositionX;
        public float cameraPositionY;
        public float cameraPositionZ;
        public float cameraRotationX;
        public float cameraRotationY;
        public float cameraRotationZ;
    }

    [Serializable]
    public class Bones
    {
        public int gender;
        public string[] family;
        public List<Bonemark> bonemarks;

        public void GetMarksFromOrm(IEnumerable<Bonemarks> marksData)
        {
            bonemarks ??= new();
            foreach (var mark in marksData)
            {
                bonemarks.Add(
                    new()
                    {
                        type = mark.Type, value = mark.Value, color = mark.Color
                        , planeValue = mark.PlaneValue, uvx = mark.Uvx, uvy = mark.Uvy, name = mark.Name
                        , cameraPositionX = mark.CameraPositionX, cameraPositionY = mark.CameraPositionY
                        , cameraPositionZ = mark.CameraPositionZ, cameraRotationX = mark.CameraRotationX
                        , cameraRotationY = mark.CameraRotationY, cameraRotationZ = mark.CameraRotationZ
                    }
                );
            }
        }
    }

    public sealed class BoneFactory : TextAssetFactory<Bones>
    {
        protected override string FilePrefix => "Bonemark_";

        protected override string GetDefaultAssetName(Bones item)
        {
            return item.family[0];
        }

        private static readonly BoneFactory Instance = new();

        public static bool Load(string assetName, out Bones item)
        {
            return Instance.LoadAsset(assetName, out item);
        }

        public static void Save(Bones item, string assetName = null)
        {
            Instance.SaveAsset(item, assetName);
        }
    }
}
