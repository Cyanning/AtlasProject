using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
namespace Editor.BuildingABPackage
{
    public class BuildAssetBundle : MonoBehaviour
    {

        //StreamingAssets与Resources的区别在于，StreamingAssets不会被压缩打进包体，而Resources会被压缩
        public static readonly string RES_OUTPUT_PATH_WINDOW = "Assets/StreamingAssets/Windows";
        public static readonly string RES_OUTPUT_PATH_AND = "Assets/StreamingAssets/Android";
        public static readonly string RES_OUTPUT_PATH_IOS = "Assets/StreamingAssets/IOS";
        public static readonly string RES_OUTPUT_PATH_WEB = "Assets/StreamingAssets/Web";

        public static readonly bool MODEL_TEST = false;
        private static readonly string TestModelName = "body_male";

        //MenuItem会在unity菜单栏添加自定义新项
        [MenuItem("ABPackager/Others/Build AssetBundleWeb")]
        private static void BuildWeb()
        {
            BuildPipeline.BuildAssetBundles(RES_OUTPUT_PATH_WEB, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.WebGL);
        }
        [MenuItem("ABPackager/Others/Build AssetBundleWindow")]
        private static void BuildWindow()
        {
            BuildPipeline.BuildAssetBundles(RES_OUTPUT_PATH_WINDOW, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows);
        }

        [MenuItem("ABPackager/Others/Build AssetBundleIOS")]
        private static void BuildIos()
        {
            BuildPipeline.BuildAssetBundles(RES_OUTPUT_PATH_IOS, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.iOS);
        }

        [MenuItem("ABPackager/Others/Build AssetBundleAndroid")]
        private static void BuildAndroid()
        {
            BuildPipeline.BuildAssetBundles(RES_OUTPUT_PATH_AND, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.Android);
        }

        [MenuItem("ABPackager/BuildEnWebAB")]
        static void BuildWebAB()
        {

            var manifest = BuildPipeline.BuildAssetBundles(RES_OUTPUT_PATH_WEB, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.WebGL);
            string[] assetBundles = manifest.GetAllAssetBundles();
            for (int i = 0; i < assetBundles.Length; i++)
            {
                string name = assetBundles[i];
                Debug.Log("打包 " + name);

                if (MODEL_TEST)
                {
                    if (!name.Equals(TestModelName))
                    {
                        continue;
                    }
                }

                var uniqueSalt = Encoding.UTF8.GetBytes(name);
                var data = File.ReadAllBytes(Path.Combine(RES_OUTPUT_PATH_WEB, name));

                /*      byte[] newData = new byte[data.Length+99];
                      data.CopyTo(newData,99);*/
                using (var myStream = new MyStream(Path.Combine(RES_OUTPUT_PATH_WEB, "encypt_" + name), FileMode.Create))
                {
                    myStream.Write(data, 0, data.Length);
                }
            }
           
            AssetDatabase.Refresh();
        }
        [MenuItem("ABPackager/BuildEnAndAB")]
        static void BuildAndAB()
        {

            var manifest = BuildPipeline.BuildAssetBundles(RES_OUTPUT_PATH_AND, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.Android);
            string[] assetBundles = manifest.GetAllAssetBundles();
            for (int i = 0; i < assetBundles.Length;i++) {
                string name = assetBundles[i];
                Debug.Log("打包 " + name);

                if (MODEL_TEST)
                {
                    if (!name.Equals(TestModelName))
                    {
                        continue;
                    }
                }

                var uniqueSalt = Encoding.UTF8.GetBytes(name);
                var data = File.ReadAllBytes(Path.Combine(RES_OUTPUT_PATH_AND, name));

                using (var myStream = new MyStream(Path.Combine(RES_OUTPUT_PATH_AND, "encypt_" + name), FileMode.Create))
                {
                    myStream.Write(data, 0, data.Length);
                }
            }
            
            AssetDatabase.Refresh();
        }
        [MenuItem("ABPackager/BuildEnIosAB")]
        static void BuildIosAB()
        {

            var manifest = BuildPipeline.BuildAssetBundles(RES_OUTPUT_PATH_IOS, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.iOS);
            string[] assetBundles = manifest.GetAllAssetBundles();
            for (int i = 0; i < assetBundles.Length; i++)
            {
                string name = assetBundles[i];
                Debug.Log("打包 " + name);

                if (MODEL_TEST)
                {
                    if (!name.Equals(TestModelName))
                    {
                        continue;
                    }
                }

                var uniqueSalt = Encoding.UTF8.GetBytes(name);
                var data = File.ReadAllBytes(Path.Combine(RES_OUTPUT_PATH_IOS, name));

                /*      byte[] newData = new byte[data.Length+99];
                      data.CopyTo(newData,99);*/
                using (var myStream = new MyStream(Path.Combine(RES_OUTPUT_PATH_IOS, "encypt_" + name), FileMode.Create))
                {
                    myStream.Write(data, 0, data.Length);
                }
            }

            AssetDatabase.Refresh();
        }


        [MenuItem("ABPackager/BuildEnWindowAB")]
        static void BuildWindowAB()
        {

            var manifest = BuildPipeline.BuildAssetBundles(RES_OUTPUT_PATH_WINDOW, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows);
  
            string[] assetBundles = manifest.GetAllAssetBundles();
            for (int i = 0; i < assetBundles.Length; i++)
            {
                string name = assetBundles[i];
                Debug.Log("打包 " + name);

                if (MODEL_TEST)
                {
                    if (!name.Equals(TestModelName))
                    {
                        continue;
                    }
                }

                var uniqueSalt = Encoding.UTF8.GetBytes(name);
                var data = File.ReadAllBytes(Path.Combine(RES_OUTPUT_PATH_WINDOW, name));

                /*      byte[] newData = new byte[data.Length+99];
                      data.CopyTo(newData,99);*/
                using (var myStream = new MyStream(Path.Combine(RES_OUTPUT_PATH_WINDOW, "encypt_" + name), FileMode.Create))
                {
                    myStream.Write(data, 0, data.Length);
                }
            }

            AssetDatabase.Refresh();
        }
    }
}
