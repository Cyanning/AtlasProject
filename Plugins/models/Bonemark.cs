using System;
using System.Collections.Generic;
using Plugins.orm.Models;


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

        public bool BeForamen => string.IsNullOrEmpty(color) && planeValue > 0;
        public bool BePainting => !string.IsNullOrEmpty(color) && planeValue == 0;
    }

    [Serializable]
    public class Bones
    {
        public int gender;
        public int[] family;
        public List<Bonemark> bonemarks;

        public void GetMarksFromOrm(IEnumerable<Bonemarks> marksData)
        {
            bonemarks ??= new List<Bonemark>();
            foreach (var mark in marksData)
            {
                bonemarks.Add(
                    new Bonemark
                    {
                        type = mark.Type, value = mark.Value, color = mark.Color
                        , planeValue = mark.PlaneValue ?? 0, uvx = mark.Uvx, uvy = mark.Uvy, name = mark.Name
                        , cameraPositionX = mark.CameraPositionX, cameraPositionY = mark.CameraPositionY
                        , cameraPositionZ = mark.CameraPositionZ, cameraRotationX = mark.CameraRotationX
                        , cameraRotationY = mark.CameraRotationY, cameraRotationZ = mark.CameraRotationZ
                    }
                );
            }
        }

        public int SavingMark(Bonemark newMark, int index = -1)
        {
            if (index == -1)
            {
                bonemarks.Add(newMark);
                return bonemarks.Count - 1;
            }

            newMark.name = bonemarks[index].name;
            bonemarks[index] = newMark;
            return index;
        }
    }

    public sealed class BoneFactory : TextAssetFactory<Bones>
    {
        protected override string FilePrefix => "Bonemark_";
        protected override string RootFolder => "TemporaryFiles/Bonemarks";

        protected override string GetDefaultAssetName(Bones item)
        {
            return item.family[0].ToString();
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
