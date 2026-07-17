using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Plugins.models;
using UnityEngine;


namespace Editor.PrefabEditor
{
    [Serializable]
    public class MaterrialWrapper
    {
        public string name;
        [CanBeNull] public Texture albe;
        [CanBeNull] public Texture normal;
    }

    [Serializable]
    public class RendererWrapper
    {
        public int value;
        public List<MaterrialWrapper> materials;
    }

    [Serializable]
    public class AllRenderers
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
        private AllRenderers AllRenderers { get; } = new();

        /// <summary>
        /// 构建 RendererWrapper 并追加到缓存。
        /// </summary>
        public void Record(GameObject target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            if (!target.TryGetComponent(out Renderer renderer)) return;

            if (!BodyStruct.GetFromPrefab(target.name, out var body)) return;

            var wrapper = new RendererWrapper
            {
                value = body.value,
                materials = new List<MaterrialWrapper>()
            };

            foreach (var material in renderer.sharedMaterials)
            {
                if (material == null) continue;
                wrapper.materials.Add(
                    new MaterrialWrapper
                    {
                        name = material.name,
                        albe = material.HasProperty(ShaderIDTextureSurface)
                            ? material.GetTexture(ShaderIDTextureSurface)
                            : null,
                        normal = material.HasProperty(ShaderIDTextureNormal)
                            ? material.GetTexture(ShaderIDTextureNormal)
                            : null
                    }
                );
            }

            AllRenderers.renderers.Add(wrapper);
        }

        /// <summary>
        /// 保存为json
        /// </summary>
        public void SaveJson(string filePath)
        {
            File.WriteAllText(filePath, JsonUtility.ToJson(AllRenderers, true));
            AllRenderers.renderers.Clear();
        }

        /// <summary>
        /// 读取 JSON 并反序列化为 AllRenderers 对象。
        /// </summary>
        public AllRenderers LoadJson(string filePath)
        {
            return JsonUtility.FromJson<AllRenderers>(File.ReadAllText(filePath));
        }
    }
}
