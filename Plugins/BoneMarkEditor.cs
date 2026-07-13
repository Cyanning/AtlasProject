using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JetBrains.Annotations;
using Plugins.models;

namespace Plugins
{
    public class BoneMarkEditor : MonoBehaviour, ICanvasEditor
    {
        public string fileName;

        public Bone bone; // 缓存的骨标
        [CanBeNull] private Bonemark _bonemark; // 上一个点击的骨标

        private Text _activeInfo; // 信息框文字
        private MainCameraContraller _camCtrl; // 封装的相机对象
        private BoneMarkManager _boneMarkManager; // 骨性标志控制脚本

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
            if (!BoneFactory.Load(fileName, out bone))
            {
                Debug.LogWarning("Bone Not Found!");
            }
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

        public string[] ModelDisplayed => bone.family;
        public int ModelGender => bone.gender;

        // 创建一个新骨性标注缓存
        public void ClickRespond(Transform clickedModel)
        {
            if (!ValidRoots.Contains(clickedModel.root.name))
                return;

            if (
                _boneMarkManager.markType != 0 &&
                _camCtrl.GetTextureUv(out var uv) &&
                _boneMarkManager.FindBonemarkData(clickedModel, uv, out _bonemark)
            )
                UpdateActiveInfo("新建：\n");
            UpdateActiveInfo();
        }

        private void AddBonemarks()
        {
            if (_bonemark is null) return;

            var pos = _camCtrl.GetMainCameraPostion();
            var rot = _camCtrl.GetMainCameraRotation();
            _bonemark.cameraPositionX = pos.x;
            _bonemark.cameraPositionY = pos.y;
            _bonemark.cameraPositionZ = pos.z;
            _bonemark.cameraRotationX = rot.x;
            _bonemark.cameraRotationY = rot.y;
            _bonemark.cameraRotationZ = rot.z;
            bone.bonemarks.Add(_bonemark);

            UpdateActiveInfo("已添加：\n");
        }

        // 视角复位
        private void ResetAtlasCarmera()
        {
            if (bone.bonemarks.Count > 0)
            {
                var mark = bone.bonemarks[^1];
                _camCtrl.SetCameraTransform(
                    mark.cameraPositionX,
                    mark.cameraPositionY,
                    mark.cameraPositionZ,
                    mark.cameraRotationX,
                    mark.cameraRotationY,
                    mark.cameraRotationZ
                );
            }
            else
            {
                _camCtrl.ResetTransform();
            }
        }

        private void SaveBonemarks()
        {
            BoneFactory.Save(bone, fileName);
            _bonemark = null;
            UpdateActiveInfo("所有标志已保存");
        }

        private void UpdateActiveInfo(string tipsInfo = "")
        {
            if (_bonemark is null)
            {
                _activeInfo.text = string.IsNullOrEmpty(tipsInfo) ? "点击任意骨性标志生成数据" : tipsInfo;
            }
            else if (string.IsNullOrEmpty(_bonemark.color))
            {
                _activeInfo.text =
                    tipsInfo +
                    $"Name: {_bonemark.name}\n" +
                    $"Plane Value: {_bonemark.planeValue}";
            }
            else
            {
                _activeInfo.text =
                    tipsInfo +
                    $"Value: {_bonemark.value}\n" +
                    $"Name: {_bonemark.name}\n" +
                    $"Color: {_bonemark.color}\n" +
                    $"Uv: {_bonemark.uvx}, {_bonemark.uvy}";
            }
        }
    }
}
