using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using Plugins.C_.models;

namespace Plugins.C_
{
    public class TestGetData : MonoBehaviour
    {
        public static string TEST_RESOURCE_PATH =
            Path.Combine(Application.dataPath, "StreamingAssets", "Test", "resource_ab");

        public static string TEST_MODEL_PATH =
            Path.Combine(Application.dataPath, "StreamingAssets", "Test", "encypt_malemodel");

        public static string TEST_NEW_RESOURCE_PATH =
            Path.Combine(Application.dataPath, "StreamingAssets", "Test", "resource_ab");

        public static string TEST_NEW_MODEL_MALE_PATH =
            Path.Combine(Application.dataPath, "StreamingAssets", "Test", "encypt_malemodel");

        public Dictionary<string, string> AllObject;
        string nowValue = "";

        private void Awake()
        {
        }

        // Start is called before the first frame update
        void Start()
        {
            LoadFile();
        }

        AssetBundle ab1;
        AssetBundle myLoadedAssetBundle;
        private readonly StringBuilder _bodys;

        public void LoadFile()
        {
            GameObject go;
            //Debug.Log("传过来的路径为："+modelPath);

            if (ab1 == null)
            {
                ab1 = AssetBundle.LoadFromFile(TEST_RESOURCE_PATH);
            }

            var fileStream = new MyStream(TEST_MODEL_PATH, FileMode.Open, FileAccess.Read, FileShare.None, 1024 * 64,
                false);
            myLoadedAssetBundle = AssetBundle.LoadFromStream(fileStream);
            go = Instantiate(myLoadedAssetBundle.LoadAsset<GameObject>("全身人体"));

            fileStream.Close();

            GetBodyAllData(go.transform);

            //string[] line = { _bodys.ToString() };
            File.WriteAllText(@"F:\allBody.txt", _bodys.ToString());
        }

        private void GetBodyAllData(Transform model)
        {
            Debug.Log("Run for add model.");
            foreach (Transform child in model)
            {
                if (child == null) continue;

                if (child.childCount > 0)
                {
                    GetBodyAllData(child);
                }
                else
                {
                    var modelName = child.name;
                    if (modelName.Contains("~"))
                    {
                        _bodys.Append(modelName.Substring(modelName.LastIndexOf('~') + 1)).Append(";");
                    }
                    else
                    {
                        _bodys.Append(modelName).Append(";");
                    }
                }
            }
        }

        public void GetCubeData()
        {
            GameObject obj = GameObject.Find("Cube");
            int i = 0;
            while (i < obj.transform.childCount)
            {
                nowValue = obj.transform.GetChild(i).name;
                getObject(obj.transform.GetChild(i));
                i++;
            }

            string data = "[";
            foreach (KeyValuePair<string, string> items in AllObject)
            {
                data += "{\"value\":\"" + items.Key + "\",\"action_id\":\"" + items.Value + "\"},";
            }

            data += "]";
            string[] line = { data };
            File.WriteAllLines(@"F:\cccccccc.txt", line);
        }

        public void getObject(Transform t)
        {
            int i = 0;
            if (t.childCount == 0)
            {
                string key = t.name.Substring(t.name.LastIndexOf("~") + 1);
                if (AllObject.ContainsKey(key))
                {
                    string[] sArray = AllObject[key].Split(';');
                    bool isHas = false;
                    foreach (string s in sArray)
                    {
                        if (s.Equals(nowValue))
                        {
                            isHas = true;
                        }
                    }

                    if (!isHas)
                    {
                        AllObject[key] = AllObject[key] + nowValue + ";";
                    }
                }
                else
                {
                    AllObject.Add(key, nowValue + ";");
                }
            }
            else
            {
                while (i < t.childCount)
                {
                    Transform parent = t.GetChild(i);


                    if (parent.childCount > 0)
                    {
                        getObject(parent);
                    }
                    else if (parent.childCount == 0)
                    {
                        string key = t.name.Substring(t.name.LastIndexOf("~") + 1);
                        if (AllObject.ContainsKey(key))
                        {
                            string[] sArray = AllObject[key].Split(';');
                            bool isHas = false;
                            foreach (string s in sArray)
                            {
                                if (s.Equals(nowValue))
                                {
                                    isHas = true;
                                }
                            }

                            if (!isHas)
                            {
                                AllObject[key] = AllObject[key] + nowValue + ";";
                            }
                        }
                        else
                        {
                            AllObject.Add(key, nowValue + ";");
                        }
                    }

                    i++;
                }
            }
        }
    }
}
