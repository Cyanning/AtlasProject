using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization;

namespace Plugins.C_.models
{
    [DataContract]
    public class ObjectColorModel
    {
        public GameObject obj;
        public Color LastColor;
        public string LastTextureName;
    }
}
