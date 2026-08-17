using UnityEngine;


namespace Plugins
{
    public class MainCameraContraller : MonoBehaviour
    {
        private Camera _cam;
        private readonly Vector3 _initialPosition = new (0, 0, -1);
        private readonly Quaternion _initialRotation = Quaternion.Euler(0f, 0f, 0f);

        // 平滑缩放参数
        private int _zoomTimes;
        private const int ZoomMaxTimes = 64;
        private const float MaxZoomRatio = 0.08f;
        private const float MinZoomRatio = 0.0000001f;
        private const float MaxMoveStep = 0.0002f;

        private void Start()
        {
            _cam = GetComponent<Camera>();
        }

        private void LateUpdate()
        {
            var scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > 0.0f && _zoomTimes < ZoomMaxTimes)
            {
                var toCenter = ClickEvent.RotationCenter - transform.position;
                var zoomRatio = GetZoomRatio(_zoomTimes);
                var moveDistance = toCenter.magnitude * zoomRatio;

                transform.position += toCenter.normalized * moveDistance;
                _zoomTimes++;
            }
            else if (scroll < 0.0f && _zoomTimes > -ZoomMaxTimes)
            {
                var toCenter = ClickEvent.RotationCenter - transform.position;
                var zoomRatio = GetZoomRatio(_zoomTimes - 1);

                // 缩小使用放大的逆运算，使相反方向操作能够回到原距离。
                var moveDistance = toCenter.magnitude * zoomRatio / (1f - zoomRatio);
                transform.position -= toCenter.normalized * moveDistance;
                _zoomTimes--;
            }

            var moveStep = GetMoveStep();
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                transform.position += transform.right * moveStep;
            }

            if (Input.GetKey(KeyCode.RightArrow))
            {
                transform.position -= transform.right * moveStep;
            }

            if (Input.GetKey(KeyCode.UpArrow))
            {
                transform.position -= transform.up * moveStep;
            }

            if (Input.GetKey(KeyCode.DownArrow))
            {
                transform.position += transform.up * moveStep;
            }

            if (Input.GetMouseButton(1))
            {
                transform.RotateAround(
                    ClickEvent.RotationCenter, new Vector3(0, 1, 0), Input.GetAxis("Mouse X") * 6
                );

                transform.RotateAround(
                    ClickEvent.RotationCenter, transform.right, -Input.GetAxis("Mouse Y") * 6
                );
            }
        }

        public bool GetPoint(out Vector3 point)
        {
            var result = Physics.Raycast(_cam.ScreenPointToRay(Input.mousePosition), out var raycast);
            point = raycast.point;
            return result;
        }

        public bool GetTextureUv(out Vector2 uv)
        {
            var result = Physics.Raycast(_cam.ScreenPointToRay(Input.mousePosition), out var raycast);
            uv = raycast.textureCoord;
            return result;
        }

        public void ResetTransform()
        {
            ResetZoomState();
        }

        /// <summary>
        /// 恢复脚本初始化时的相机位置，并清空缩放次数。
        /// </summary>
        public void ResetZoomState()
        {
            transform.position = _initialPosition;
            transform.rotation = _initialRotation;
            _zoomTimes = 0;
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
                x = pos.x,
                y = pos.y,
                z = pos.z
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

        private static float GetZoomRatio(int lowerLevel)
        {
            int count;

            if (lowerLevel >= 0)
            {
                count = lowerLevel;
            }
            else
            {
                count = -lowerLevel - 1;
            }

            var progress = Mathf.Clamp01(count / (float)(ZoomMaxTimes - 1));
            return Mathf.SmoothStep(MaxZoomRatio, MinZoomRatio, progress);
        }

        private float GetMoveStep()
        {
            var zoomRatio = GetZoomRatio(Mathf.Abs(_zoomTimes));
            return MaxMoveStep * (zoomRatio / MaxZoomRatio);
        }
    }
}
