using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Editor.models
{
    public static class CookieWrapper
    {
        private const string CookiePath = "Editor/cookie.txt";
        private static readonly string CookieInAsset = Path.Combine("Assets", CookiePath);

        public static void Create(string context)
        {
            // 创建缓存文件
            File.WriteAllText(Path.Combine(Application.dataPath, CookiePath), context, Encoding.UTF8);
            AssetDatabase.ImportAsset(CookieInAsset);
        }

        public static string Reading()
        {
            // 读取缓存文件
            return AssetDatabase.LoadAssetAtPath<TextAsset>(CookieInAsset)?.text.Trim() ?? "";
        }
    }
}
