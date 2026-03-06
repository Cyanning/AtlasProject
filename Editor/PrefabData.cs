using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections.Generic;
using JetBrains.Annotations;
using Plugins.C_;
using Plugins.C_.models;

namespace Editor
{
    public class PrefabData : MonoBehaviour
    {
        private static readonly string[] ModelNames = { "BodyMale", "BodyFemale" };
        private static readonly string[] ForamenNames = { "ForamensMale", "ForamensFemale" };

        public static AtlasItem EncodetModelVisible(AtlasItem atlas)
        {
            GameObject rootObj = null;
            GameObject foramen = null;

            foreach (var rootGameObject in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (!rootGameObject.activeInHierarchy) continue;

                var index = Array.IndexOf(ModelNames, rootGameObject.name);
                if (index >= 0)
                {
                    rootObj = rootGameObject;
                    atlas.gender = index;
                }
                else if (ForamenNames.Contains(rootGameObject.name))
                {
                    foramen = rootGameObject;
                }
            }

            //必须有模型
            if (rootObj is null) return atlas;

            // 检查孔洞模型性别
            if (foramen?.name != ForamenNames[atlas.gender])
            {
                foramen = null;
            }

            // 为模型id数组建立临时哈希变量（避免重复）
            var modelDisplayed = new HashSet<string>();
            var modelTranslucent = new HashSet<string>();

            // 获取所有显示的模型
            foreach (var obj in rootObj.GetComponentsInChildren<Transform>())
            {
                if (!obj.gameObject.activeInHierarchy || obj.childCount > 0 || obj.name[^8] != '~') continue;

                var value = obj.name[^7..];
                modelDisplayed.Add(value);

                if (obj.TryGetComponent<ModelTranslucent>(out var translucent) && translucent.isTranslucnet)
                {
                    modelTranslucent.Add(value);
                }
            }

            // 获取骨性标志的孔洞模型
            if (foramen is not null)
            {
                foreach (var obj in foramen.GetComponentsInChildren<Transform>())
                {
                    if (!obj.gameObject.activeInHierarchy || obj.transform.childCount > 0 || obj.name[^8] != '~')
                    {
                        continue;
                    }

                    modelDisplayed.Add(obj.transform.name[^7..]);
                }
            }

            if (modelDisplayed.Count == 0 && modelTranslucent.Count == 0)
            {
                return atlas;
            }

            atlas.modelDisplayed = modelDisplayed.ToArray();
            atlas.modelTranslucent = modelTranslucent.ToArray();
            return atlas;
        }

        public static void DecodeModelVisible(AtlasItem atlas)
        {
            GameObject rootObj = null;
            GameObject foramen = null;

            foreach (var rootGameObject in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (rootGameObject.name == ModelNames[atlas.gender])
                {
                    rootObj = rootGameObject;
                }
                else if (rootGameObject.name == ForamenNames[atlas.gender])
                {
                    foramen = rootGameObject;
                }
            }

            if (rootObj is null) return;

            var modelSetter = new ModelSetter(
                atlas.modelDisplayed.ToHashSet(), atlas.modelTranslucent.ToHashSet()
            );
            modelSetter.SetModelVisible(rootObj);
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
