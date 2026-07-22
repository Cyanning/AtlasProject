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
    }

    /// <summary>
    /// 记录 Renderer 的共享材质和贴图，并将记录缓存为 JSON。
    /// </summary>
    public sealed class HumanRendererRecorder
    {
        private static readonly int ShaderIDTextureSurface = Shader.PropertyToID("_albe");
        private static readonly int ShaderIDTextureNormal = Shader.PropertyToID("_normal");
        private BodyRenderersWrapper _renderers = new ();

        /// <summary>
        /// 构建 MaterrialWrapper 并追加到缓存。
        /// </summary>
        public void Record(GameObject target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            if (!BodyStruct.GetFromPrefab(target.name, out var body))
            {
                Debug.LogWarning($"模型名称有问题：{target.name}");
                return;
            }

            if (!target.TryGetComponent(out Renderer renderer))
            {
                Debug.LogWarning($"无Renderer挂载：{target.name}");
                return;
            }

            var wrapper = new RendererWrapper()
            {
                value = body.value,
                materials = new List<MaterialWrapper>()
            };

            var sharedMaterials = renderer.sharedMaterials;
            if (sharedMaterials.Length == 0)
            {
                Debug.LogWarning($"无材质球：{target.name}");
                return;
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

                wrapper.materials.Add(tex);
            }

            _renderers.renderers.Add(wrapper);
        }

        private static string GetTextureAssetName(Material material, int textureID)
        {
            return material.HasProperty(textureID)
                ? Path.GetFileName(AssetDatabase.GetAssetPath(material.GetTexture(textureID)))
                : "";
        }

        /// <summary>
        /// 保存为json
        /// </summary>
        public void SaveJson(string filePath)
        {
            File.WriteAllText(filePath, JsonUtility.ToJson(_renderers, true));
        }

        /// <summary>
        /// 读取 JSON 并反序列化为 RendererWrapper 对象。
        /// </summary>
        public bool LoadJson(string filePath)
        {
            _renderers = JsonUtility.FromJson<BodyRenderersWrapper>(File.ReadAllText(filePath));
            return _renderers.renderers.Count > 0;
        }

        public RendererWrapper GetRendererRecord(int bodyValue)
        {
            var result =
                from record in _renderers.renderers
                where record != null && record.value == bodyValue
                select record;

            return result.FirstOrDefault();
        }
    }
}
