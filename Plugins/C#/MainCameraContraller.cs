using System;
using UnityEngine;
using Update = Unity.VisualScripting.Update;


namespace Plugins.C_
{
    public class MainCameraContraller : MonoBehaviour
    {
        private Camera _cam;
        private GameObject _canvas;

        private void Start()
        {
            _cam = GetComponent<Camera>();
            _canvas = GameObject.FindGameObjectWithTag("GameController");
        }


        private void Update()
        {
            if (!Input.GetMouseButtonDown(0)) return;

            if (!Physics.Raycast(_cam.ScreenPointToRay(Input.mousePosition), out var raycast)) return;

            _canvas.GetComponent<AtlasWorkflows>().Clicked(raycast);
        }

        public void SetCameraTransform(float posX, float posY, float posZ, float rotX, float rotY, float rotZ)
        {
            _cam.transform.position = new Vector3(posX, posY, posZ);
            _cam.transform.rotation = Quaternion.Euler(rotX, rotY, rotZ);
        }

        public Vector3 GetMainCameraPostion()
        {
            return _cam.transform.position;
        }

        public Vector3 GetMainCameraRotation()
        {
            var eulerAngles = _cam.transform.eulerAngles;
            return new Vector3()
            {
                x = eulerAngles.x > 180f ? eulerAngles.x - 360f : eulerAngles.x,
                y = eulerAngles.y > 180f ? eulerAngles.y - 360f : eulerAngles.y,
                z = eulerAngles.z > 180f ? eulerAngles.z - 360f : eulerAngles.z
            };
        }

    }
}
