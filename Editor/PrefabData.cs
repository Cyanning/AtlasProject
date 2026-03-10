using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections.Generic;
using JetBrains.Annotations;
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

    public class PrefabData : MonoBehaviour
    {
        private static readonly string[] ModelNames = { "BodyMale", "BodyFemale" };
        private static readonly string[] ForamenNames = { "ForamensMale", "ForamensFemale" };

        public static ModelData EncodetModelVisible()
        {
            // 建立数据缓存变量，避免value重复使用HashSet
            var gender = -1;
            var modelDisplayed = new HashSet<string>();
            var modelTranslucent = new HashSet<string>();

            // 引用显示中的模型
            for (var i = 0; i < ModelNames.Length; i++)
            {
                var body = GameObject.Find(ModelNames[i]);
                if (body is null || !body.activeInHierarchy) continue;

                // 设置性别
                gender = i;

                // 获取所有显示的模型 value
                foreach (var obj in body.GetComponentsInChildren<Transform>())
                {
                    if (!obj.gameObject.activeInHierarchy || obj.childCount > 0 || obj.name[^8] != '~') continue;
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
                var foramen = GameObject.Find(ForamenNames[gender]);
                if (foramen?.activeInHierarchy == true)
                {
                    var foramens =
                        from obj in foramen.GetComponentsInChildren<Transform>()
                        where obj.gameObject.activeInHierarchy && obj.transform.childCount == 0 && obj.name[^8] == '~'
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
            GameObject rootObj = null;
            GameObject foramen = null;

            foreach (var rootGameObject in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (rootGameObject.name == ModelNames[modelData.Gender])
                {
                    rootObj = rootGameObject;
                }
                else if (rootGameObject.name == ForamenNames[modelData.Gender])
                {
                    foramen = rootGameObject;
                }
            }

            var modelSetter = new ModelSetter(
                modelData.ModelDisplayed.ToHashSet(), modelData.ModelTranslucent.ToHashSet()
            );

            if (rootObj is not null)
                modelSetter.SetModelVisible(rootObj);

            if (foramen is not null)
                modelSetter.SetModelVisible(foramen);
        }
    }

    public class ModelSetter
    {
        private readonly HashSet<string> _checkListofActive;
        [CanBeNull] private readonly HashSet<string> _checkListofTranslucent;

        public ModelSetter(HashSet<string> activeSet, HashSet<string> translucenteSet = null)
        {
            _checkListofActive = activeSet;
            _checkListofTranslucent = translucenteSet;
        }

        public bool SetModelVisible(GameObject node)
        {
            var isActive = false;

            var childCount = node.transform.childCount;
            if (childCount > 0)
            {
                for (var i = 0; i < childCount; i++)
                {
                    isActive |= SetModelVisible(node.transform.GetChild(i).gameObject);
                }
            }
            else
            {
                var value = node.name[^7..];
                isActive = _checkListofActive.Contains(value);

                if (_checkListofTranslucent is not null &&
                    node.TryGetComponent<ModelTranslucent>(out var modelTranslucent))
                {
                    modelTranslucent.isTranslucnet = _checkListofTranslucent.Contains(value);
                }
            }

            if (node.activeSelf != isActive)
            {
                node.SetActive(isActive);
            }

            return isActive;
        }
    }
}
