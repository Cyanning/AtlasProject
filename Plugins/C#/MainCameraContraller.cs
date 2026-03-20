using System;
using UnityEngine;


namespace Plugins.C_
{
    public class MainCameraContraller : MonoBehaviour
    {
        private Camera _cam;

        private void Start()
        {
            _cam = GetComponent<Camera>();
        }

        public bool GetPoint(out Vector3 point)
        {
            var result = Physics.Raycast(_cam.ScreenPointToRay(Input.mousePosition), out var raycast);
            point = raycast.point;
            return result;
        }

        public void SetCameraTransform(float posX, float posY, float posZ, float rotX, float rotY, float rotZ)
        {
            _cam.transform.position = new Vector3(posX, posY, posZ);
            _cam.transform.rotation = Quaternion.Euler(rotX, rotY, rotZ);
        }

        public Vector3 GetMainCameraPostion()
        {
            var pos = _cam.transform.position;
            return new Vector3
            {
                x = (float)Math.Round(pos.x, 2, MidpointRounding.AwayFromZero),
                y = (float)Math.Round(pos.x, 2, MidpointRounding.AwayFromZero),
                z = (float)Math.Round(pos.x, 2, MidpointRounding.AwayFromZero)
            };
        }

        public Vector3 GetMainCameraRotation()
        {
            var eulerAngles = _cam.transform.eulerAngles;
            return new Vector3
            {
                x = eulerAngles.x > 180f ? eulerAngles.x - 360f : eulerAngles.x,
                y = eulerAngles.y > 180f ? eulerAngles.y - 360f : eulerAngles.y,
                z = eulerAngles.z > 180f ? eulerAngles.z - 360f : eulerAngles.z
            };
        }
    }
}
