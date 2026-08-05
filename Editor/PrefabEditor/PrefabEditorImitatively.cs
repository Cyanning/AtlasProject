using System.Linq;
using UnityEngine;
using Plugins.models;


namespace Editor.PrefabEditor
{
    public class PrefabEditorImitatively
    {
        private readonly GameObject[] _gameObjectsUntreated;

        public PrefabEditorImitatively(GameObject target)
        {
            var gameObjectsUntreated =
                from GoUntreated in target.GetComponentsInChildren<Transform>(true)
                where GoUntreated.childCount == 0
                select GoUntreated.gameObject;

            _gameObjectsUntreated = gameObjectsUntreated.ToArray();
        }

        /// <summary>
        /// 递归形式让两个预制体结构统一
        /// </summary>
        public void PrefabStructCopying(Transform originParent, Transform targetParent)
        {
            for (var i = 0; i < originParent.childCount; i++)
            {
                var originNode = originParent.GetChild(i);

                // 规范预制体名字
                var prefabName = originNode.name;
                if (prefabName.StartsWith("Foramens_") || prefabName.StartsWith("foramens_"))
                {
                    prefabName = prefabName.Remove(0, 9);
                }

                // 拿取结构性命名
                if (!BodyStruct.GetFromPrefab(prefabName, out var body))
                {
                    Debug.Log($"对象 {originNode.name} 无法识别");
                    continue;
                }

                if (originNode.childCount > 0) // 父级节点则继续递归
                {
                    PrefabStructCopying(
                        originNode,
                        GetTransfromNode(targetParent, body.ToString())
                    );
                }
                else  // 设置叶子节点的对象相关数据
                {
                    var originGo = originNode.gameObject;
                    if (TryGetGameObject(originGo, out var newGo))
                    {
                        GameObjectRendererCopying(originGo, newGo);
                        newGo.name = body.ToString();
                        newGo.transform.SetParent(targetParent);
                    }
                    else
                    {
                        Debug.Log($"模型 {originGo} 非发现匹配项");
                    }
                }
            }
        }

        /// <summary>
        /// 查找某个transform的子transform中名字为name的GameObject实例
        /// </summary>
        private static Transform GetTransfromNode(Transform parent, string name)
        {
            for (var i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child.name == name)
                {
                    return child;
                }
            }

            var newChild = new GameObject(name);
            newChild.transform.SetParent(parent, false);

            return newChild.transform;
        }

        /// <summary>
        /// 从所有子对象中查找目标对象
        /// </summary>
        private bool TryGetGameObject(GameObject origin, out GameObject target)
        {
            var targetName = origin.GetComponent<MeshFilter>().sharedMesh.name;

            foreach (var go in _gameObjectsUntreated)
            {
                if (go.name == targetName)
                {
                    target = go;
                    return true;
                }
            }

            target = null;
            return false;
        }

        /// <summary>
        /// 复制模型对象相关数据
        /// </summary>
        private static void GameObjectRendererCopying(GameObject origin, GameObject target)
        {
            var targetMaterials = origin.GetComponent<MeshRenderer>().sharedMaterials;
            target.GetComponent<MeshRenderer>().SetMaterials(new(targetMaterials));
        }

        public static void PrefabNameRetailor(GameObject target)
        {
            foreach (var child in target.GetComponentsInChildren<Transform>())
            {
                var prefabName = child.name;

                if (prefabName.StartsWith("Foramens_") || prefabName.StartsWith("foramens_"))
                {
                    prefabName = prefabName.Remove(0, 9);
                }

                child.gameObject.name = prefabName;
            }
        }
    }
}
