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
}
