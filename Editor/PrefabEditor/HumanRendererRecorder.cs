using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Plugins.models;


namespace Editor.PrefabEditor
{
    [Serializable]
    public class MaterialWrapper
    {
        public string name;
        public string albe;
        public string normal;
    }

    [Serializable]
    public class RendererWrapper
    {
        public int value;
        public List<MaterialWrapper> materials;
    }

    [Serializable]
    public class BodyRenderersWrapper
    {
        public List<RendererWrapper> renderers = new();

        public void SaveJson(string filePath)
        {
            File.WriteAllText(filePath, JsonUtility.ToJson(this, true));
        }

        public static bool LoadJson(string filePath, out BodyRenderersWrapper wrapper)
        {
            wrapper = JsonUtility.FromJson<BodyRenderersWrapper>(File.ReadAllText(filePath));
            return wrapper.renderers.Count > 0;
        }

        public RendererWrapper GetRendererRecord(int bodyValue)
        {
            var result =
                from record in renderers
                where record != null && record.value == bodyValue
                select record;

            return result.FirstOrDefault();
        }
    }

    /// <summary>
    /// 记录 Renderer 的共享材质和贴图，并将记录缓存为 JSON。
    /// </summary>
    public static class HumanRendererRecorder
    {
        private static readonly int ShaderIDTextureSurface = Shader.PropertyToID("_albe");
        private static readonly int ShaderIDTextureNormal = Shader.PropertyToID("_normal");

        /// <summary>
        /// 构建 MaterrialWrapper 并追加到缓存。
        /// </summary>
        private static bool RecordRenderer(GameObject target, out RendererWrapper rendererWrapper)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            rendererWrapper = null;

            if (!target.TryGetComponent(out Renderer renderer))
            {
                Debug.LogWarning($"无Renderer挂载：{target.name}");
                return false;
            }

            if (!BodyStruct.GetFromPrefab(target.name, out var body))
            {
                Debug.LogWarning($"模型名称有问题：{target.name}");
                return false;
            }

            rendererWrapper = new RendererWrapper()
            {
                value = body.Value,
                materials = new List<MaterialWrapper>()
            };

            var sharedMaterials = renderer.sharedMaterials;
            if (sharedMaterials.Length == 0)
            {
                Debug.LogWarning($"无材质球：{target.name}");
                return false;
            }

            foreach (var material in sharedMaterials)
            {
                var tex = new MaterialWrapper
                {
                    name = material.name,
                    albe = GetTextureAssetName(material, ShaderIDTextureSurface),
                    normal = GetTextureAssetName(material, ShaderIDTextureNormal)
                };
                if (string.IsNullOrEmpty(tex.albe) && string.IsNullOrEmpty(tex.normal))
                {
                    Debug.LogWarning($"无贴图：{target.name}");
                }

                rendererWrapper.materials.Add(tex);
            }

            return true;
        }

        private static string GetTextureAssetName(Material material, int textureID)
        {
            return material.HasProperty(textureID)
                ? Path.GetFileName(AssetDatabase.GetAssetPath(material.GetTexture(textureID)))
                : "";
        }

        public static BodyRenderersWrapper RecordChildren(GameObject rootGo)
        {
            BodyRenderersWrapper renderers = new();
            foreach (var target in rootGo.GetComponentsInChildren<GameObject>())
            {
                if (RecordRenderer(target, out var rendererWrapper))
                {
                    renderers.renderers.Add(rendererWrapper);
                }
            }

            return renderers;
        }
    }
}
