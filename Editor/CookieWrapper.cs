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
            AssetDatabase.ImportAsset(AssetsPath);
        }

        public static bool Reading(out string context)
        {
            var cookie = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetsPath);
            if (cookie is null)
            {
                context = "";
                return false;
            }

            context = cookie.text.Trim();
            AssetDatabase.DeleteAsset(AssetsPath);
            return true;
        }
    }
}
