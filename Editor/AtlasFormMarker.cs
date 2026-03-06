using UnityEditor;
using UnityEngine;
using System.Linq;
using Plugins.C_.models;


namespace Editor
{
    public class AtlasFormMarker : EditorWindow
    {
        private AtlasItem _atlas;
        private TypeItem[] _boneMarkTypes;
        private TypeItem[] _atlasTypes;
        private bool[] _atlasTypeFlags;

        //MenuItem会在unity菜单栏添加自定义新项
        [MenuItem("自定义功能/创建图谱")]
        public static void ShowWindow()
        {
            GetWindow<AtlasFormMarker>("创建图谱");
        }

        private void OnEnable()
        {
            _atlas = new AtlasItem();

            if (OptionalConfig.ReadConfig(out var optionalConfig))
            {
                _atlas.boneMarkType = 0;
                _boneMarkTypes = optionalConfig.boneMarkTypes;
                _atlasTypes = optionalConfig.atlasTypes;
                _atlasTypeFlags = new bool[_atlasTypes.Length];
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("图谱名称（必填）：", EditorStyles.boldLabel);
            _atlas.name = EditorGUILayout.TextField(_atlas.name);

            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("选择骨性标志类型（必填）：", EditorStyles.boldLabel);
            _atlas.boneMarkType = GUILayout.SelectionGrid(
                _atlas.boneMarkType,
                OptionalConfig.GetContext(_boneMarkTypes),
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
            if (GUILayout.Button("保存图谱") && _atlas.name.Length > 0)
            {
                AtlasSaved();
                Close();
            }
        }

        private void AtlasSaved()
        {
            _atlas.types = OptionalConfig.GetMultiSelected(_atlasTypes, _atlasTypeFlags).ToList();

            // 若已存在数据 则仅替换本次数据
            if (AtlasFactory.Load(_atlas.name, out var atlas))
            {
                atlas.boneMarkType = _atlas.boneMarkType;
                atlas.types = _atlas.types;
            }
            else
            {
                atlas = _atlas;
            }

            // 添加模型数据
            atlas = PrefabData.EncodetModelVisible(atlas);

            if (AtlasFactory.Save(atlas))
            {
                Debug.Log($"新的图谱 {_atlas.name} 数据已建立");
            }
        }
    }
}
