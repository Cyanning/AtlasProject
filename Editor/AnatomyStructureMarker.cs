using UnityEditor;
using UnityEngine;
using Editor.PrefabEditor;

namespace Editor
{
    public class AnatomyStructureMarker : MonoBehaviour
    {
        [MenuItem("自定义功能/预制体处理脚本")]
        // Unity 菜单入口：查找模型根节点并启动树结构比较。
        public static void CheckingPrefab()
        {
            var bodyMale = GameObject.Find("BodyMale");
            if (bodyMale is null)
            {
                Debug.LogError("未找到根节点：BodyMale");
                return;
            }

            CompareTrees.Run(bodyMale.transform);
        }
    }
}
