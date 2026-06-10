using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Plugins.C_;
using Editor.models;

namespace Editor
{
    public static class PrefabEditor
    {
        private static readonly string[] ModelNames =
        {
            "BodyMale", "BodyFemale", "ForamensMale", "ForamensFemale"
        };

        private static IEnumerable<Transform> ForEachChildren(Transform root)
        {
            if (root is null)
            {
                yield break;
            }

            var stack = new Stack<Transform>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                var node = stack.Pop();
                var i = node.childCount - 1;

                while (i >= 0)
                {
                    stack.Push(node.GetChild(i));
                    i--;
                }

                yield return node;
            }
        }

        public static BodyStructWrapper EncodetModelActive()
        {
            // 建立数据缓存变量，避免value重复使用HashSet
            var gender = -1;
            var values = new HashSet<BodyStruct>();

            // 引用显示中的模型
            var i = 0;
            while (i < 4)
            {
                var go = GameObject.Find(ModelNames[i]);
                if (go is null)
                {
                    if (i < 2)
                    {
                        i += 1;
                        continue;
                    }

                    break;
                }

                // 设置性别
                gender = i % 2;

                // 获取所有显示的模型 value
                foreach (var childTf in ForEachChildren(go.transform))
                {
                    if (
                        childTf.childCount == 0 &&
                        childTf.gameObject.activeInHierarchy &&
                        BodyStruct.ByPrefabName(childTf.name, out var body)
                    )
                    {
                        Debug.Log($"{body.name}={body.Value}");
                        values.Add(body);
                    }
                }

                i += 2;
            }

            return new BodyStructWrapper(gender, values);
        }

        public static BodyStructWrapper EncodetModelTranslucent(int gender)
        {
            // 建立数据缓存变量，避免value重复使用HashSet
            var values = new HashSet<BodyStruct>();

            // 引用显示中的模型
            var go = GameObject.Find(ModelNames[gender]);
            if (go is null) return null;

            // 获取所有显示的模型 value
            foreach (var childTf in ForEachChildren(go.transform))
            {
                if (
                    childTf.childCount == 0 &&
                    childTf.TryGetComponent<ModelTranslucent>(out var translucent) &&
                    translucent.isTranslucnet &&
                    BodyStruct.ByPrefabName(childTf.name, out var body)
                )
                {
                    values.Add(body);
                }
            }

            return new BodyStructWrapper(gender, values);
        }

        public static void DecodeModelActive(BodyStructWrapper bodys)
        {
            // 数据处理类的实例
            var setter = new PrefabSetActive(bodys.ValuesAsInt().ToHashSet());

            // 通过场景查找隐藏的模型对象
            foreach (var rootObj in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                var index = Array.IndexOf(ModelNames, rootObj.name);
                if (index < 0) continue;

                if (index % 2 == bodys.gender)
                {
                    setter.Setting(rootObj);
                }
                else
                {
                    rootObj.SetActive(false);
                }
            }
        }

        public static void DecodeModelTranslucent(BodyStructWrapper bodys)
        {
            // 数据处理类的实例
            var modelSetter = new PrefabSetTranslucent(bodys.ValuesAsInt().ToHashSet());

            // 通过场景查找隐藏的模型对象
            foreach (var rootObj in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                var index = Array.IndexOf(ModelNames, rootObj.name);
                if (index < 0) continue;

                if (index % 2 == bodys.gender)
                {
                    modelSetter.Setting(rootObj);
                }
                else
                {
                    rootObj.SetActive(false);
                }
            }
        }
    }
}
