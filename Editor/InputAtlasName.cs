using System.Collections.Generic;
using Plugins.C_.models;
using UnityEditor;
using UnityEngine;
using System.IO;


namespace Editor
{
    public class AtlasAttrsInput : EditorWindow
    {
        private AtlasItem _atlas;
        private OptionalConfig _optionalConfig;
        private int _boneMarkIndex;
        private bool[] _systemClassIndexes;
        private bool[] _regionClassIndexes;

        //MenuItem会在unity菜单栏添加自定义新项
        [MenuItem("脚本/设置图谱展示的模型")]
        public static void ShowInputWindowMale()
        {
            GetWindow<AtlasAttrsInput>("图谱名称");
        }

        private void OnEnable()
        {
            _atlas = GetShowModelOnDisplay.GetValueDisplayed();
            _atlas.name = "";

            var configPath = Path.Combine(Application.dataPath, "Editor/OptionalConfig.json");
            if (File.Exists(configPath))
            {
                _optionalConfig = JsonUtility.FromJson<OptionalConfig>(File.ReadAllText(configPath));
                _boneMarkIndex = 0;
                _systemClassIndexes = new bool[_optionalConfig.atlasTypes.systemClasses.Length];
                _regionClassIndexes = new bool[_optionalConfig.atlasTypes.regionClasses.Length];
            }
        }

        private void OnGUI()
        {
            GUILayout.Label("请输入图谱名称：", EditorStyles.boldLabel);
            _atlas.name = EditorGUILayout.TextField("图谱名称", _atlas.name);

            GUILayout.Label("选择骨性标志类型：", EditorStyles.boldLabel);
            _boneMarkIndex = GUILayout.SelectionGrid(
                _boneMarkIndex, _optionalConfig.boneMarkClasses, 2, EditorStyles.toggle
            );
            ToggleGrid("系统分类", _systemClassIndexes, _optionalConfig.atlasTypes.systemClasses);
            ToggleGrid("区域分类", _regionClassIndexes, _optionalConfig.atlasTypes.regionClasses);

            // GUILayout.Label("选择基于系统的分类：", EditorStyles.boldLabel);
            // _systemClassIndex = GUILayout.SelectionGrid(
            //     _systemClassIndex, _radioButtons.atlasClassesBySystem, 2, EditorStyles.toggle
            // );

            // GUILayout.Label("选择基于区域的分类：", EditorStyles.boldLabel);
            // _regionClassIndex = GUILayout.SelectionGrid(
            //     _regionClassIndex, _radioButtons.atlasClassesByRegion, 2, EditorStyles.toggle
            // );

            if (GUILayout.Button("确定"))
            {
                var context1 = _optionalConfig.boneMarkClasses[_boneMarkIndex];
                var context2 = string.Join(",", GetSelectedIndexes(_systemClassIndexes));
                var context3 = string.Join(",", GetSelectedIndexes(_regionClassIndexes));
                Debug.Log($"当前选择的是: {context1}; {context2}; {context3}");
            }
            // if (GUILayout.Button("确定") && _atlas.name.Length > 0)
            // {
            //     GetShowModelOnDisplay.SaveValueDisplayed(_atlas);
            //     Close();
            // }
        }

        private static void ToggleGrid(string name, IList<bool> selected, IReadOnlyList<string> options)
        {
            GUILayout.Label($"选择基于{name}的分类：", EditorStyles.boldLabel);
            // 开启水平布局，作为外层容器
            EditorGUILayout.BeginHorizontal();
            {
                // 左侧布局
                EditorGUILayout.BeginVertical();
                for (var i = 0; i < options.Count; i += 2)
                {
                    selected[i] = EditorGUILayout.ToggleLeft(options[i], selected[i]);
                }

                EditorGUILayout.EndVertical();

                // 右侧布局
                EditorGUILayout.BeginVertical();
                for (var i = 1; i < options.Count; i += 2)
                {
                    selected[i] = EditorGUILayout.ToggleLeft(options[i], selected[i]);
                }

                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }

        private static IEnumerable<int> GetSelectedIndexes(IReadOnlyList<bool> selected)
        {
            for (var i = 0; i < selected.Count; i++)
            {
                if (selected[i]) yield return i;
            }
        }
    }
}
