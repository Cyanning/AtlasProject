using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization;

namespace Plugins.C_.models
{
    [DataContract]
    public class MarkModel
    {

        [DataMember(Name = "type")]
        public int Type { get; set; }


        [DataMember(Name = "value")]
        public string Value { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "color")]
        public string Color { get; set; }

        [DataMember(Name = "en_name")]
        public string EnName { get; set; }

        [DataMember(Name = "introduction")]
        public string Introduction { get; set; }

        [DataMember(Name = "camera_position_x")]
        public string CameraPositionX { get; set; }
        [DataMember(Name = "camera_position_y")]
        public string CameraPositionY { get; set; }
        [DataMember(Name = "camera_position_z")]
        public string CameraPositionZ { get; set; }
        [DataMember(Name = "camera_rotation_x")]
        public string CameraRotationX { get; set; }
        [DataMember(Name = "camera_rotation_y")]
        public string CameraRotationY { get; set; }
        [DataMember(Name = "camera_rotation_z")]
        public string CameraRotationZ { get; set; }

        [DataMember(Name = "scale")]
        public string scale { get; set; }
        [DataMember(Name = "uvx")]
        public string UVX { get; set; }
        [DataMember(Name = "uvy")]
        public string UVY { get; set; }

    }
}
