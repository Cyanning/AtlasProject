using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization;


namespace Plugins.C_.models
{
    [DataContract]
    public class ShowModel
    {
        [DataMember(Name = "position_x")] public float PositionX { get; set; }
        [DataMember(Name = "position_y")] public float PositionY { get; set; }
        [DataMember(Name = "position_z")] public float PositionZ { get; set; }


        [DataMember(Name = "rotation_x")] public float RotationX { get; set; }
        [DataMember(Name = "rotation_y")] public float RotationY { get; set; }
        [DataMember(Name = "rotation_z")] public float RotationZ { get; set; }

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

        [DataMember(Name = "scale")] public float Scale { get; set; }
        [DataMember(Name = "model_name")] public string ModelName { get; set; }
        [DataMember(Name = "alpha_name")] public string AlphaName { get; set; }

        [DataMember(Name = "rotation_center_x")]
        public float RotationCenterX { get; set; }

        [DataMember(Name = "rotation_center_y")]
        public float RotationCenterY { get; set; }

        [DataMember(Name = "rotation_center_z")]
        public float RotationCenterZ { get; set; }

        [DataMember(Name = "sex")] public int Sex { get; set; }
        [DataMember(Name = "model_path")] public int ModelPath { get; set; }

        [DataMember(Name = "device_type")] public string DeviceType { get; set; }
    }
}
