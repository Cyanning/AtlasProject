using UnityEngine;
using UnityEditor;
using Plugins.C_;


namespace Editor
{
    [InitializeOnLoad]
    public static class CustomCheckbox
    {
        static CustomCheckbox()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
        }

        private static void OnHierarchyGUI(int instanceID, Rect selectionRect)
        {
            if (EditorApplication.isPlaying) return;

            var obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (obj == null || !obj.name.Contains("~")) return;

            // 获取状态
            var activeState = obj.activeInHierarchy;
            // 定义enable的checkbox的区域：放在右边
            var newActiveState = GUI.Toggle(
                new Rect(selectionRect.xMax, selectionRect.y, 18, selectionRect.height),
                activeState,
                GUIContent.none
            );
            // 如果状态改变，更新
            if (newActiveState != activeState)
                obj.SetActive(newActiveState);

            // 检测是否包含透明
            var objTranslucent = obj.GetComponent<ModelTranslucent>();
            if (objTranslucent == null) return;
            // 获取状态
            var translucentState = objTranslucent.isTranslucnet;
            // 定义透明的checkbox的区域：放在右边
            var newTranslucentState = GUI.Toggle(
                new Rect(selectionRect.xMax - 18, selectionRect.y, 18, selectionRect.height),
                translucentState,
                GUIContent.none
            );
            // 如果状态改变，更新
            if (newTranslucentState != translucentState)
                obj.GetComponent<ModelTranslucent>().SetTranslucentState(newTranslucentState);
        }
    }
}
