using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Plugins.C_;

namespace Editor
{
    public readonly struct ModelData
    {
        public readonly int Gender;
        public readonly string[] ModelDisplayed;
        public readonly string[] ModelTranslucent;

        public ModelData(int gender, string[] modelDisplayed, string[] modelTranslucent)
        {
            Gender = gender;
            ModelDisplayed = modelDisplayed;
            ModelTranslucent = modelTranslucent;
        }
    }

    public static class PrefabData
    {
        private static readonly string[] ModelNames =
        {
            "BodyMale", "BodyFemale", "ForamensMale", "ForamensFemale"
        };

        public static ModelData EncodetModelVisible()
        {
            // 建立数据缓存变量，避免value重复使用HashSet
            var gender = -1;
            var modelDisplayed = new HashSet<string>();
            var modelTranslucent = new HashSet<string>();

            // 引用显示中的模型
            for (var i = 0; i < 2; i++)
            {
                var body = GameObject.Find(ModelNames[i]);
                if (body is null) continue;

                // 设置性别
                gender = i;

                // 获取所有显示的模型 value
                foreach (var obj in body.GetComponentsInChildren<Transform>())
                {
                    if (obj.childCount > 0 || obj.name[^8] != '~') continue;
                    var value = obj.name[^7..];
                    modelDisplayed.Add(value);

                    if (obj.TryGetComponent<ModelTranslucent>(out var translucent) && translucent.isTranslucnet)
                    {
                        modelTranslucent.Add(value);
                    }
                }

                break;
            }

            // 主模型不可为空
            if (gender >= 0)
            {
                // 获取骨性标志的孔洞模型
                var foramen = GameObject.Find(ModelNames[gender + 2]);
                if (foramen is not null)
                {
                    var foramens =
                        from obj in foramen.GetComponentsInChildren<Transform>()
                        where obj.transform.childCount == 0 && obj.name[^8] == '~'
                        select obj.transform.name[^7..];

                    modelDisplayed.UnionWith(foramens.ToHashSet());
                }
            }

            return new ModelData(
                gender,
                modelDisplayed.ToArray(),
                modelTranslucent.ToArray()
            );
        }

        public static void DecodeModelVisible(ModelData modelData)
        {
            // 数据处理类的实例
            var modelSetter = new ModelSetter(
                modelData.ModelDisplayed.ToHashSet(),
                modelData.ModelTranslucent.ToHashSet()
            );

            // 通过场景查找隐藏的模型对象
            foreach (var rootObj in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                var index = Array.IndexOf(ModelNames, rootObj.name);
                if (index < 0) continue;

                if (index % 2 == modelData.Gender)
                {
                    modelSetter.SetModelVisible(rootObj);
                }
                else
                {
                    rootObj.SetActive(false);
                }
            }
        }
    }

    public class ModelSetter
    {
        private readonly HashSet<string> _checkofActive;
        private readonly HashSet<string> _checkofTranslucent;

        public ModelSetter(HashSet<string> activeSet = null, HashSet<string> translucenteSet = null)
        {
            _checkofActive = activeSet ?? new HashSet<string>();
            _checkofTranslucent = translucenteSet ?? new HashSet<string>();
        }

        public (bool isActive, bool isTranslucent) SetModelVisible(GameObject nodeGo)
        {
            var nodeTf = nodeGo.transform;
            var childCount = nodeTf.childCount;
            var isActive = false;
            var isTranslucent = true;

            if (childCount > 0)
            {
                for (var i = 0; i < childCount; i++)
                {
                    isActive |= SetModelVisible(nodeTf.GetChild(i).gameObject).isActive;
                    isTranslucent &= SetModelVisible(nodeTf.GetChild(i).gameObject).isTranslucent;
                }
            }
            else
            {
                var value = nodeGo.name[^7..];
                isActive = _checkofActive.Contains(value);
                isTranslucent = _checkofTranslucent.Contains(value);
            }

            if (nodeGo.activeSelf != isActive)
            {
                nodeGo.SetActive(isActive);
            }

            if (nodeGo.TryGetComponent<ModelTranslucent>(out var modelTranslucent))
            {
                modelTranslucent.isTranslucnet = isTranslucent;
            }

            return (isActive, isTranslucent);
        }
    }
}
