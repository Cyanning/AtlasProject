using UnityEngine;


/// <summary>
/// 脚本挂载新建相机上 —— 
/// </summary>

namespace Plugins.C_
{
    public class Smooth3DCamera : MonoBehaviour
    {
        public Transform pivot;
        public Vector3 pivotOffset = Vector3.zero;
        public Transform target;
        public float distance = 10.0f;
        public float minDistance = 0.03f;
        public float maxDistance = 100f;
        public float zoomSpeed = 0.01f;
        public float xSpeed = 250.0f;
        public float ySpeed = 250.0f;
        public bool allowYTilt = true;
        public float yMinLimit = -360f;
        public float yMaxLimit = 360f;
        private float x = 0.0f;
        private float y = 0.0f;
        private float targetX = 0f;
        private float targetY = 0f;
        public float targetDistance = 0f;
        private float xVelocity = 0.01f;
        private float yVelocity = 0.01f;
        private float zoomVelocity = 1f;
        ClickEvent clickEvent;
        public Camera mainCamera;

        private void Start()
        {
            var angles = transform.eulerAngles;
            targetX = x = angles.x;
            targetY = y = ClampAngle(angles.y, yMinLimit, yMaxLimit);
            targetDistance = distance;

            clickEvent = transform.GetComponent<ClickEvent>();
        }


        private void LateUpdate()
        {
            if (!pivot) return;
            var scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > 0.0f)
            {
                //Debug.Log(scroll);

                Vector3 tmp = new Vector3(0, 0, -1 * (0.025f));

                Vector3 position = transform.position - transform.rotation * tmp;
                transform.position = position;


                // clickEvent.updateLines();
            }
            else if (scroll < 0.0f)
            {
                Vector3 tmp = new Vector3(0, 0, 1 * (0.025f));

                Vector3 position = transform.position - transform.rotation * tmp;
                transform.position = position;
                // clickEvent.updateLines();
            }

            if (Input.GetKey(KeyCode.LeftArrow))
            {
                transform.Translate(transform.right * 0.0001f, Space.World);
                // clickEvent.updateLines();
            }

            if (Input.GetKey(KeyCode.RightArrow))
            {
                transform.Translate(transform.right * -0.0001f, Space.World);

                //clickEvent.updateLines();
            }

            if (Input.GetKey(KeyCode.UpArrow))
            {
                transform.Translate(transform.up * -0.0001f, Space.World);

                // clickEvent.updateLines();
            }

            if (Input.GetKey(KeyCode.DownArrow))
            {
                transform.Translate(transform.up * 0.0001f, Space.World);
                // clickEvent.updateLines();
            }

            if (Input.GetMouseButton(1) || (Input.GetMouseButton(0) &&
                                            (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))))
            {
                if (transform.up.y > 0)
                {
                    transform.RotateAround(ClickEvent.RotationCenter, new Vector3(0, 1, 0),
                        Input.GetAxis("Mouse X") * 6);
                }
                else
                {
                    transform.RotateAround(ClickEvent.RotationCenter, new Vector3(0, 1, 0),
                        Input.GetAxis("Mouse X") * 6);
                }

                transform.RotateAround(ClickEvent.RotationCenter, transform.right, -Input.GetAxis("Mouse Y") * 6);

                // clickEvent.updateLines();
            }
        }


        private static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360) angle += 360;
            if (angle > 360) angle -= 360;
            return Mathf.Clamp(angle, min, max);
        }
    }
}
