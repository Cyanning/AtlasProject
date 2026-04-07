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
            if (root == null)
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
                if (go == null)
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
                    if (childTf.childCount > 0) continue;

                    if (BodyStruct.ByPrefabName(go.name, out var body))
                    {
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
            if (go == null) return null;

            // 获取所有显示的模型 value
            foreach (var childTf in ForEachChildren(go.transform))
            {
                if (childTf.childCount > 0) continue;

                if (BodyStruct.ByPrefabName(go.name, out var body) &&
                    childTf.TryGetComponent<ModelTranslucent>(out var translucent) &&
                    translucent.isTranslucnet)
                {
                    values.Add(body);
                }
            }

            return new BodyStructWrapper(gender, values);
        }

        public static void DecodeModelActive(BodyStructWrapper bodys)
        {
            // 数据处理类的实例
            var setter = new ActiveSetter(bodys.ValuesAsInt().ToHashSet());

            // 通过场景查找隐藏的模型对象
            foreach (var rootObj in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                var index = Array.IndexOf(ModelNames, rootObj.name);
                if (index < 0) continue;

                if (index % 2 == bodys.gender)
                {
                    setter.SetActive(rootObj);
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
            var modelSetter = new TranslucentSetter(bodys.ValuesAsInt().ToHashSet());

            // 通过场景查找隐藏的模型对象
            foreach (var rootObj in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                var index = Array.IndexOf(ModelNames, rootObj.name);
                if (index < 0) continue;

                if (index % 2 == bodys.gender)
                {
                    modelSetter.SetTranslucent(rootObj);
                }
                else
                {
                    rootObj.SetActive(false);
                }
            }
        }
    }

    public class ActiveSetter
    {
        private readonly HashSet<int> _checkofActive;

        public ActiveSetter(HashSet<int> activeSet)
        {
            _checkofActive = activeSet ?? new HashSet<int>();
        }

        public bool SetActive(GameObject nodeGo)
        {
            var nodeTf = nodeGo.transform;
            var childCount = nodeTf.childCount;
            var isActive = false;

            if (childCount > 0)
            {
                for (var i = 0; i < childCount; i++)
                {
                    isActive |= SetActive(nodeTf.GetChild(i).gameObject);
                }
            }
            else if (BodyStruct.ByPrefabName(nodeTf.name, out var body))
            {
                isActive = _checkofActive.Contains(body.value);
            }

            if (nodeGo.activeSelf != isActive)
            {
                nodeGo.SetActive(isActive);
            }


            return isActive;
        }
    }

    public class TranslucentSetter
    {
        private readonly HashSet<int> _checkofTranslucent;

        public TranslucentSetter(HashSet<int> translucenteSet)
        {
            _checkofTranslucent = translucenteSet ?? new HashSet<int>();
        }

        public bool SetTranslucent(GameObject nodeGo)
        {
            var nodeTf = nodeGo.transform;
            var childCount = nodeTf.childCount;
            var isTranslucent = true;

            if (childCount > 0)
            {
                for (var i = 0; i < childCount; i++)
                {
                    isTranslucent &= SetTranslucent(nodeTf.GetChild(i).gameObject);
                }
            }
            else if (BodyStruct.ByPrefabName(nodeTf.name, out var body))
            {
                isTranslucent = _checkofTranslucent.Contains(body.value);
            }

            if (nodeGo.TryGetComponent<ModelTranslucent>(out var modelTranslucent))
            {
                modelTranslucent.isTranslucnet = isTranslucent;
            }

            return isTranslucent;
        }
    }
}
