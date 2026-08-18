using SQLite;
using UnityEngine;

namespace Plugins.orm.Models
{
    [Table("bone_marks")]
    public class Bonemarks
    {
        [PrimaryKey, AutoIncrement, Column("id")]
        public int Id { get; set; }

        [Column("type")] public int Type { get; set; }
        [Column("value")] public int Value { get; set; }
        [Column("color")] public string Color { get; set; }
        [Column("plane_value")] public int? PlaneValue { get; set; }
        [Column("uvx")] public float Uvx { get; set; }
        [Column("uvy")] public float Uvy { get; set; }
        [Column("name")] public string Name { get; set; }
        [Column("camera_position_x")] public float CameraPositionX { get; set; }
        [Column("camera_position_y")] public float CameraPositionY { get; set; }
        [Column("camera_position_z")] public float CameraPositionZ { get; set; }
        [Column("camera_rotation_x")] public float CameraRotationX { get; set; }
        [Column("camera_rotation_y")] public float CameraRotationY { get; set; }
        [Column("camera_rotation_z")] public float CameraRotationZ { get; set; }

        [Ignore] public bool BeForamen => string.IsNullOrEmpty(Color) && PlaneValue > 0;
        [Ignore] public bool BePainting => !string.IsNullOrEmpty(Color) && PlaneValue == 0;

        [Ignore] public Vector3 Position {
            get => new(CameraPositionX, CameraPositionY, CameraPositionZ);
            set {
                CameraPositionX = value.x;
                CameraPositionY = value.y;
                CameraPositionZ = value.z;
            }
        }

        [Ignore] public Vector3 Rotation {
            get => new(CameraRotationX, CameraRotationY, CameraRotationZ);
            set {
                CameraRotationX = value.x;
                CameraRotationY = value.y;
                CameraRotationZ = value.z;
            }
        }
    }
}
