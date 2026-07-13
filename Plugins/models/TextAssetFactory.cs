using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Plugins.models
{
    public abstract class TextAssetFactory<T> where T : class
    {
        protected virtual string DatabaseFolder => "Atlas_database";
        protected virtual string FilePrefix => "";
        protected virtual string FileExtension => ".json";
        protected virtual Encoding FileEncoding => Encoding.UTF8;

        protected virtual string GetDefaultAssetName(T item)
        {
            // 当没有指定传文件路径（可以不包含前缀后缀）时，该方法决定如何取得文件名
            return nameof(T);
        }

        protected virtual void ApplyUniformName(string assetName, T item)
        {
            // 该方法用来处理文件名和对象的关系，默认什么都不做
        }

        private string CurrentAssetPath(string assetName, bool autoCreate = false)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                throw new ArgumentException("Asset name is empty.", nameof(assetName));
            }

            var assetsFolder = Path.Combine("Assets", DatabaseFolder, Path.GetDirectoryName(assetName) ?? "");

            if (!Directory.Exists(assetsFolder) && autoCreate)
            {
                Directory.CreateDirectory(assetsFolder);
            }

            var fileName = Path.GetFileNameWithoutExtension(assetName);

            if (!(string.IsNullOrEmpty(FilePrefix) || fileName.StartsWith(FilePrefix)))
            {
                fileName = FilePrefix + fileName;
            }

            return Path.Combine(assetsFolder, $"{fileName}{FileExtension}");
        }

        protected bool LoadAsset(string assetName, out T item)
        {
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(CurrentAssetPath(assetName));
            if (asset is null)
            {
                item = null;
                return false;
            }

            item = JsonUtility.FromJson<T>(asset.text);
            return true;
        }

        protected void SaveAsset(T item, string assetName = null)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            assetName ??= GetDefaultAssetName(item);
            var assetPath = CurrentAssetPath(assetName, true);

            ApplyUniformName(Path.GetFileNameWithoutExtension(assetPath), item);

            File.WriteAllText(Path.GetFullPath(assetPath), JsonUtility.ToJson(item), FileEncoding);
            AssetDatabase.ImportAsset(assetPath);
        }
    }
}
