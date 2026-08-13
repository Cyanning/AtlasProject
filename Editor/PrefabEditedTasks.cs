using UnityEditor;
using UnityEngine;
using Editor.PrefabEditor;
using System.Collections.Generic;
using Plugins.models;


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

        [MenuItem("Customer/批量修改材质（预制体）")]
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
            var materialEditor = new HumanMaterialsEditor(new("Nv"));

            for (var i = 0; i < root.childCount; i++)
            {
                var brench = root.GetChild(i);
                if (!targets.Contains(brench.name)) continue;

                var num = materialEditor.ProcessChildren(brench.gameObject);
                Debug.Log($"{brench.name} 已修改 {num} 个模型的材质");
            }
        }

        [MenuItem("Customer/批量修改材质（FBX）")]
        // Unity 菜单入口：查找模型根节点并启动树结构比较。
        public static void ChangeMaterialsForFbx()
        {
            var fbxPrefab = GameObject.Find("NvMiniao");
            var fbxBodyStruct = new BodyStruct(201000, "泌尿生殖");
            if (fbxPrefab is null)
            {
                Debug.LogError("未找到根节点：BodyMale");
                return;
            }

            var materialEditor = new HumanMaterialsEditor(new NameConverter("Nv"));

            var num = materialEditor.ProcessChildren(fbxPrefab, rootBody: fbxBodyStruct);
            Debug.Log($"{fbxPrefab.name} 已修改 {num} 个模型的材质");
        }

        [MenuItem("Customer/复制预制体结构")]
        // Unity 菜单入口：查找模型根节点并启动树结构比较。
        public static void CopyPrefab()
        {
            var origin = GameObject.Find("#ForamensMaleOld");
            var target = GameObject.Find("#ForamensFemale");

            PrefabEditorImitatively.PrefabNameRetailor(target);
            if (origin == null || target == null)
            {
                Debug.LogError("未找到根节点");
                return;
            }

            var prefabEditor = new PrefabEditorImitatively(target);

            prefabEditor.PrefabStructCopying(origin.transform, target.transform);
        }

        [MenuItem("Customer/设置Read-Write")]
        // Unity 菜单入口：查找模型根节点并启动树结构比较。
        public static void SetFileReadable()
        {
            AssetFileEditor.SetTextureReadWrite("Assets", @"^.+_mark\d+\.png$");
        }

    }
}
