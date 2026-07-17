using UnityEditor;
using UnityEngine;
using Plugins.models;
using Editor.PrefabEditor;

namespace Editor.Atlas
{
    public class AtlasFormReader : EditorWindow
    {
        private string _atlasName;

        [MenuItem("Customer/图谱-读取模型")]
        public static void ShowWindow()
        {
            GetWindow<AtlasFormReader>("读取图谱");
        }

        private void OnEnable()
        {
            _atlasName = CookieWrapper.Reading();
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
                PrefabCollection.DecodeModelActive(new BodyStructWrapper(atlas.gender, atlas.modelDisplayed));
                PrefabCollection.DecodeModelTranslucent(new BodyStructWrapper(atlas.gender, atlas.modelTranslucent));

                CookieWrapper.Create(_atlasName);
                Debug.Log($"加载 {_atlasName} 成功");
            }
            else
            {
                Debug.Log($"找不到 {_atlasName} 的数据");
            }
        }
    }
}
