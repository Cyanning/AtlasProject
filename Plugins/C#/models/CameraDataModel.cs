using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization;

namespace Plugins.C_.models
{
    [DataContract]
    public class CameraDataModel
    {
        [DataMember(Name = "camera_position_x")]
        public float CameraPositionX { get; set; }
        [DataMember(Name = "camera_position_y")]
        public float CameraPositionY { get; set; }
        [DataMember(Name = "camera_position_z")]
        public float CameraPositionZ { get; set; }


        [DataMember(Name = "camera_rotation_x")]
        public float CameraRotationX { get; set; }
        [DataMember(Name = "camera_rotation_y")]
        public float CameraRotationY { get; set; }
        [DataMember(Name = "camera_rotation_z")]
        public float CameraRotationZ { get; set; }
    }
}
