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
    /// 根据 Renderer 记录文件，为指定模型替换对应的 Nv 材质。
    /// </summary>
    public sealed class HumanMaterialsEditor
    {
        private const string NewMaterialPrefix = "Nv";
        private const string RendererRecordsDirectory = @"D:\AnatomyLibrary\unity_editor";
        private const string MaterialAssetsDirectory = "Assets/model/Materials";

        private static readonly Dictionary<string, string> TextureAssetsDirectory = new()
        {
            { "Guge", "Assets/model/NvMaps/Guge" },
            { "Jiedi", "Assets/model/NvMaps/Jiedi" },
            { "Jirou", "Assets/model/NvMaps/Jirou" },
            { "Miniao", "Assets/model/NvMaps/Miniao" }
        };

        private static readonly int ShaderIDTextureSurface = Shader.PropertyToID("_albe");
        private static readonly int ShaderIDTextureNormal = Shader.PropertyToID("_normal");

        private RendererWrapper[] _currentSystemRecords;
        private readonly Dictionary<string, Material> _materialCache = new(StringComparer.Ordinal);

        /// <summary>
        /// 遍历目标对象下的所有叶子节点，并替换各叶子节点的材质。
        /// </summary>
        /// <returns>成功替换材质的叶子节点数量。</returns>
        public int ReplaceMaterials(GameObject target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (!BodyStruct.GetFromPrefab(target.name, out var currentRootStruct))
            {
                Debug.LogError($"根模型不符合标准 {target.name}");
                return 0;
            }

            if (!TryLoadSystemRecords(currentRootStruct))
            {
                Debug.LogError($"找不到记录的材质数据 {target.name}");
                return 0;
            }

            var replacedCount = 0;
            foreach (var leaf in PrefabCollection.ForEachChildren(target.transform, leafOnly: true))
            {
                if (ReplaceLeafMaterials(leaf.gameObject))
                {
                    replacedCount++;
                }
            }

            return replacedCount;
        }

        /// <summary>
        /// 查找单个叶子节点的原材质记录，并应用对应的 Nv 材质。
        /// </summary>
        private bool ReplaceLeafMaterials(GameObject target)
        {
            if (!BodyStruct.GetFromPrefab(target.name, out var body))
            {
                Debug.LogWarning($"无法从模型名称解析 value：{target.name}");
                return false;
            }

            if (!target.TryGetComponent(out Renderer renderer))
            {
                Debug.LogWarning($"模型未挂载 Renderer：{target.name}");
                return false;
            }

            var rendererRecord = GetRendererRecord(body.value);
            if (rendererRecord == null)
            {
                Debug.LogWarning($"没有找到模型的材质记录：{target.name}，value：{body.value}");
                return false;
            }

            // 添加根据记录找材质球，数量对不上不替换
            var newMaterials = GetMaterials(rendererRecord);
            if (newMaterials.Length != renderer.sharedMaterials.Length)
            {
                return false;
            }

            renderer.sharedMaterials = newMaterials;
            return true;
        }

        /// <summary>
        /// 读取材质记录文档
        /// </summary>
        private bool TryLoadSystemRecords(BodyStruct rootStruct)
        {
            if (!Directory.Exists(RendererRecordsDirectory))
            {
                throw new DirectoryNotFoundException(RendererRecordsDirectory);
            }

            var recordFilePath = Directory.EnumerateFiles(
                RendererRecordsDirectory,
                $"*~{rootStruct.value}.json",
                SearchOption.TopDirectoryOnly).FirstOrDefault();

            if (string.IsNullOrEmpty(recordFilePath))
            {
                return false;
            }

            var recordsWrapper = HumanRendererRecorder.LoadJson(recordFilePath);
            if (recordsWrapper?.renderers == null || recordsWrapper.renderers.Count == 0)
            {
                throw new InvalidDataException($"Renderer 记录文件格式错误：{recordFilePath}");
            }

            _currentSystemRecords = recordsWrapper.renderers.ToArray();
            return true;
        }

        /// <summary>
        /// 从缓存加载材质
        /// </summary>
        private RendererWrapper GetRendererRecord(int bodyValue)
        {
            var result =
                from record in _currentSystemRecords
                where record != null && record.value == bodyValue
                select record;

            return result.FirstOrDefault();
        }

        /// <summary>
        /// 获取新材质，优先从材质球缓存加载
        /// <param name="rendererRecord">材质球数据，依据传入的数据转换为新数据</param>
        /// </summary>
        private Material[] GetMaterials(RendererWrapper rendererRecord)
        {
            var materials = new Material[rendererRecord.materials.Count];
            for (var i = 0; i < rendererRecord.materials.Count; i++)
            {
                var materialRecord = rendererRecord.materials[i];
                if (materialRecord == null || string.IsNullOrWhiteSpace(materialRecord.name))
                {
                    Debug.LogWarning($"value 为 {rendererRecord.value} 的第 {i} 个材质记录无效。");
                    continue;
                }

                var newMaterialRecord = MarkNewMaterialWrapper(materialRecord);

                if (_materialCache.TryGetValue(newMaterialRecord.name, out materials[i])) continue;

                if (MarkMaterial(newMaterialRecord, out materials[i])) continue;

                materials[i] = null;
            }

            return materials;
        }

        /// <summary>
        /// 把老名字换成新名字
        /// </summary>
        private static MaterialWrapper MarkNewMaterialWrapper(MaterialWrapper oldMaterial)
        {
            return new MaterialWrapper
            {
                name = GetNewName(oldMaterial.name),
                albe = GetNewName(oldMaterial.albe),
                normal = GetNewName(oldMaterial.normal)
            };
        }

        private static string GetNewName(string oldName)
        {
            if (string.IsNullOrWhiteSpace(oldName))
            {
                return "";
            }

            return oldName.StartsWith(NewMaterialPrefix, StringComparison.Ordinal)
                ? oldName
                : $"{NewMaterialPrefix}{oldName}";
        }

        /// <summary>
        /// 制作材质球
        /// </summary>
        private bool MarkMaterial(MaterialWrapper materialWrapper, out Material newMaterial)
        {
            var newMaterialPath = Path.Combine(MaterialAssetsDirectory, $"{materialWrapper.name}.mat");
            newMaterial = AssetDatabase.LoadAssetAtPath<Material>(newMaterialPath);

            if (newMaterial == null)
            {
                return false;
            }

            if (materialWrapper.name != newMaterial.name)
            {
                Debug.Log($"给材质球改名：{newMaterial.name} ==> {materialWrapper.name}");
                newMaterial.name = materialWrapper.name;
                AssetDatabase.RenameAsset(newMaterialPath, materialWrapper.name);
            }

            SetTexture(newMaterial, materialWrapper.albe, ShaderIDTextureSurface);
            SetTexture(newMaterial, materialWrapper.normal, ShaderIDTextureNormal);

            _materialCache[newMaterial.name] = newMaterial;

            return true;
        }

        /// <summary>
        /// 给材质球贴图
        /// </summary>
        private static void SetTexture(Material material, string imgName, int shaderID)
        {
            if (!material.HasProperty(shaderID) || string.IsNullOrWhiteSpace(imgName))
            {
                return;
            }

            foreach (var textureAsset in TextureAssetsDirectory)
            {
                if (imgName.Contains(textureAsset.Key))
                {
                    var newTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(
                        Path.Combine(textureAsset.Value, imgName)
                    );
                    if (newTexture == null) continue;

                    material.SetTexture(shaderID, newTexture);
                    return;
                }
            }
        }
    }
}
