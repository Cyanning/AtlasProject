using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Plugins.models;
using UnityEditor;
using UnityEngine;

namespace Editor.PrefabEditor
{
    /// <summary>
    /// 指定需要修改 Renderer 的哪些内容。
    /// </summary>
    [Flags]
    public enum RendererChange
    {
        None = 0,
        Materials = 1,
        Textures = 2,
        All = Materials | Textures
    }

    /// <summary>
    /// 根据 Renderer 记录修改 GameObject 的共享材质和贴图。
    /// </summary>
    public sealed class HumanMaterialsEditor
    {
        private const string RendererRecordsDirectory = @"D:\AnatomyLibrary\unity_editor";
        private const string MaterialAssetsDirectory = "Assets/model/Materials";

        private readonly Dictionary<string, string> _textureAssetsDirectory = new()
        {
            { "Guge", "Assets/model/NvMaps/Guge" },
            { "Jiedi", "Assets/model/NvMaps/Jiedi" },
            { "Jirou", "Assets/model/NvMaps/Jirou" },
            { "Miniao", "Assets/model/NvMaps/Miniao" }
        };

        private readonly int _shaderIdTextureAlbe = Shader.PropertyToID("_albe");
        private readonly int _shaderIdTextureNormal = Shader.PropertyToID("_normal");
        private readonly Dictionary<string, Material> _materialCache = new(StringComparer.Ordinal);
        private readonly NameConverter _nameConverter;

        private BodyRenderersWrapper _currentRecorder;
        private int _currentSystemValue;


        public HumanMaterialsEditor(NameConverter nameConverter)
        {
            _nameConverter = nameConverter ?? throw new ArgumentNullException(nameof(nameConverter));
        }

        /// <summary>
        /// 加载一个系统的全部 Renderer 记录。再次加载其他系统时会替换当前记录，
        /// 材质缓存会保留，以便不同模型继续共用已经加载的材质资源。
        /// </summary>
        public bool LoadSystemRecords(GameObject systemRoot, BodyStruct systemBody = null)
        {
            if (systemRoot == null)
            {
                throw new ArgumentNullException(nameof(systemRoot));
            }

            systemBody ??= new BodyStruct(systemRoot.name);
            if (_currentRecorder != null && _currentSystemValue == systemBody.value)
            {
                return true;
            }

            var recordFilePath = Directory.EnumerateFiles(
                RendererRecordsDirectory,
                $"*~{systemBody.value}.json",
                SearchOption.TopDirectoryOnly).FirstOrDefault();

            _currentRecorder = null;
            _currentSystemValue = 0;

            if (string.IsNullOrEmpty(recordFilePath) ||
                !BodyRenderersWrapper.LoadJson(recordFilePath, out _currentRecorder))
            {
                return false;
            }

            _currentSystemValue = systemBody.value;
            return true;
        }

        /// <summary>
        /// 批量处理根对象下的所有叶子 GameObject。
        /// </summary>
        public int ProcessChildren(GameObject root, RendererChange changes = RendererChange.All, BodyStruct rootBody = null)
        {
            if (!LoadSystemRecords(root, rootBody))
            {
                Debug.LogError($"找不到记录的材质数据：{root.name}");
                return 0;
            }

            var changedCount = 0;
            foreach (var leaf in PrefabCollection.ForEachChildren(root.transform, leafOnly: true))
            {
                if (ProcessGameObject(leaf.gameObject, changes))
                {
                    changedCount++;
                }
            }

            return changedCount;
        }

        /// <summary>
        /// 单独处理一个 GameObject。调用前需要先通过 LoadSystemRecords 加载其所属系统记录。
        /// </summary>
        public bool ProcessGameObject(GameObject target, RendererChange changes = RendererChange.All)
        {
            if (changes == RendererChange.None ||
                !TryGetRendererRecord(target, out var renderer, out var rendererRecord))
            {
                return false;
            }

            if ((changes & RendererChange.Materials) != 0)
            {
                ReplaceMaterialBindings(renderer, rendererRecord);
            }

            if ((changes & RendererChange.Textures) != 0)
            {
                ReplaceRendererTextures(renderer, rendererRecord);
            }

            return true;
        }

