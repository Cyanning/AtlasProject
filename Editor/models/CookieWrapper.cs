using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Editor.models
{
    public static class CookieWrapper
    {
        private const string AssetsPath = "Assets/Editor/models/cookie.txt";
        private static readonly string SystemPath = Path.Combine(Application.dataPath, "Editor", "cookie.txt");

        public static void Create(string context)
        {
            // 创建缓存文件
            File.WriteAllText(SystemPath, context, Encoding.UTF8);
            AssetDatabase.ImportAsset(AssetsPath);
        }

        public static string Reading()
        {
            // 读取缓存文件
            return AssetDatabase.LoadAssetAtPath<TextAsset>(AssetsPath)?.text.Trim() ?? "";
        }
    }
}
