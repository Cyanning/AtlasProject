using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Plugins.C_;

namespace Editor
{
    public struct ModelVisibleInfo
    {
        public int Gender;
        public string[] ModelDisplayed;
        public string[] ModelTranslucent;
    }

    public class PrefabData : MonoBehaviour
    {
        public static bool GetModelVisible(out ModelVisibleInfo modelInfo)
        {
            modelInfo = new ModelVisibleInfo {Gender = 0};

            // 检查模型性别
            var rootObj = GameObject.Find("BodyMale");
            if (rootObj is null || !rootObj.activeInHierarchy)
            {
                rootObj = GameObject.Find("BodyFemale");
                if (rootObj is null || !rootObj.activeInHierarchy) return false;
                modelInfo.Gender = 1;
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
            var foramen = GameObject.Find(modelInfo.Gender == 0 ? "ForamensMale" : "ForamensFemale");
            if (foramen is not null)
            {
                foreach (var obj in foramen.GetComponentsInChildren<Transform>())
                {
                    if (!obj.gameObject.activeInHierarchy || obj.transform.childCount > 0 || obj.name[^8] != '~')
                        continue;
                    modelDisplayed.Add(obj.transform.name[^7..]);
                }
            }

            if (modelDisplayed.Count == 0 && modelTranslucent.Count == 0) return false;
            modelInfo.ModelDisplayed = modelDisplayed.ToArray();
            modelInfo.ModelTranslucent = modelTranslucent.ToArray();
            return true;
        }
    }
}
