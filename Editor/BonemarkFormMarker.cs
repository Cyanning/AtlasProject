using System;
using UnityEditor;
using UnityEngine;
using Plugins.C_.models;
using Unity.VisualScripting;

namespace Editor
{
    public class BonemarkFormMarker : EditorWindow
    {
        // 表单数据
        private string _boneFileName;
        private Bone _bone;

        //MenuItem会在unity菜单栏添加自定义新项
        [MenuItem("自定义功能/骨骼-创建单独视图")]
        public static void ShowWindow()
        {
            GetWindow<BonemarkFormMarker>("创建骨骼单独视图");
        }

        private void OnEnable()
        {
            _bone = new Bone();
            // 获取当前显示的模型数据
            var bodysForActive = PrefabEditor.EncodetModelActive();
            _bone.gender = bodysForActive.gender;
            _bone.family = bodysForActive.ValuesAsStr();

            _boneFileName =
                CommonParentFinder.Find(_bone.family, out var commonParent)
                    ? commonParent.name
                    : "";
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("骨骼名称（可修改）：", EditorStyles.boldLabel);
            _boneFileName = EditorGUILayout.TextField(_boneFileName);

            EditorGUILayout.Space(20);
            if (GUILayout.Button("保存骨骼"))
            {
                BoneSaved();
                Close();
            }
        }

        private void BoneSaved()
        {
            //保存文件
            BoneFactory.Save(_bone, _boneFileName);
            Debug.Log($"新的骨骼 [ {string.Join(",", _bone.family)} ] 数据已建立");
        }
    }
}
