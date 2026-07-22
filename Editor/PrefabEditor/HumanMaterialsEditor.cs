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

        private static readonly int ShaderIDTextureAlbe = Shader.PropertyToID("_albe");
        private static readonly int ShaderIDTextureNormal = Shader.PropertyToID("_normal");

        private readonly HumanRendererRecorder _currentRecorder = new();
        private readonly Dictionary<string, Material> _materialCache = new(StringComparer.Ordinal);

        /// <summary>
        /// 遍历目标对象下的所有叶子节点，并替换各叶子节点的材质。
        /// </summary>
        /// <param name="target">目标预制体群根对象</param>
        /// <param name="targetBodyStruct">自定义根对象的模型数据</param>
        /// <returns>成功替换材质的叶子节点数量。</returns>
        public int ReplaceMaterials(GameObject target, BodyStruct targetBodyStruct = null)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            targetBodyStruct ??= new BodyStruct(target.name);
            if (!TryLoadSystemRecords(targetBodyStruct))
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
        /// 读取材质记录文档
        /// <param name="rootStruct">缓存了材质数据的对象的根节点</param>
        /// <returns>是否成功获取</returns>
        /// </summary>
        private bool TryLoadSystemRecords(BodyStruct rootStruct)
        {
            if (!Directory.Exists(RendererRecordsDirectory))
            {
                throw new DirectoryNotFoundException(RendererRecordsDirectory);
            }

            // 遍历文件夹中的材质数据文件，找到符合条件的文件绝对地址
            var recordFilePath = Directory.EnumerateFiles(
                RendererRecordsDirectory,
                $"*~{rootStruct.value}.json",
                SearchOption.TopDirectoryOnly).FirstOrDefault();

            // 极简语句判断是否加载成功，HumanRendererRecorder自动加载文件
            return !string.IsNullOrEmpty(recordFilePath) && _currentRecorder.LoadJson(recordFilePath);
        }

        /// <summary>
        /// 查找单个叶子节点的原材质记录，并应用对应的 Nv 材质。
        /// <param name="target">被替换的 Game 对象</param>
        /// <returns>是否成功替换材质球</returns>
        /// </summary>
        private bool ReplaceLeafMaterials(GameObject target)
        {
            var body = new BodyStruct(target.name);
            if (!target.TryGetComponent(out Renderer renderer))
            {
                Debug.LogWarning($"模型未挂载 Renderer：{target.name}");
                return false;
            }

            var rendererRecord = _currentRecorder.GetRendererRecord(body.value);
            if (rendererRecord == null)
            {
                Debug.LogWarning($"没有找到模型的材质记录：{body.name}，value：{body.value}");
                return false;
            }

            // 根据材质记录 转换为新材质数据，并找到材质球，再修改贴图
            var newRendererRecord = WrapperConverter(rendererRecord);
            var newMaterials = GetMaterials(renderer.sharedMaterials, newRendererRecord);
            for (var i = 0; i < newMaterials.Length; i++)
            {
                SetTexture(newMaterials[i], newRendererRecord.materials[i].albe, ShaderIDTextureAlbe);
                SetTexture(newMaterials[i], newRendererRecord.materials[i].normal, ShaderIDTextureNormal);
            }

            renderer.sharedMaterials = newMaterials;
            return true;
        }

        /// <summary>
        /// 根据材质记录，获取新材质（优先从材质球缓存加载）
        /// <param name="rendererRecord">材质球数据，依据传入的数据转换为新数据</param>
        /// </summary>
        private Material[] GetMaterials(Material[] currentMaterials, RendererWrapper rendererRecord)
        {
            var materials = new Material[rendererRecord.materials.Count];
            for (var i = 0; i < materials.Length; i++)
            {
                var materialRecord = rendererRecord.materials[i];
                materials[i] = null;

                // 材质球新旧对比, 一致则直接从Renderer组件中拿
                if (i < currentMaterials.Length && currentMaterials[i].name == materialRecord.name)
                {
                    materials[i] = currentMaterials[i];
                    continue;
                }

                // 优先拿取缓存的材质球对象
                if (_materialCache.TryGetValue(materialRecord.name, out var newMaterial))
                {
                    continue;
                }

                // 从项目资源拿取材质球
                var newMaterialPath = Path.Combine(MaterialAssetsDirectory, $"{materialRecord.name}.mat");
                newMaterial = AssetDatabase.LoadAssetAtPath<Material>(newMaterialPath);
                if (newMaterial == null)
                {
                    continue;
                }

                // 修正材质球名称
                if (materialRecord.name != newMaterial.name)
                {
                    newMaterial.name = materialRecord.name;
                    AssetDatabase.RenameAsset(newMaterialPath, materialRecord.name);
                }

                // 缓存材质球
                _materialCache[newMaterial.name] = newMaterial;
                materials[i] = newMaterial;
            }

            return materials;
        }

        /// <summary>
        /// 给材质球贴图
        /// </summary>
        private static void SetTexture(Material material, string imgName, int shaderID)
        {
            // 判断传入的参数是否有效
            if (!material.HasProperty(shaderID) || string.IsNullOrWhiteSpace(imgName))
            {
                return;
            }

            // 判断是否会重复设置
            if (material.GetTexture(shaderID)?.name == Path.GetFileNameWithoutExtension(imgName))
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

        /// <summary>
        /// 把老名字换成新名字
        /// </summary>
        private static RendererWrapper WrapperConverter(RendererWrapper oldRenderer)
        {
            var newRenderer = new RendererWrapper
            {
                value = oldRenderer.value,
                materials = new List<MaterialWrapper>()
            };
            foreach (var oldMaterial in oldRenderer.materials)
            {
                if (oldMaterial == null || string.IsNullOrWhiteSpace(oldMaterial.name))
                {
                    Debug.LogWarning($"value 为 {oldRenderer.value} 有无效材质记录");
                    continue;
                }

                newRenderer.materials.Add(
                    new MaterialWrapper
                    {
                        name = GetNewName(oldMaterial.name),
                        albe = GetNewName(oldMaterial.albe),
                        normal = GetNewName(oldMaterial.normal)
                    }
                );
            }

            return newRenderer;
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
    }
}
