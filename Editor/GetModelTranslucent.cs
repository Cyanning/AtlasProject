using UnityEngine;
using UnityEditor;
using Plugins.C_;


namespace Editor
{
    [InitializeOnLoad]
    public static class HierarchyCheckbox
    {
        static HierarchyCheckbox()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
        }

        private static void OnHierarchyGUI(int instanceID, Rect selectionRect)
        {
            if (EditorApplication.isPlaying) return;
            
            var obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (obj == null || !obj.name.Contains("~")) return;

            // 定义checkbox的区域：放在右边
            var activeToggleRect = new Rect(selectionRect.xMax, selectionRect.y, 18, selectionRect.height);
            var translucentToggleRect = new Rect(selectionRect.xMax - 18, selectionRect.y, 18, selectionRect.height);

            // 获取状态
            var activeState = obj.activeInHierarchy;
            var translucentState = obj.GetComponent<ModelTranslucent>().isTranslucnet;

            // 绘制勾选框
            var newActiveState = GUI.Toggle(activeToggleRect, activeState, GUIContent.none);
            var newTranslucentState = GUI.Toggle(translucentToggleRect, translucentState, GUIContent.none);

            // 如果状态改变，更新
            if (newActiveState != activeState)
            {
                obj.SetActive(newActiveState);
            }

            if (newTranslucentState != translucentState)
            {
                obj.GetComponent<ModelTranslucent>().SetTranslucentState(newTranslucentState);
            }
        }
    }
}
