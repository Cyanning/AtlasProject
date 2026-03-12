using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class CookieWrapper
    {
        private const string AssetsPath = "Assets/Editor/cookie.txt";
        private static readonly string SystemPath = Path.Combine(Application.dataPath, "Editor", "cookie.txt");

        public static void Create(string context)
        {
            File.WriteAllText(SystemPath, context, Encoding.UTF8);
        }

        public static string Reading()
        {
            return AssetDatabase.LoadAssetAtPath<TextAsset>(AssetsPath)?.text.Trim() ?? "";
        }
    }
}
