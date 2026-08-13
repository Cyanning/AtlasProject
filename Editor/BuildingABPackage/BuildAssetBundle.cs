using UnityEngine;
using UnityEditor;
using System.IO;
namespace Editor.BuildingABPackage
{
    public class BuildAssetBundle : MonoBehaviour
    {
        //StreamingAssets与Resources的区别在于，StreamingAssets不会被压缩打进包体，而Resources会被压缩

        private const string ResOutputPathAnd = "Assets/StreamingAssets/Android";
        private const string ResOutputPathIos = "Assets/StreamingAssets/IOS";
        private const string ResOutputPathWeb = "Assets/StreamingAssets/Web";
        private const string ResOuputPathWindow = "Assets/StreamingAssets/Windows";

        [MenuItem("ABPackager/BuildEnAndAB")]
        private static void BuildAndAb()
        {
            var manifest = BuildPipeline.BuildAssetBundles(
                ResOutputPathAnd, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.Android
            );
            foreach (var name in manifest.GetAllAssetBundles())
            {
                Debug.Log("打包 " + name);
                var data = File.ReadAllBytes(Path.Combine(ResOutputPathAnd, name));

                using var myStream = new MyStream(
                    Path.Combine(ResOutputPathAnd, "encypt_" + name), FileMode.Create
                );
                myStream.Write(data, 0, data.Length);

            }

            AssetDatabase.Refresh();
        }
        [MenuItem("ABPackager/BuildEnIosAB")]
        private static void BuildIosAb()
        {
            var manifest = BuildPipeline.BuildAssetBundles(
                ResOutputPathIos, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.iOS
            );
            foreach (var name in manifest.GetAllAssetBundles())
            {
                Debug.Log("打包 " + name);
                var data = File.ReadAllBytes(Path.Combine(ResOutputPathIos, name));

                using var myStream = new MyStream(
                    Path.Combine(ResOutputPathIos, "encypt_" + name), FileMode.Create
                );
                myStream.Write(data, 0, data.Length);
            }
            AssetDatabase.Refresh();
        }

        [MenuItem("ABPackager/BuildEnWebAB")]
        private static void BuildWebAb()
        {
            var manifest = BuildPipeline.BuildAssetBundles(
                ResOutputPathWeb, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.WebGL
            );

            foreach (var name in manifest.GetAllAssetBundles())
            {
                Debug.Log("打包 " + name);
                var data = File.ReadAllBytes(Path.Combine(ResOutputPathWeb, name));

                using var myStream = new MyStream(
                    Path.Combine(ResOutputPathWeb, "encypt_" + name), FileMode.Create
                );
                myStream.Write(data, 0, data.Length);

            }

            AssetDatabase.Refresh();
        }


        [MenuItem("ABPackager/BuildEnWindowAB")]
        private static void BuildWindowAb()
        {
            var manifest = BuildPipeline.BuildAssetBundles(
                ResOuputPathWindow, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows64
            );

            foreach (var name in manifest.GetAllAssetBundles())
            {
                Debug.Log("打包 " + name);
                var data = File.ReadAllBytes(Path.Combine(ResOuputPathWindow, name));

                using var myStream = new MyStream(
                    Path.Combine(ResOuputPathWindow, "encypt_" + name), FileMode.Create
                );
                myStream.Write(data, 0, data.Length);
            }
            AssetDatabase.Refresh();
        }
    }
}
