using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor.PrefabEditor
{
    /// <summary>
    /// 替换指定目标模型使用的共享材质。
    /// 新材质中配置的贴图会随材质一起应用到模型上。
    /// </summary>
    public sealed class HumanMaterialsEditor
    {
        private readonly Dictionary<string, Texture> _textureCache = new();
        private readonly Dictionary<string, Material> _materialCache = new();

        private static readonly int ShaderIDTextureSurface = Shader.PropertyToID("_albe");
        private static readonly int ShaderIDTextureNormal = Shader.PropertyToID("_normal");

        private static readonly Dictionary<string, string> MapPathes = new()
        {
            { "Guge", "Assets/model/NvMaps/Guge" },
            { "Jiedi", "Assets/model/NvMaps/Jiedi" },
            { "Jirou", "Assets/model/NvMaps/Jirou" },
            { "Miniao", "Assets/model/NvMaps/Miniao" }
        };

        /// <summary>
        /// 替换目标对象及其所有子节点（包括未激活节点）上的材质。
        /// 材质生成函数返回 null 时，保留对应的旧材质。
        /// </summary>
        /// <param name="target">本次需要替换材质的对象。</param>
        public void ReplaceMaterials(GameObject target)
        {
            if (target == null || !target.TryGetComponent(out Renderer render))
            {
                throw new ArgumentNullException(nameof(target));
            }

            var oldMaterials = render.sharedMaterials;
            if (oldMaterials.Length == 0)
            {
                Debug.LogWarning($"无材质球：{target.name}");
            }

            var newMaterials = new List<Material>();
            foreach (var oldMaterial in oldMaterials)
            {
                if (oldMaterial == null) continue;

                if (AssetPathFactory($"{oldMaterial.name}.mat", out var newMaterialPath))
                {
                    Material newMaterial;
                    Debug.Log($"获取到的地址：{newMaterialPath}");
                    if (!string.IsNullOrEmpty(newMaterialPath))
                    {
                        newMaterial = AssetDatabase.LoadAssetAtPath<Material>(newMaterialPath);
                        if (newMaterial == null)
                        {
                            throw new FileNotFoundException(newMaterialPath);
                        }

                        var fileName = Path.GetFileNameWithoutExtension(newMaterialPath);
                        if (!string.IsNullOrEmpty(fileName) && fileName != newMaterial.name)
                        {
                            Debug.Log($"给材质球改名：{newMaterial.name} ==> {fileName}");
                            newMaterial.name = fileName;
                            AssetDatabase.RenameAsset(newMaterialPath, fileName);
                        }
                    }
                    else
                    {
                        newMaterial = oldMaterial;
                    }

                    ReplaceTexture(oldMaterial, newMaterial, ShaderIDTextureSurface);
                    ReplaceTexture(oldMaterial, newMaterial, ShaderIDTextureNormal);

                    Debug.Log($"给 {target.name} 添加材质球 {newMaterial.name}");

                    newMaterials.Add(newMaterial);
                }
                else
                {
                    newMaterials.Add(oldMaterial);
                }
            }

            // if (newMaterials.Count > 0)
            // {
            //     render.SetSharedMaterials(newMaterials);
            // }
        }

        private void ReplaceTexture(Material oldMaterial, Material newMaterial, int shaderID)
        {
            var oldTexture = oldMaterial.GetTexture(shaderID);
            var oldTexturePath = oldTexture == null
                ? oldMaterial.name
                : Path.GetFileName(AssetDatabase.GetAssetPath(oldTexture));

            if (AssetPathFactory(oldTexturePath, out var newTexturePath))
            {
                if (!string.IsNullOrEmpty(newTexturePath))
                {
                    var newTextureName = Path.GetFileNameWithoutExtension(newTexturePath);
                    if (!_textureCache.TryGetValue(newTextureName, out var newTexture))
                    {
                        newTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(newTexturePath);
                        _textureCache.Add(newTextureName, newTexture);
                    }

                    if (newTexture != null)
                    {
                        Debug.Log($"给 {newMaterial.name} 贴上 {newTexturePath}");
                        // newMaterial.SetTexture(shaderID, newTexture);
                    }
                }
            }
        }

        /// <summary>
        /// 检查材质和贴图名称不一致的问题
        /// </summary>
        /// <param name="target">本次需要检查材质的对象。</param>
        public static void CheckingMaterials(GameObject target)
        {
            if (target == null || !target.TryGetComponent(out Renderer render))
            {
                throw new ArgumentNullException(nameof(target));
            }

            var oldMaterials = render.sharedMaterials;
            if (oldMaterials.Length == 0)
            {
                Debug.LogWarning($"无材质球：{target.name}");
            }

            foreach (var oldMaterial in oldMaterials)
            {
                if (oldMaterial.HasProperty(ShaderIDTextureSurface))
                {
                    var texSurface = oldMaterial.GetTexture(ShaderIDTextureSurface);
                    if (texSurface != null && texSurface.name != oldMaterial.name)
                    {
                        Debug.LogWarning($"{target.name} => Mat: {oldMaterial.name}; TexSurface: {texSurface.name}");
                    }
                }

                if (oldMaterial.HasProperty(ShaderIDTextureNormal))
                {
                    var texNormal = oldMaterial.GetTexture(ShaderIDTextureNormal);
                    if (texNormal != null && texNormal.name != $"{oldMaterial.name}_N")
                    {
                        Debug.LogWarning($"{target.name} => Mat: {oldMaterial.name}; TexNormal: {texNormal.name}");
                    }
                }
            }
        }

        private bool AssetPathFactory(string filePath, out string newFilePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var suffix = Path.GetExtension(filePath);
            newFilePath = "";

            if (fileName.StartsWith("Nv"))
            {
                return true;
            }

            switch (suffix.ToLower())
            {
                case ".mat":
                    newFilePath = $"Assets/model/Materials/Nv{fileName}{suffix}";
                    break;

                case ".jpg" or ".png":
                    foreach (var key in MapPathes.Keys)
                    {
                        if (fileName.Contains(key))
                        {
                            newFilePath = $"Assets/model/NvMaps/Guge/Nv{fileName}{suffix}";
                        }
                    }

                    break;

                default:
                    Debug.LogWarning($"{fileName} 缺少旧数据，进入全文件查找");
                    newFilePath = AssetDatabase
                        .FindAssets($"t:Texture2D {fileName}", new[] { "Assets/model/NvMaps" })
                        .Select(AssetDatabase.GUIDToAssetPath)
                        .FirstOrDefault(path => Path.GetFileNameWithoutExtension(path) == $"Nv{fileName}");
                    break;
            }

            return !string.IsNullOrEmpty(newFilePath);
        }

        private static string FindAllFiles(string fileName)
        {
            var allFile = AssetDatabase.FindAssets(
                $"t:Texture2D {fileName}", MapPathes.Values.ToArray()
            );

            Debug.Log($"查找遍历{fileName} 匹配到：{allFile.Length}");
            foreach (var file in allFile)
            {
                var path = AssetDatabase.GUIDToAssetPath(file);
                var name = Path.GetFileNameWithoutExtension(path);
                var result = name == $"Nv{fileName}";
                Debug.Log($"查找遍历Name：{name}, 结果：{result}");
                if (result)
                {
                    return path;
                }
            }

            return "";
        }
    }
}