        /// <summary>
        /// 只替换一个 GameObject 的共享材质绑定，不修改贴图。
        /// </summary>
        public bool ReplaceGameObjectMaterials(GameObject target)
        {
            return ProcessGameObject(target, RendererChange.Materials);
        }

        /// <summary>
        /// 只替换一个 GameObject 当前共享材质上的贴图，不修改材质绑定。
        /// </summary>
        public bool ReplaceGameObjectTextures(GameObject target)
        {
            return ProcessGameObject(target, RendererChange.Textures);
        }

        /// <summary>
        /// 保留原有调用入口：批量替换子对象的材质和贴图。
        /// </summary>
        public int ReplaceMaterials(GameObject target, BodyStruct targetBodyStruct = null)
        {
            return ProcessChildren(target, RendererChange.All, targetBodyStruct);
        }

        private bool TryGetRendererRecord(GameObject target, out Renderer renderer, out RendererWrapper rendererRecord)
        {
            renderer = null;
            rendererRecord = null;

            if (_currentRecorder == null)
            {
                Debug.LogWarning("请先加载模型所属系统的 Renderer 记录");
                return false;
            }

            if (!target.TryGetComponent(out renderer))
            {
                Debug.LogWarning($"模型未挂载 Renderer：{target.name}");
                return false;
            }

            var body = new BodyStruct(target.name);
            var oldRendererRecord = _currentRecorder.GetRendererRecord(body.value);
            if (oldRendererRecord == null)
            {
                Debug.LogWarning($"没有找到模型的材质记录：{body.name}，value：{body.value}");
                return false;
            }

            rendererRecord = _nameConverter.ConvertRenderer(oldRendererRecord);
            return true;
        }

        private void ReplaceMaterialBindings(Renderer renderer, RendererWrapper rendererRecord)
        {
            var currentMaterials = renderer.sharedMaterials;
            var newMaterials = new Material[rendererRecord.materials.Count];

            for (var i = 0; i < newMaterials.Length; i++)
            {
                var materialName = rendererRecord.materials[i].name;

                if (i < currentMaterials.Length &&
                    currentMaterials[i] != null &&
                    currentMaterials[i].name == materialName)
                {
                    newMaterials[i] = currentMaterials[i];
                    continue;
                }

                newMaterials[i] = LoadMaterial(materialName);
            }

            renderer.sharedMaterials = newMaterials;
        }

        private Material LoadMaterial(string materialName)
        {
            if (_materialCache.TryGetValue(materialName, out var material))
            {
                return material;
            }

            var materialPath = $"{MaterialAssetsDirectory}/{materialName}.mat";
            material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material != null)
            {
                _materialCache[materialName] = material;
            }

            return material;
        }

        private void ReplaceRendererTextures(Renderer renderer, RendererWrapper rendererRecord)
        {
            var materials = renderer.sharedMaterials;
            var count = Math.Min(materials.Length, rendererRecord.materials.Count);

            for (var i = 0; i < count; i++)
            {
                var materialRecord = rendererRecord.materials[i];
                SetTexture(materials[i], materialRecord.albe, _shaderIdTextureAlbe);
                SetTexture(materials[i], materialRecord.normal, _shaderIdTextureNormal);
            }
        }

        private void SetTexture(Material material, string textureName, int shaderId)
        {
            if (material == null ||
                string.IsNullOrWhiteSpace(textureName) ||
                !material.HasProperty(shaderId))
            {
                return;
            }

            foreach (var textureDirectory in _textureAssetsDirectory)
            {
                if (textureName.IndexOf(textureDirectory.Key, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                var texturePath = $"{textureDirectory.Value}/{textureName}";
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                if (texture != null)
                {
                    material.SetTexture(shaderId, texture);
                }

                return;
            }
        }
    }
}
