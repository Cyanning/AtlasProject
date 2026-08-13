using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Plugins.models;


namespace Plugins
{
    public class BoneMarkEditor : MonoBehaviour, ICanvasEditor
    {
        public string fileName;

        // 数据
        private Bonemark _bonemark; // 当前点击的骨标
        private Bones _bones; // 缓存的骨标集合
        private int _markIndex;
        private AnatomyDatabase _data;

        private bool HasHistory => _markIndex > -1;
        private bool HasClicked => _bonemark != null;

        // 信息框文字
        private Text _cilckedText;
        private Text _historyText;
        private Text _tipsText;

        // 封装的相机对象
        private MainCameraContraller _camCtrl;
        // 骨性标志控制脚本
        private BoneMarkManager _boneMarkManager;

        // 接口
        public string[] ModelDisplayed => _bones.family.Select(val => val.ToString()).ToArray();
        public int ModelGender => _bones.gender;

        private static readonly HashSet<string> ValidRoots = new()
        {
            "BodyMaleStatic(Clone)", "BodyFemaleStatic(Clone)", "ForamensMale(Clone)", "ForamensFemale(Clone)"
        };

        private void Awake()
        {
            // 初始化数据
            if (!BoneFactory.Load(fileName, out _bones))
            {
                Debug.LogError("Bone File Not Found!");
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
                    case "LastMarkDataBtn":
                        userInterface.GetComponent<Button>().onClick.AddListener(
                            () => SwitchMarkIndex(false)
                        );
                        break;
                    case "NextMarkDataBtn":
                        userInterface.GetComponent<Button>().onClick.AddListener(
                            () => SwitchMarkIndex(true)
                        );
                        break;
                    case "LoadMarksDataBtn":
                        userInterface.GetComponent<Button>().onClick.AddListener(LoadingMarksData);
                        break;
                    case "ChangeBonesBtn":
                        userInterface.GetComponent<Button>().onClick.AddListener(SwitchBonemarkMode);
                        break;
                    case "ResetCarmeraBtn":
                        userInterface.GetComponent<Button>().onClick.AddListener(ResetAtlasCarmera);
                        break;
                    case "AddBonemarkBtn":
                        userInterface.GetComponent<Button>().onClick.AddListener(AddBonemark);
                        break;
                    case "DelBonemarkBtn":
                        userInterface.GetComponent<Button>().onClick.AddListener(DelBonemark);
                        break;
                    case "SaveAllBtn":
                        userInterface.GetComponent<Button>().onClick.AddListener(SaveBonemarks);
                        break;
                    case "ClickedInfo":
                        _cilckedText = userInterface.GetComponent<Text>();
                        break;
                    case "HistoryInfo":
                        _historyText = userInterface.GetComponent<Text>();
                        break;
                    case "TipsInfo":
                        _tipsText = userInterface.GetComponent<Text>();
                        break;
                    default:
                        Debug.LogWarning($"Unrecognized UI object: {userInterface.name}");
                        break;
                }
            }

            // 界面显示初始化
            _data = new AnatomyDatabase();
            _markIndex = -1;
            InfomationDisplay("点击任意骨性标志生成数据");
        }

        // 激活一个新骨性标注点
        public void ClickRespond(Transform clickedModel)
        {
            if (_boneMarkManager.MarkType == 0)
            {
                return;
            }

            if (!ValidRoots.Contains(clickedModel.root.name))
            {
                InfomationDisplay("点击对象错误");
                return;
            }

            if (!_camCtrl.GetTextureUv(out var uv))
            {
                InfomationDisplay("UV 获取失败");
                return;
            }

            if (!_boneMarkManager.FindBonemarkData(clickedModel.gameObject, uv, out _bonemark))
            {
                InfomationDisplay("获取鼠标点位失败");
                return;
            }

            if (!MatchNewOld()) InfomationDisplay();
        }

