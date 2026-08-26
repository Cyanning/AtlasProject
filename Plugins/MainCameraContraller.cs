using UnityEngine;


namespace Plugins
{
    public class MainCameraContraller : MonoBehaviour
    {
        private Camera _cam;

        // 缩放参数：每次滚轮操作按当前相机到模型中心的距离缩放。
        private const float ZoomStepRatio = 0.08f;
        // 防止相机穿过模型中心。
        private const float MinZoomDistance = 0.0001f;

        // 平滑移动参数
        private const float MaxMoveStep = 0.00005f;
        // 达到该距离后使用最大平移步长，距离更远时不再继续增大。
        private const float MoveFullSpeedDistance = 1f;
        // 数值越大，靠近模型后的平移步长衰减越快。
        private const float MoveStepFalloffPower = 1.5f;

        private void Start()
        {
            _cam = GetComponent<Camera>();
        }

        private void Update()
        {
            var scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0.0f)
            {
                var toCenter = ClickEvent.RotationCenter - transform.position;
                var distanceToCenter = toCenter.magnitude;

                // 相机与旋转中心重合时没有有效方向，无法继续缩放。
                if (distanceToCenter > Mathf.Epsilon)
                {
                    var directionToCenter = toCenter / distanceToCenter;
                    var targetDistance = scroll > 0.0f
                        ? distanceToCenter * (1f - ZoomStepRatio)
                        : distanceToCenter / (1f - ZoomStepRatio);

                    if (scroll > 0.0f)
                    {
                        targetDistance = Mathf.Max(targetDistance, MinZoomDistance);
                    }

                    transform.position = ClickEvent.RotationCenter - directionToCenter * targetDistance;
                }
            }

            if (Input.GetKey(KeyCode.A))
            {
                transform.position += transform.right * GetMoveStep();
            }

            if (Input.GetKey(KeyCode.D))
            {
                transform.position -= transform.right * GetMoveStep();
            }

            if (Input.GetKey(KeyCode.W))
            {
                transform.position -= transform.up * GetMoveStep();
            }

            if (Input.GetKey(KeyCode.S))
            {
                transform.position += transform.up * GetMoveStep();
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
        /// 恢复脚本初始化时的相机位置。
        /// </summary>
        public void ResetZoomState()
        {
            transform.position = new Vector3(0, 0, -1);
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }

        public void SetCameraTransform(Vector3 pos, Vector3 rot)
        {
            _cam.transform.position = new Vector3(pos.x, pos.y, pos.z);
            _cam.transform.rotation = Quaternion.Euler(rot.x, rot.y, rot.z);
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

        private float GetMoveStep()
        {
            var distanceToCenter = Vector3.Distance(transform.position, ClickEvent.RotationCenter);
            var normalizedDistance = Mathf.Clamp01(distanceToCenter / MoveFullSpeedDistance);
            return MaxMoveStep * Mathf.Pow(normalizedDistance, MoveStepFalloffPower);
        }
    }
}
