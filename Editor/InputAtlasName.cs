using Plugins.C_.models;
using UnityEditor;
using UnityEngine;


namespace Editor
{
    public class InputAtlasName : EditorWindow
    {
        private Atlas _atlas;
        private readonly string[] _boneMarkClasses = { "不开启", "部位", "表面", "标志" };

        //MenuItem会在unity菜单栏添加自定义新项
        [MenuItem("脚本/设置图谱展示的模型")]
        public static void ShowInputWindowMale()
        {
            GetWindow<InputAtlasName>("图谱名称");
        }

        private void OnEnable()
        {
            _atlas = GetShowModelOnDisplay.GetValueDisplayed();
            _atlas.name = "";
            _atlas.boneMarkType = 0;
        }

        private void OnGUI()
        {
            GUILayout.Label("请输入图谱名称：", EditorStyles.boldLabel);
            _atlas.name = EditorGUILayout.TextField("图谱名称", _atlas.name);
            GUILayout.Space(10);

            GUILayout.Label("确认需要透明的模型id：", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                string.Join(", ", _atlas.modelTranslucent),
                EditorStyles.wordWrappedLabel
            );
            GUILayout.Space(10);

            GUILayout.Label("选择骨性标志类型：", EditorStyles.boldLabel);
            for (var i = 0; i < _boneMarkClasses.Length; i++)
            {
                var newSelected = EditorGUILayout.ToggleLeft(_boneMarkClasses[i], _atlas.boneMarkType == i);
                if (newSelected && _atlas.boneMarkType != i)
                {
                    _atlas.boneMarkType = i; // 确保只有一个被选中
                }
            }
            GUILayout.Space(10);

            if (GUILayout.Button("确定") && _atlas.name.Length > 0)
            {
                GetShowModelOnDisplay.SaveValueDisplayed(_atlas);
                Close();
            }
        }
    }
}
