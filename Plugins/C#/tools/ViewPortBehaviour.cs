using UnityEngine;
using UnityEngine.UI;

namespace Plugins.C_.tools
{
    public class ViewPortBehaviour : MonoBehaviour
    {
        public Text txtInfo;
        private Camera camera;
        private string json;
        private string modelName;

        private void Start()
        {
            camera = Camera.main;
        }

        void Update()
        {
            Vector2 startPos = Input.mousePosition;
            var ray = camera.ScreenPointToRay(startPos);
            if (Physics.Raycast(ray, out var rayHitInfo)) //使用默认射线长度和其他默认参数
            {
                var hitPoint = rayHitInfo.point;
                var cTp = camera.transform.position;
                var ctr = camera.transform.eulerAngles;

                modelName = rayHitInfo.transform.name;
                json = "{\"camera_position_x\":" + cTp.x + ",\n\"camera_position_y\":" + cTp.y +
                       ",\n\"camera_position_z\":" + cTp.z + ",\n\"camera_rotation_x\":" + ctr.x +
                       ",\n\"camera_rotation_y\":" + ctr.y + ",\n\"camera_rotation_z\":" + ctr.z + ",\n\"target_x\":" +
                       hitPoint.x + ",\n\"target_y\": " + hitPoint.y + ",\n\"target_z\":" + hitPoint.z + "}";
                txtInfo.text = json;
            }

            if (Input.GetMouseButtonDown(0))
            {
            }
            else if (Input.GetMouseButton(0))
            {
            }
            else if (Input.GetMouseButtonUp(0))
            {
                System.IO.File.WriteAllText(@$"D:\\Anatomy_documents\altas\item1\{modelName}.txt", json);
            }
        }
    }
}
