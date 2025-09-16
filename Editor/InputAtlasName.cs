using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class InputAtlasName : EditorWindow
    {
        private string _atlasName = "";

        //MenuItem会在unity菜单栏添加自定义新项
        [MenuItem("脚本/设置图谱展示的模型")]
        public static void ShowInputWindowMale()
        {
            GetWindow<InputAtlasName>("图谱名称");
        }

        private void OnGUI()
        {
            var altasData = GetShowModelOnDisplay.GetValueDisplayed();

            GUILayout.Label("请输入图谱名称：", EditorStyles.boldLabel);
            _atlasName = EditorGUILayout.TextField("图谱名称", _atlasName);
            GUILayout.Space(10);

            GUILayout.Label("确认需要透明的模型id：", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                string.Join(", ", altasData.modelTranslucent),
                EditorStyles.wordWrappedLabel
            );
            GUILayout.Space(10);

            if (GUILayout.Button("确定") && _atlasName.Length > 0)
            {
                GetShowModelOnDisplay.SaveValueDisplayed(_atlasName, altasData);
                Close();
            }
        }
    }
}
