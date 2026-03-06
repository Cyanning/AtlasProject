using System;
using System.IO;
using UnityEngine;
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

    public static class AtlasFactory
    {
        // folderName: Atlas_database / {folder} / {atlas name}

        private static string AtlasPath(string folderName)
        {

            var folders = folderName.Split(
                new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
                StringSplitOptions.RemoveEmptyEntries
            );
            folders[^1] = $"Atlas_{folders[^1]}.json";
            return Path.Combine(
                Application.dataPath, "Atlas_database", string.Join(Path.DirectorySeparatorChar, folders)
            );
        }

        private static string AtlasName(string folderName)
        {
            var index = folderName.LastIndexOfAny(
                new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }
            );
            return  folderName[index..];
        }

        public static bool Load(string folderName, out AtlasItem atlas)
        {
            var path = AtlasPath(folderName);
            if (File.Exists(path))
            {
                atlas = JsonUtility.FromJson<AtlasItem>(File.ReadAllText(path));
                return true;
            }

            atlas = new AtlasItem { name = AtlasName(folderName) };
            return false;
        }

        public static bool Save(AtlasItem atlas, string folderName=null, bool uniformName=true)
        {
            // 若 uniformName 为 true（默认），则将地址中的名字赋值 atlas的 name属性
            if (folderName is null)
            {
                folderName = atlas.name;
            }
            else if (uniformName)
            {
                atlas.name = AtlasName(folderName);
            }

            var path =  AtlasPath(folderName);
            if (File.Exists(path))
            {
                File.WriteAllText(path, JsonUtility.ToJson(atlas));
                return true;
            }

            return false;
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
