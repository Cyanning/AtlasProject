using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class AssetFileEditor
    {
        /// <summary>
        /// 将指定文件夹中名称符合正则表达式的贴图设置为可读写。
        /// </summary>
        /// <param name="folderPath">Assets 相对路径或项目 Assets 目录下的绝对路径。</param>
        /// <param name="fileNamePattern">用于匹配贴图文件名的正则表达式。</param>
        /// <returns>实际修改的贴图数量。</returns>
        public static void SetTextureReadWrite(string folderPath, string fileNamePattern)
        {
            SetImporterReadWrite<TextureImporter>(
                folderPath,
                fileNamePattern,
                "t:Texture",
                null
            );
        }

        /// <summary>
        /// 将指定文件夹中名称符合正则表达式的 FBX 设置为可读写。
        /// </summary>
        /// <param name="folderPath">Assets 相对路径或项目 Assets 目录下的绝对路径。</param>
        /// <param name="fileNamePattern">用于匹配 FBX 文件名的正则表达式。</param>
        /// <returns>实际修改的 FBX 数量。</returns>
        public static void SetFbxReadWrite(string folderPath, string fileNamePattern)
        {
            SetImporterReadWrite<ModelImporter>(
                folderPath,
                fileNamePattern,
                "t:Model",
                ".fbx"
            );
        }

        /// <summary>
        /// 在指定 Unity 资源文件夹及其子文件夹中查找符合条件的资源，
        /// 并将对应导入器的 Read/Write 设置为启用状态。
        /// </summary>
        /// <typeparam name="TImporter">
        /// 目标资源的导入器类型，目前支持 <see cref="TextureImporter"/> 和
        /// <see cref="ModelImporter"/>。
        /// </typeparam>
        /// <param name="folderPath">
        /// 待搜索的文件夹。可传入以 <c>Assets/</c> 开头的项目相对路径，
        /// 或当前项目 Assets 目录内的绝对路径。
        /// </param>
        /// <param name="fileNamePattern">
        /// 用于匹配资源文件名的正则表达式。匹配内容包含扩展名，但不包含目录路径。
        /// </param>
        /// <param name="assetFilter">
        /// 传递给 <see cref="AssetDatabase.FindAssets(string, string[])"/> 的资源过滤条件，
        /// 例如 <c>t:Texture</c> 或 <c>t:Model</c>。
        /// </param>
        /// <param name="requiredExtension">
        /// 资源必须具有的扩展名，例如 <c>.fbx</c>；传入 <see langword="null"/> 时不额外限制扩展名。
        /// </param>
        /// <exception cref="DirectoryNotFoundException">规范化后的路径不是有效的 Unity 资源文件夹。</exception>
        /// <exception cref="ArgumentException">文件夹地址或正则表达式无效。</exception>
        private static void SetImporterReadWrite<TImporter>
        (
            string folderPath,
            string fileNamePattern,
            string assetFilter,
            string requiredExtension
        ) where TImporter : AssetImporter
        {
            var assetFolder = NormalizeAssetFolder(folderPath);
            if (!AssetDatabase.IsValidFolder(assetFolder))
            {
                throw new DirectoryNotFoundException($"Unity 资源文件夹不存在：{assetFolder}");
            }

            var fileNameRegex = new Regex(fileNamePattern);
            var changedCount = 0;
            foreach (var guid in AssetDatabase.FindAssets(assetFilter, new[] { assetFolder }))
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (
                    requiredExtension != null
                    && !Path.GetExtension(assetPath).Equals(requiredExtension, StringComparison.OrdinalIgnoreCase)
                ) continue;

                if (!fileNameRegex.IsMatch(Path.GetFileName(assetPath))) continue;
                if (AssetImporter.GetAtPath(assetPath) is not TImporter importer) continue;

                switch (importer)
                {
                    case TextureImporter { isReadable: false } textureImporter:
                        textureImporter.isReadable = true;
                        break;
                    case ModelImporter { isReadable: false } modelImporter:
                        modelImporter.isReadable = true;
                        break;
                    default:
                        continue;
                }

                importer.SaveAndReimport();
                changedCount++;
            }

            Debug.Log($"{nameof(AssetFileEditor)}：已修改 {changedCount} 个资源的 Read/Write 设置。");
        }

        /// <summary>
        /// 将传入的文件夹地址转换为 AssetDatabase 使用的、以 <c>Assets</c> 开头的资源路径。
        /// </summary>
        /// <remarks>
        /// 路径分隔符会统一为正斜杠，并移除末尾斜杠。对于不以 <c>Assets</c> 开头的相对路径，
        /// 会自动补充 <c>Assets/</c> 前缀；绝对路径则必须位于当前项目的 Assets 目录内。
        /// </remarks>
        /// <param name="folderPath">Assets 相对路径，或当前项目 Assets 目录内的绝对路径。</param>
        /// <returns>规范化后的 Unity 资源文件夹路径。</returns>
        /// <exception cref="ArgumentException">路径为空，或绝对路径位于当前项目的 Assets 目录之外。</exception>
        private static string NormalizeAssetFolder(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                throw new ArgumentException("文件夹地址不能为空。", nameof(folderPath));
            }

            var normalizedPath = folderPath.Trim().Replace('\\', '/').TrimEnd('/');
            var assetsAbsolutePath = Application.dataPath.Replace('\\', '/').TrimEnd('/');

            if (Path.IsPathRooted(normalizedPath))
            {
                if (
                    !normalizedPath.Equals(assetsAbsolutePath, StringComparison.OrdinalIgnoreCase)
                    && !normalizedPath.StartsWith(assetsAbsolutePath + "/", StringComparison.OrdinalIgnoreCase)
                )
                {
                    throw new ArgumentException("文件夹必须位于当前项目的 Assets 目录中。", nameof(folderPath));
                }

                return "Assets" + normalizedPath[assetsAbsolutePath.Length..];
            }

            return normalizedPath.Equals("Assets", StringComparison.OrdinalIgnoreCase)
                   || normalizedPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase)
                ? normalizedPath
                : $"Assets/{normalizedPath.TrimStart('/')}";
        }
    }
}
