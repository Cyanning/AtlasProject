using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Plugins.C_;
using Plugins.C_.models;

namespace Editor
{
    public class GetShowModelOnDisplay : MonoBehaviour
    {
        public static Atlas GetValueDisplayed()
        {
            var atlas = new Atlas();

            // 检查模型，男性模型优先
            var rootObj = GameObject.Find("BodyMale");
            atlas.gender = 0;
            if (rootObj == null || !rootObj.activeInHierarchy)
            {
                rootObj = GameObject.Find("BodyFemale");
                atlas.gender = 1;
                if (rootObj == null || !rootObj.activeInHierarchy)
                    throw new FileLoadException("No model found");
            }

            // 为模型id数组建立临时哈希变量（避免重复）
            var modelDisplayed = new HashSet<string>();
            var modelTranslucent = new HashSet<string>();

            // 获取所有显示的模型
            foreach (var obj in rootObj.GetComponentsInChildren<Transform>())
            {
                if (!obj.gameObject.activeInHierarchy || obj.transform.childCount > 0 || obj.name[^8] != '~')
                    continue;

                var value = obj.transform.name[^7..];
                if (obj.GetComponent<ModelTranslucent>().isTranslucnet)
                {
                    modelTranslucent.Add(value);
                }

                modelDisplayed.Add(value);
            }

            // 获取骨性标志的孔洞模型
            var foramen = GameObject.Find(atlas.gender == 1 ? "ForamensFemale" : "ForamensMale");
            if (foramen != null)
            {
                foreach (var obj in foramen.GetComponentsInChildren<Transform>())
                {
                    if (!obj.gameObject.activeInHierarchy || obj.transform.childCount > 0 || obj.name[^8] != '~')
                        continue;
                    modelDisplayed.Add(obj.transform.name[^7..]);
                }
            }

            // 转换为数组存下来
            atlas.modelDisplayed = modelDisplayed.ToArray();
            atlas.modelTranslucent = modelTranslucent.ToArray();
            return atlas;
        }

        public static void SaveValueDisplayed(Atlas atlas)
        {
            // 如果存在模型文件则保留视角数据
            var atlasPath = Path.Combine(Application.dataPath, "Atlas_database", $"Atlas_{atlas.name}.json");
            if (File.Exists(atlasPath)) // 判断文件是否存在，如果有记录视角要保留视角数据
            {
                var oldAtlas = JsonUtility.FromJson<Atlas>(File.ReadAllText(atlasPath));
                atlas.cameraPositionX = oldAtlas.cameraPositionX;
                atlas.cameraPositionY = oldAtlas.cameraPositionY;
                atlas.cameraPositionZ = oldAtlas.cameraPositionZ;
                atlas.cameraRotationX = oldAtlas.cameraRotationX;
                atlas.cameraRotationY = oldAtlas.cameraRotationY;
                atlas.cameraRotationZ = oldAtlas.cameraRotationZ;
            }

            File.WriteAllText(atlasPath, JsonUtility.ToJson(atlas));
            print($"新的图谱 {atlas.name} 数据已建立");
        }
    }
}
