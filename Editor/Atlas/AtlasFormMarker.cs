using System;
using UnityEditor;
using UnityEngine;
using Plugins.C_.models;

namespace Editor.Atlas
{
    public class AtlasFormMarker : EditorWindow
    {
        // 表单数据
        private string _atlasFile;
        private int _boneMarkType;
        private bool[] _atlasTypeFlags;

        // 选项
        private TypeItem[] _boneMarkTypes;
        private TypeItem[] _atlasTypes;

        //MenuItem会在unity菜单栏添加自定义新项
        [MenuItem("自定义功能/图谱-新建")]
        public static void ShowWindow()
        {
            GetWindow<AtlasFormMarker>("创建图谱");
        }

        private void OnEnable()
        {
            if (TypesConfig.ReadConfig(out var optionalConfig))
            {
                _boneMarkTypes = optionalConfig.boneMarkTypes;
                _atlasTypes = optionalConfig.atlasTypes;
            }
            else
            {
                _boneMarkTypes = Array.Empty<TypeItem>();
                _atlasTypes = Array.Empty<TypeItem>();
            }

            _atlasFile = CookieWrapper.Reading();

            if (AtlasFactory.Load(_atlasFile, out var atlas))
            {
                _boneMarkType = atlas.boneMarkType;
                _atlasTypeFlags = TypesConfig.FlagsSelected(_atlasTypes, atlas.types);
            }
            else
            {
                _boneMarkType = 0;
                _atlasTypeFlags = new bool[_atlasTypes.Length];
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("图谱名称（必填）：", EditorStyles.boldLabel);
            _atlasFile = EditorGUILayout.TextField(_atlasFile);

            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("选择骨性标志类型（必填）：", EditorStyles.boldLabel);
            _boneMarkType = GUILayout.SelectionGrid(
                _boneMarkType,
                TypesConfig.GetContext(_boneMarkTypes),
                2, EditorStyles.toggle
            );

            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("选择图谱的分类（选填、多选）：", EditorStyles.boldLabel);
            // 开启水平布局，作为外层容器
            EditorGUILayout.BeginHorizontal();
            {
                // 左侧布局
                EditorGUILayout.BeginVertical();
                for (var i = 0; i < _atlasTypes.Length; i += 2)
                {
                    _atlasTypeFlags[i] = EditorGUILayout.ToggleLeft(_atlasTypes[i].tittle, _atlasTypeFlags[i]);
                }

                EditorGUILayout.EndVertical();

                // 右侧布局
                EditorGUILayout.BeginVertical();
                for (var i = 1; i < _atlasTypes.Length; i += 2)
                {
                    _atlasTypeFlags[i] = EditorGUILayout.ToggleLeft(_atlasTypes[i].tittle, _atlasTypeFlags[i]);
                }

                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(20);
            if (GUILayout.Button("保存图谱") && !string.IsNullOrEmpty(_atlasFile))
            {
                AtlasSaved();
                Close();
            }
        }

        private void AtlasSaved()
        {
            // 若已存在数据 则仅替换本次数据
            if (!AtlasFactory.Load(_atlasFile, out var atlas))
            {
                atlas = new AtlasItem();
            }

            // 获取当前显示的模型数据
            var bodysForActive = PrefabCollection.EncodetModelActive();
            atlas.gender = bodysForActive.gender;
            atlas.modelDisplayed = bodysForActive.ValuesAsStr();

            // 获取当前透明的模型数据
            var bodysForTranslucent = PrefabCollection.EncodetModelTranslucent(atlas.gender);
            atlas.modelTranslucent = bodysForTranslucent.ValuesAsStr();

            // 获取骨性标志的类型和预分类
            atlas.boneMarkType = _boneMarkType;
            atlas.types = TypesConfig.IdNumsSelected(_atlasTypes, _atlasTypeFlags);

            //保存文件
            AtlasFactory.Save(atlas, _atlasFile);
            CookieWrapper.Create(_atlasFile);
            Debug.Log($"新的图谱 {atlas.name} 数据已建立");
        }
    }
}
