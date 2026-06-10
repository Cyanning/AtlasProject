using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Plugins.C_.models
{
    public static class AtlasFactory
    {
        // folderName: folders/.../name
        private static string CurrentAssetPath(
            string folderName,
            string databaseFolder = "Atlas_database",
            string filePrefix = "Atlas_",
            bool isSaving = false)
        {
            if (string.IsNullOrEmpty(folderName))
            {
                throw new ArgumentException("Folder name is empty.", nameof(folderName));
            }

            var assetsFolder = Path.Combine("Assets", databaseFolder, Path.GetDirectoryName(folderName) ?? "");

            if (!Directory.Exists(assetsFolder) && isSaving)
            {
                Directory.CreateDirectory(assetsFolder);
            }

            var fileName = Path.GetFileNameWithoutExtension(folderName);

            if (!string.IsNullOrEmpty(filePrefix) && fileName.StartsWith(filePrefix))
            {
                fileName = fileName[filePrefix.Length..];
            }

            return Path.Combine(assetsFolder, $"{filePrefix}{fileName}.json").Replace('\\', '/');
        }

        public static bool Load<T>(
            string folderName,
            out T item,
            string databaseFolder = "Atlas_database",
            string filePrefix = "Atlas_") where T : class
        {
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(
                CurrentAssetPath(folderName, databaseFolder, filePrefix)
            );

            if (asset is null)
            {
                item = null;
                return false;
            }

            item = JsonUtility.FromJson<T>(asset.text);
            return true;
        }

        public static bool Load(string folderName, out AtlasItem atlas)
        {
            return Load<AtlasItem>(folderName, out atlas);
        }

        public static void Save<T>(
            T item,
            string folderName,
            bool uniformName = false,
            Action<T, string> uniformNameSetter = null,
            string databaseFolder = "Atlas_database",
            string filePrefix = "Atlas_") where T : class
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (uniformName)
            {
                uniformNameSetter?.Invoke(item, Path.GetFileName(folderName));
            }

            var path = CurrentAssetPath(folderName, databaseFolder, filePrefix, true);
            File.WriteAllText(Path.GetFullPath(path), JsonUtility.ToJson(item), Encoding.UTF8);
            AssetDatabase.ImportAsset(path);
        }

        // 若 uniformName 为 true，则将 folderName 中的名字赋值给 atlas的 name属性
        // folderName为空时，uniformName 设置无效
        public static void Save(AtlasItem atlas, string folderName = null, bool uniformName = false)
        {
            folderName ??= atlas.name;
            Save(atlas, folderName, uniformName, (item, name) => item.name = name);
        }
    }
}
