using System;
using UnityEditor;
using UnityEngine;
using Plugins.C_;
using Plugins.C_.models;

namespace Editor
{
    public class PrefabEditor : MonoBehaviour
    {
        [MenuItem("自定义功能/数据库Test")]
        public static void CheckPrefabNames()
        {
            var bodyMale = GameObject.Find("BodyMale");
            foreach (var prefab in PrefabCollection.ForEachChildren(bodyMale.transform))
            {
                // 预制体名字是否正常
                if (BodyStruct.GetFromPrefab(prefab.name, out var body))
                {
                    // 是否能找到对应数据
                    if (AnatomyDatabase.FindBodyFromValue(body.value, out var info))
                    {
                        Debug.Log($"{info.Value}, {info.Name} 【✓】");
                    }
                    else
                    {
                        Debug.LogWarning($"库中找不到数据：{prefab.name}");
                    }

                    // 检查网格名字是否匹配
                    if (prefab.childCount == 0)
                    {
                        try
                        {
                            var meshName = "";

                            if (prefab.TryGetComponent(out MeshFilter mesh))
                            {
                                meshName = mesh.sharedMesh.name.Trim();
                            }
                            else if (prefab.TryGetComponent(out SkinnedMeshRenderer skMesh))
                            {
                                meshName = skMesh.sharedMesh.name.Trim();
                            }

                            if (meshName != body.value.ToString())
                            {
                                Debug.LogWarning($"网格名称不对应：{prefab.name} ≠ {meshName}");
                            }
                        }
                        catch (NullReferenceException)
                        {
                            Debug.LogError($"网格为空：{prefab.name}");
                        }
                    }
                }
                else if (!prefab.name.Contains("Point"))
                {
                    Debug.LogWarning($"预制体名称有问题：{prefab.name}");
                }
            }
        }
    }
}
