using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Plugins.C_.models
{
    public static class AtlasFactory
    {
        // folderName: folders/.../name
        private static (string assets, string system) AtlasPath(string folderName, bool isSaving = false)
        {
            var directory = Path.Combine("Atlas_database", Path.GetDirectoryName(folderName) ?? "");

            if (!Directory.Exists(directory) && isSaving)
            {
                Directory.CreateDirectory(directory);
            }

            var relativePath = Path.Combine(directory, $"Atlas_{Path.GetFileName(folderName)}.json");
            return (
                Path.Combine("Assets", relativePath),
                Path.Combine(Application.dataPath, relativePath)
            );
        }

        public static bool Load(string folderName, out AtlasItem atlas)
        {
            var atlasAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(AtlasPath(folderName).assets);
            if (atlasAsset is null)
            {
                atlas = null;
                return false;
            }

            atlas = JsonUtility.FromJson<AtlasItem>(atlasAsset.text);
            return true;
        }

        // 若 uniformName 为 true，则将 folderName 中的名字赋值给 atlas的 name属性
        // folderName为空时，uniformName 设置无效
        public static void Save(AtlasItem atlas, string folderName = null, bool uniformName = false)
        {
            folderName ??= atlas.name;
            if (uniformName)
            {
                atlas.name = Path.GetFileName(folderName);
            }

            var path = AtlasPath(folderName, true);
            File.WriteAllText(path.system, JsonUtility.ToJson(atlas), Encoding.UTF8);
            AssetDatabase.ImportAsset(path.assets);
        }
    }
}
