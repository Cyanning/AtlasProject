using UnityEditor;
using UnityEngine;
using Plugins.C_.models;

namespace Editor
{
    public class AtlasFormReader : EditorWindow
    {
        private string _atlasName;

        [MenuItem("自定义功能/读取图谱")]
        public static void ShowWindow()
        {
            GetWindow<AtlasFormReader>("读取图谱");
        }

        private void OnEnable()
        {
            _atlasName = "";
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("图谱名称（必填）：", EditorStyles.boldLabel);
            _atlasName = EditorGUILayout.TextField(_atlasName);

            EditorGUILayout.Space(20);
            if (GUILayout.Button("加载图谱") && _atlasName.Length > 0)
            {
                AtlasRead();
                Close();
            }
        }

        private void AtlasRead()
        {
            if (AtlasFactory.Load(_atlasName, out var atlas))
            {
                PrefabData.DecodeModelVisible(
                    new ModelData(atlas.gender, atlas.modelDisplayed, atlas.modelTranslucent)
                );
                CookieWrapper.Create(_atlasName);
                Debug.Log($"加载{_atlasName}成功");
            }
        }
    }
}
