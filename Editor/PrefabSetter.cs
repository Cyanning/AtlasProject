using System.Collections.Generic;
using UnityEngine;
using Plugins.C_;
using Editor.models;

namespace Editor
{
    public abstract class PrefabSetter
    {
        private readonly HashSet<int> checkList;
        protected abstract bool InitialValue { get; }

        protected PrefabSetter(HashSet<int> checkList)
        {
            this.checkList = checkList ?? new HashSet<int>();
        }

        protected abstract bool LogicalCalculus(bool parentFlag, bool childFlag);

        protected abstract void CheckState(bool flag, GameObject nodeGo);

        public bool Setting(GameObject nodeGo)
        {
            var nodeTf = nodeGo.transform;
            var childCount = nodeTf.childCount;
            var flag = InitialValue;

            if (childCount > 0)
            {
                for (var i = 0; i < childCount; i++)
                {
                    flag = LogicalCalculus(flag, Setting(nodeTf.GetChild(i).gameObject));
                }
            }
            else if (BodyStruct.ByPrefabName(nodeTf.name, out var body))
            {
                flag = checkList.Contains(body.value);
            }

            CheckState(flag, nodeGo);

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

        protected override void CheckState(bool flag, GameObject nodeGo)
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

        protected override void CheckState(bool flag, GameObject nodeGo)
        {
            if (nodeGo.TryGetComponent<ModelTranslucent>(out var modelTranslucent))
            {
                modelTranslucent.isTranslucnet = flag;
            }
        }
    }
}