        private void SwitchBonemarkMode()
        {
            _boneMarkManager.SwitchBonemarkMode();
            _boneMarkManager.SetBonemark();
        }

        private void LoadingMarksData()
        {
            if (_boneMarkManager.MarkType == 0) return;

            var marks = _data.FindAllBonemarks(_bones.family, _boneMarkManager.MarkType);
            _bones.GetMarksFromOrm(marks);
            _markIndex = -1;
            InfomationDisplay("加载数据库成功");
        }

        private void SwitchMarkIndex(bool advanceOrBack)
        {
            _markIndex += advanceOrBack ? 1 : -1;
            var len = _bones.bonemarks.Count;
            if (_markIndex >= len)
            {
                _markIndex = -1;
            }
            else if (_markIndex < -1)
            {
                _markIndex += len + 1;
            }
            if (!MatchNewOld()) InfomationDisplay();
        }

        private void AddBonemark()
        {
            if (HasClicked) return;

            var pos = _camCtrl.GetMainCameraPostion();
            var rot = _camCtrl.GetMainCameraRotation();
            _bonemark.cameraPositionX = pos.x;
            _bonemark.cameraPositionY = pos.y;
            _bonemark.cameraPositionZ = pos.z;
            _bonemark.cameraRotationX = rot.x;
            _bonemark.cameraRotationY = rot.y;
            _bonemark.cameraRotationZ = rot.z;

            _markIndex = _bones.SavingMark(_bonemark, _markIndex);
            _bonemark = null;
            InfomationDisplay("当前点位已添加");
        }

        private void DelBonemark()
        {
            if (HasHistory)
            {
                _bones.bonemarks.RemoveAt(_markIndex);
                InfomationDisplay("该条记录已删除");
            }
            else
            {
                InfomationDisplay("未指定删除条目");
            }
        }

        // 视角复位
        private void ResetAtlasCarmera()
        {
            if (HasHistory)
            {
                var mark = _bones.bonemarks[_markIndex];
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
            BoneFactory.Save(_bones, fileName);
            _bonemark = null;
            _markIndex = -1;
            InfomationDisplay("所有标志已保存");
        }

        private bool MatchNewOld()
        {
            if (!HasClicked || !HasHistory)
                return false;

            var oldBonemark = _bones.bonemarks[_markIndex];
            if (oldBonemark.BePainting && _bonemark.BePainting)
            {
                if (
                    oldBonemark.type == _bonemark.type
                    && oldBonemark.value == _bonemark.value
                    && oldBonemark.color == _bonemark.color
                )
                {
                    InfomationDisplay("匹配成功", Color.green);
                    return true;
                }
            }
            else if (oldBonemark.BeForamen && _bonemark.BeForamen)
            {
                if (oldBonemark.type == _bonemark.type && oldBonemark.planeValue == _bonemark.planeValue)
                {
                    InfomationDisplay("匹配成功", Color.green);
                    return true;
                }
            }

            return false;
        }

        private void InfomationDisplay(string tips = "", Color? fcolor = null)
        {
            _cilckedText.text = HasClicked ? GetBonemarkInfo(_bonemark) : "<无目标点>";

            _historyText.text = HasHistory ? GetBonemarkInfo(_bones.bonemarks[_markIndex]) : "<新建>";

            _tipsText.color = fcolor ?? Color.white;
            _tipsText.text = tips;
        }

        private static string GetBonemarkInfo(Bonemark mark)
        {
            return string.Join("\n",
                $"Type: {mark.type}",
                $"Value: {mark.value}",
                $"Name: {mark.name}",
                $"Color: {mark.color}",
                $"Uv: {mark.uvx}, {mark.uvy}",
                $"Plane Value: {mark.planeValue}",
                $"Position: {mark.cameraPositionX}, {mark.cameraPositionY}, {mark.cameraPositionZ}",
                $"Rotation: {mark.cameraRotationX}, {mark.cameraRotationY}, {mark.cameraRotationZ}"
            );
        }
    }
}
