using UnityEditor;
using UnityEngine;
using Plugins.models;
using Editor.PrefabEditor;

namespace Editor.BonemarkEditor
{
    public class ModelLoadingForBonemark : EditorWindow
    {
        private string _fileName;

        [MenuItem("Customer/加载模型--骨性标志")]
        public static void ShowWindow()
        {
            GetWindow<ModelLoadingForBonemark>("加载模型--骨性标志");
        }

        private void OnEnable()
        {
            _fileName = "";
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("名称（必填）：", EditorStyles.boldLabel);
            _fileName = EditorGUILayout.TextField(_fileName);

            EditorGUILayout.Space(20);
            if (GUILayout.Button("加载模型") && _fileName.Length > 0)
            {
                ModelLoading();
                Close();
            }
        }

        private void ModelLoading()
        {
            if (BoneFactory.Load(_fileName, out var bones))
            {
                PrefabCollection.DecodeModelActive(new BodyStructWrapper(bones.gender, bones.family));
                Debug.Log($"加载 {_fileName} 成功");
            }
            else
            {
                Debug.Log($"找不到 {_fileName} 的数据");
            }
        }
    }
}
