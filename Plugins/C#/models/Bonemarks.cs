using System;
using UnityEngine;
using System.Collections.Generic;


namespace Plugins.C_.models
{
    [Serializable]
    public class BoneMaps
    {
        public Texture2D essence;
        public Dictionary<int, Texture2D> Invisible = new();
        public Dictionary<int, Texture2D> Displayed = new();
    }

    [Serializable]
    public class Bonemark
    {
        public int type;
        public int value;
        public string name;
        public string color;
        public float uvx;
        public float uvy;
        public float cameraPositionX;
        public float cameraPositionY;
        public float cameraPositionZ;
        public float cameraRotationX;
        public float cameraRotationY;
        public float cameraRotationZ;
    }
}
