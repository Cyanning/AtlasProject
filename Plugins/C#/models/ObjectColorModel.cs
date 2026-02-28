using UnityEngine;
using System.Runtime.Serialization;

namespace Plugins.C_.models
{
    [DataContract]
    public class ObjectColorModel
    {
        public GameObject Obj;
        public Color LastColor;
        public string LastTextureName;
    }
}
