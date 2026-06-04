using System.Collections.Generic;
using UnityEngine;
using Plugins.C_;
using Editor.models;

namespace Editor
{
    public abstract class PrefabSetter
    {
        private readonly HashSet<int> _checkList;
        protected abstract bool InitialValue { get; }

        protected PrefabSetter(HashSet<int> checkList)
        {
            _checkList = checkList ?? new HashSet<int>();
        }

        protected abstract bool LogicalCalculus(bool parentFlag, bool childFlag);

        protected abstract void CheckState(GameObject nodeGo, bool flag);

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
