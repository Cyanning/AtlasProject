using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JetBrains.Annotations;
using Plugins.C_.models;

namespace Plugins.C_
{
    public class BoneMarkEditor : MonoBehaviour, ICanvasEditor
    {
        public string atlasFile;
        public List<Row> labelsMatrix;

        public List<Bonemark> bonemarks; // 缓存的骨标
        [CanBeNull] private Bonemark _bonemark; // 上一个点击的骨标

        private Text _activeInfo;
        private MainCameraContraller _camCtrl;

        private BoneMarkManager _boneMarkManager;  // 骨性标志控制脚本

        private static readonly HashSet<string> ValidRoots = new()
        {
            "BodyMaleStatic(Clone)",
            "BodyFemaleStatic(Clone)",
            "ForamensMale(Clone)",
            "ForamensFemale(Clone)"
        };

        private void Awake()
        {
            // 初始化数据
            bonemarks = new List<Bonemark>();
        }

        private void Start()
        {
            // 绑定主相机脚本
            _camCtrl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<MainCameraContraller>();
            _boneMarkManager = gameObject.AddComponent<BoneMarkManager>();

            //按钮绑定事件
            for (var i = 0; i < transform.childCount; i++)
            {
                var userInterface = transform.GetChild(i);
                switch (userInterface.name)
                {
                    case "ChangeBonesBtn":
                        userInterface.GetComponent<Button>().onClick.AddListener(_boneMarkManager.SettingBonemarkMode);
                        break;
                    case "ResetCarmeraBtn":
                        userInterface.GetComponent<Button>().onClick.AddListener(ResetAtlasCarmera);
                        break;
                    case "AddBonemarkBtn":
                        userInterface.GetComponent<Button>().onClick.AddListener(AddBonemarks);
                        break;
                    case "SaveAllBtn":
                        userInterface.GetComponent<Button>().onClick.AddListener(SaveBonemarks);
                        break;
                    case "ActiveInfo":
                        _activeInfo = userInterface.GetComponent<Text>();
                        break;
                }
            }

            // 界面显示初始化
            UpdateActiveInfo();
        }

        // 创建一个骨性标注缓存
        public void ClickRespond(Transform clickedModel)
        {
            if (!ValidRoots.Contains(clickedModel.root.name))
                return;

            if (_boneMarkManager.markType != 0 && _camCtrl.GetTextureUv(out var uv))
            {
                _bonemark = _boneMarkManager.FindBonemarkData(clickedModel, uv);
            }
        }

        private void AddBonemarks()
        {
            bonemarks.Add(_bonemark);
        }

        // 视角复位
        private void ResetAtlasCarmera()
        {
            _camCtrl.SetCameraTransform(0,0,0,0,0,0);
        }

        private void SaveBonemarks()
        {

        }

        private void UpdateActiveInfo(string tipsInfo = null, bool isAdd = false)
        {

        }

    }
}
