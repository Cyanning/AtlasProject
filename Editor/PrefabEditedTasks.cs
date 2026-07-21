using UnityEditor;
using UnityEngine;
using Editor.PrefabEditor;
using System.Collections.Generic;


namespace Editor
{
    public class PrefabEditedTasks : MonoBehaviour
    {
        [MenuItem("Customer/预制体处理脚本")]
        // Unity 菜单入口：查找模型根节点并启动树结构比较。
        public static void CheckingPrefabStructure()
        {
            var bodyMale = GameObject.Find("BodyMale");
            if (bodyMale is null)
            {
                Debug.LogError("未找到根节点：BodyMale");
                return;
            }

            CompareTrees.Run(bodyMale.transform);
        }

        [MenuItem("Customer/批量修改材质")]
        // Unity 菜单入口：查找模型根节点并启动树结构比较。
        public static void ChangeMaterials()
        {
            var bodyMale = GameObject.Find("BodyFemale");
            if (bodyMale is null)
            {
                Debug.LogError("未找到根节点：BodyMale");
                return;
            }

            var root = bodyMale.transform;
            var targets = new List<string> { "骨骼系统~101000", "结缔组织~111000", "肌肉系统~121000", "泌尿生殖~201000" };
            var materialEditor = new HumanMaterialsEditor();

            for (var i = 0; i < root.childCount; i++)
            {
                var brench = root.GetChild(i);
                if (!targets.Contains(brench.name)) continue;

                var num = materialEditor.ReplaceMaterials(brench.gameObject);
                Debug.Log($"{brench.name} 已修改 {num} 个模型的材质");
            }
        }


    }

}
