using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Plugins.models;


namespace Editor.PrefabEditor
{
    // 编辑预制体模型相关属性的脚本
    public class HumanMeshEditor
    {
        private readonly string[] _fbxFiles;

        // 构造方法：传入文件夹地址数组决定网格的查找范围
        public HumanMeshEditor(string[] fbxFolders)
        {
            var fbxFiles =
                from fileGuid in AssetDatabase.FindAssets("t:Model", fbxFolders)
                let file = AssetDatabase.GUIDToAssetPath(fileGuid)
                where file.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase)
                select file;

            _fbxFiles = fbxFiles.ToArray();
        }

        // 查询模型预制体结构与网格id是否一致
        private void MeshCkecker(Transform prefab)
        {
            if (!BodyStruct.GetFromPrefab(prefab.name, out var body)) return;

            var stepNumber = 0;
            MeshFilter mesh = null;
            SkinnedMeshRenderer skMesh = null;

            try
            {
                var meshName = "";

                if (prefab.TryGetComponent(out mesh))
                {
                    stepNumber = 1;
                    meshName = mesh.sharedMesh.name.Trim();
                }
                else if (prefab.TryGetComponent(out skMesh))
                {
                    stepNumber = 2;
                    meshName = skMesh.sharedMesh.name.Trim();
                }

                if (meshName != body.Value.ToString())
                {
                    Debug.LogError($"网格问题：{body.Name}: {meshName} ==> {body.Value}");
                }
            }
            catch (NullReferenceException)
            {
                var meshResult = false;

                if (stepNumber == 1)
                {
                    meshResult = FindAndSetMesh(mesh, body);
                }
                else if (stepNumber == 2)
                {
                    meshResult = FindAndSetMesh(skMesh, body);
                }

                if (!meshResult)
                {
                    Debug.LogError($"网格为空：{body.Name}, {body.Value}");
                }
            }
        }

        // 传入关联网格的component和模型数据结构，自动根据id查找网格并赋值
        private bool FindAndSetMesh<T>(T component, BodyStruct body) where T : Component
        {
            // 根据不同模型采用不同设置方法
            Action<Mesh> setMesh;
            if (component is MeshFilter meshFilter)
            {
                setMesh = mesh => meshFilter.sharedMesh = mesh;
            }
            else if (component is SkinnedMeshRenderer meshRenderer)
            {
                setMesh = mesh => meshRenderer.sharedMesh = mesh;
            }
            else
            {
                return false;
            }

            // 遍历所有fbx文件查找匹配的mesh
            var value = body.Value.ToString();
            foreach (var fbxFile in _fbxFiles)
            {
                foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(fbxFile))
                {
                    if (obj is Mesh mesh && mesh.name == value)
                    {
                        setMesh(mesh);
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
