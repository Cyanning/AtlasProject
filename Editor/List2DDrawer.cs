using UnityEditor;
using UnityEngine;
using Plugins.C_;


namespace Editor
{
    [CustomPropertyDrawer(typeof(PairLocaltion))]
    public class List2DDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 开始绘制属性
            label.text = "";
            EditorGUI.BeginProperty(position, label, property);

            // 绘制前缀标签
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // 缩进取消
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // 计算每个字段宽度
            const float labelWidth = 14f;
            const float spacing = 4f;
            var fieldWidth = position.width / 2 - spacing - labelWidth;

            // 定义 L/R 字段的 Rect
            var lLabelRect = new Rect(position.x, position.y, labelWidth, position.height);
            var lFieldRect = new Rect(lLabelRect.xMax, position.y, fieldWidth, position.height);

            var rLabelRect = new Rect(lFieldRect.xMax + spacing, position.y, labelWidth, position.height);
            var rFieldRect = new Rect(rLabelRect.xMax, position.y, fieldWidth, position.height);

            // 获取属性
            var lProp = property.FindPropertyRelative("l");
            var rProp = property.FindPropertyRelative("r");

            // 绘制 L/R 标签与输入框
            EditorGUI.LabelField(lLabelRect, "L");
            EditorGUI.PropertyField(lFieldRect, lProp, GUIContent.none);

            EditorGUI.LabelField(rLabelRect, "R");
            EditorGUI.PropertyField(rFieldRect, rProp, GUIContent.none);

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}
