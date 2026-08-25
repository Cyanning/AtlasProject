using UnityEngine;


namespace Plugins
{
    public class MainCameraContraller : MonoBehaviour
    {
        private Camera _cam;

        // 平滑缩放参数
        private int _zoomTimes;
        private const int ZoomMaxTimes = 64;
        private const float MaxZoomRatio = 0.08f;
        private const float MinZoomRatio = 0.0000001f;
        private const float MaxMoveStep = 0.00002f;

        private void Start()
        {
            _cam = GetComponent<Camera>();
        }

        private void Update()
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
            if (Input.GetKey(KeyCode.A))
            {
                transform.position += transform.right * moveStep;
            }

            if (Input.GetKey(KeyCode.D))
            {
                transform.position -= transform.right * moveStep;
            }

            if (Input.GetKey(KeyCode.W))
            {
                transform.position -= transform.up * moveStep;
            }

            if (Input.GetKey(KeyCode.S))
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

        /// <summary>
        /// 恢复脚本初始化时的相机位置，并清空缩放次数。
        /// </summary>
        public void ResetZoomState()
        {
            transform.position = new Vector3(0, 0, -1);
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            ResetZoomTimes();
        }

        /// <summary>
        /// 保留当前相机位置和旋转，只清空缩放次数。
        /// </summary>
        public void ResetZoomTimes()
        {
            _zoomTimes = 0;
        }

        public void SetCameraTransform(Vector3 pos, Vector3 rot)
        {
            _cam.transform.position = new Vector3(pos.x, pos.y, pos.z);
            _cam.transform.rotation = Quaternion.Euler(rot.x, rot.y, rot.z);
            ResetZoomTimes();
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
