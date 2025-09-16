using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Plugins.C_;
using Plugins.C_.models;

namespace Editor
{
    public class GetShowModelOnDisplay : MonoBehaviour
    {
        public static Atlas GetValueDisplayed()
        {
            var newAtlas = new Atlas { modelDisplayed = new List<string>(), modelTranslucent = new List<string>() };

            // 检查模型，男性模型优先
            var rootObj = GameObject.Find("BodyMale");
            newAtlas.gender = 0;
            if (rootObj == null || !rootObj.activeInHierarchy)
            {
                rootObj = GameObject.Find("BodyFemale");
                newAtlas.gender = 1;
                if (rootObj == null || !rootObj.activeInHierarchy)
                    throw new FileLoadException("No model found");
            }

            // 获取所有显示的模型
            foreach (var obj in rootObj.GetComponentsInChildren<Transform>())
            {
                if (!obj.gameObject.activeInHierarchy || obj.transform.childCount > 0 || !obj.name.Contains("~"))
                {
                    continue;
                }

                var value = obj.transform.name.Split("~")[^1].Trim();
                if (obj.GetComponent<ModelTranslucent>().isTranslucnet)
                {
                    newAtlas.modelTranslucent.Add(value);
                }

                newAtlas.modelDisplayed.Add(value);
            }

            return newAtlas;
        }

        public static void SaveValueDisplayed(string atlasName, Atlas atlasData)
        {
            // 如果存在模型文件则保留视角数据
            var atlasPath = Path.Combine(Application.dataPath, "Atlas_database", $"Atlas_{atlasName}.json");
            if (File.Exists(atlasPath)) // 判断文件是否存在，如果有记录视角要保留视角数据
            {
                var oldAtlas = JsonUtility.FromJson<Atlas>(File.ReadAllText(atlasPath));
                atlasData.cameraPositionX = oldAtlas.cameraPositionX;
                atlasData.cameraPositionY = oldAtlas.cameraPositionY;
                atlasData.cameraPositionZ = oldAtlas.cameraPositionZ;
                atlasData.cameraRotationX = oldAtlas.cameraRotationX;
                atlasData.cameraRotationY = oldAtlas.cameraRotationY;
                atlasData.cameraRotationZ = oldAtlas.cameraRotationZ;
            }

            File.WriteAllText(atlasPath, JsonUtility.ToJson(atlasData));
            Debug.Log($"新的图谱 {atlasName} 数据已建立");
        }
    }
}
