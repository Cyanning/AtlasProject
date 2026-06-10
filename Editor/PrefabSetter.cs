using System.Collections.Generic;
using UnityEngine;
using Plugins.C_;
using Editor.models;

namespace Editor
{
    public abstract class PrefabSetter
    {
        // 需要操作的模型id
        private readonly HashSet<int> _checkList;

        // 模型状态码在计算前的初始值
        protected abstract bool InitialValue { get; }

        // 父模型和子模型同字段关联性的计算方式
        protected abstract bool LogicalCalculus(bool parentFlag, bool childFlag);

        // 构造函数，该类必须传递需要操作的模型id列表
        protected PrefabSetter(HashSet<int> checkList)
        {
            _checkList = checkList ?? new HashSet<int>();
        }

        // 改变模型状态的算法
        protected abstract void CheckState(GameObject nodeGo, bool flag);

        // 递归设置模型状态的函数
        public bool Setting(GameObject nodeGo)
        {
            var nodeTf = nodeGo.transform;
            var childCount = nodeTf.childCount;
            var flag = InitialValue;

            if (childCount > 0)
            {
                for (var i = 0; i < childCount; i++)
                {
                    var childFlag = Setting(nodeTf.GetChild(i).gameObject);
                    flag = LogicalCalculus(flag, childFlag);
                }
            }
            else if (BodyStruct.ByPrefabName(nodeTf.name, out var body))
            {
                flag = _checkList.Contains(body.value);
            }

            CheckState(nodeGo, flag);

            return flag;
        }
    }


    // 设置模型显示状态: activeInHierarchy 属性
    public class PrefabSetActive : PrefabSetter
    {
        protected override bool InitialValue => false;

        public PrefabSetActive(HashSet<int> checkList) : base(checkList) { }

        protected override bool LogicalCalculus(bool parentFlag, bool childFlag)
        {
            return parentFlag | childFlag;
        }

        protected override void CheckState(GameObject nodeGo, bool flag)
        {
            if (nodeGo.activeSelf != flag)
            {
                nodeGo.SetActive(flag);
            }
        }
    }

    // 设置模型透明状态: ModelTranslucent 属性
    public class PrefabSetTranslucent : PrefabSetter
    {
        protected override bool InitialValue => true;

        public PrefabSetTranslucent(HashSet<int> checkList) : base(checkList) { }

        protected override bool LogicalCalculus(bool parentFlag, bool childFlag)
        {
            return parentFlag & childFlag;
        }

        protected override void CheckState(GameObject nodeGo, bool flag)
        {
            if (nodeGo.TryGetComponent<ModelTranslucent>(out var modelTranslucent))
            {
                modelTranslucent.isTranslucnet = flag;
            }
        }
    }
}
