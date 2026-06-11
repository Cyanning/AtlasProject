using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Editor
{
    // 在当前场景中按名称片段查找多个 Transform，并计算它们的最近共同父级。
    public static class CommonParentFinder
    {
        public static bool Find(string[] values, out Transform commonParent)
        {
            commonParent = null;

            if (values is null || values.Length == 0) return false;

            // 1. 收集所有匹配的 Transform（去重并过滤无效值）
            var targets = new List<Transform>();
            foreach (var val in values)
            {
                if (string.IsNullOrWhiteSpace(val)) continue;

                var target = FindFirstByNameContains(val);
                if (target != null) targets.Add(target);
            }

            if (targets.Count == 0) return false;

            // 2. 先确定第一个 tf 的最近父级
            commonParent = targets[0].parent;

            for (var i = 1; i < targets.Count; i++)
            {
                commonParent = GetLowestCommonParent(commonParent, targets[i]);

                // 如果两个物体找不到共同父级，则返回空
                if (commonParent == null) return false;
            }

            return true;
        }

        // 从当前激活场景的根对象开始查找，true 表示包含未激活的子物体。
        private static Transform FindFirstByNameContains(string value)
        {
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                foreach (var child in root.GetComponentsInChildren<Transform>(true))
                {
                    if (child.name.EndsWith($"~{value}"))
                    {
                        return child;
                    }
                }
            }

            return null;
        }

        // 最近共同父级算法：
        // 1. 记录 x 到根节点路径上的所有祖先。
        // 2. y 从自身一路向上，第一次遇到的已记录节点就是最近共同父级。
        private static Transform GetLowestCommonParent(Transform x, Transform y)
        {
            var ancestors = new HashSet<Transform>();

            while (x is not null)
            {
                ancestors.Add(x);
                x = x.parent;
            }

            while (y is not null)
            {
                if (ancestors.Contains(y))
                {
                    return y;
                }

                y = y.parent;
            }

            return null;
        }
    }
}
