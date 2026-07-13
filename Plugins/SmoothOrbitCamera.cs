using UnityEngine;

namespace Plugins
{
    [RequireComponent(typeof(Camera))]
    public class SmoothOrbitCamera : MonoBehaviour
    {
        [Header("观察目标")]
        public Transform target;
        public Vector3 targetOffset;

        [Header("旋转设置 (鼠标右键)")]
        public float xSpeed = 20.0f;
        public float ySpeed = 20.0f;
        public float yMinLimit = -80f; // 限制仰角
        public float yMaxLimit = 80f;  // 限制俯角
        public float rotationDamping = 5.0f;

        [Header("缩放设置 (鼠标滚轮)")]
        public float zoomSpeed = 1.0f;
        public float minDistance = 0.01f;
        public float maxDistance = 20.0f;
        public float zoomDamping = 5.0f;

        [Header("平移设置 (方向键)")]
        public float panSpeed = 5.0f;
        public float panDamping = 5.0f;

        // 目标状态（用户输入驱动）
        private float _targetX;
        private float _targetY;
        private float _targetDistance = 5.0f;
        private Vector3 _targetCenter;

        // 当前状态（平滑插值过渡）
        private float _currentX;
        private float _currentY;
        private float _currentDistance = 5.0f;
        private Vector3 _currentCenter;

        private void Start()
        {
            // 初始化中心点
            if (target != null)
            {
                _targetCenter = target.position + targetOffset;
            }
            else
            {
                // 若无目标，以当前相机前方 5 米处作为虚拟观察点
                _targetCenter = transform.position + transform.forward * _targetDistance;
            }
            _currentCenter = _targetCenter;

            // 初始化角度
            var angles = transform.eulerAngles;
            _currentX = _targetX = angles.y;
            _currentY = _targetY = angles.x;

            _targetDistance = Vector3.Distance(transform.position, _targetCenter);
            _currentDistance = _targetDistance;
        }

        private void LateUpdate()
        {
            HandleInput();
            ApplySmoothMovement();
        }

        private void HandleInput()
        {
            // 1. 右键控制旋转角度
            if (Input.GetMouseButton(1))
            {
                _targetX += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
                _targetY -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
                _targetY = Mathf.Clamp(_targetY, yMinLimit, yMaxLimit);
            }

            // 2. 滚轮控制距离缩放
            var scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.001f)
            {
                _targetDistance -= scroll * zoomSpeed;
                _targetDistance = Mathf.Clamp(_targetDistance, minDistance, maxDistance);
            }

            // 3. 方向键控制中心点上下左右平移（基于相机自身的坐标系）
            var hPan = Input.GetAxisRaw("Horizontal"); // 左右方向键 (A/D 或 Left/Right)
            var vPan = Input.GetAxisRaw("Vertical");   // 上下方向键 (W/S 或 Up/Down)

            if (Mathf.Abs(hPan) > 0.1f || Mathf.Abs(vPan) > 0.1f)
            {
                // 计算相机右方和上方的屏幕空间向量，转换为世界坐标位移
                var panDirection = (transform.right * hPan + transform.up * vPan).normalized;
                _targetCenter += panDirection * (_targetDistance * 0.2f * panSpeed * Time.deltaTime);
                // 乘距离的 0.2 倍是为了让相机离物体远时平移变快，离得近时平移变慢，符合人类视觉习惯
            }
        }

        private void ApplySmoothMovement()
        {
            // 使用 Mathf.Lerp 对旋转、缩放、平移进行阻尼平滑
            _currentX = Mathf.Lerp(_currentX, _targetX, Time.deltaTime * rotationDamping);
            _currentY = Mathf.Lerp(_currentY, _targetY, Time.deltaTime * rotationDamping);
            _currentDistance = Mathf.Lerp(_currentDistance, _targetDistance, Time.deltaTime * zoomDamping);
            _currentCenter = Vector3.Lerp(_currentCenter, _targetCenter, Time.deltaTime * panDamping);

            // 根据计算出的平滑参数算最终位置
            var rotation = Quaternion.Euler(_currentY, _currentX, 0);

            // 核心公式：最终位置 = 观察中心点 - (旋转朝向 * 距离)
            var position = _currentCenter - (rotation * Vector3.forward * _currentDistance);

            // 应用给相机
            transform.rotation = rotation;
            transform.position = position;
        }

        // 外部重置视角接口（比如双击某个物体，相机平滑拉近聚焦）
        public void FocusOn(Transform newTarget)
        {
            if (newTarget == target) return;
            target = newTarget;
            _targetCenter = newTarget.position;
            _targetDistance = 5.0f; // 默认聚焦距离
        }
    }
}
