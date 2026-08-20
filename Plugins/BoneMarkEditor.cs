using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Plugins.models;
using Plugins.orm.Models;
using Plugins.orm.Servers;


namespace Plugins
{
    public class BoneMarkEditor : MonoBehaviour, ICanvasEditor
    {
        public int boneOrderNum;
        public string currentMarkName;

        // 数据
        private Bonemarks _bonemark; // 当前点击的骨标
        private BonemarksServer _bones; // 缓存的骨标集合
        private int _markIndex;

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
        public int ModelGender => _bones.Gender;
        public string[] ModelDisplayed => _bones.Family.Select(static e => e.ToString()).ToArray();
        public string[] ForamensDisplayed => _bones.GenerateBonemarkForamens().Select(e => e.ToString()).ToArray();

        private static readonly HashSet<string> ValidRoots = new()
        {
            "BodyMaleStatic(Clone)", "BodyFemaleStatic(Clone)", "ForamensMale(Clone)", "ForamensFemale(Clone)"
        };

        private void Awake()
        {
            // 初始化数据
            _bones = new BonemarksServer(0);
            if (_bones.TryGenerateBonemarkView(boneOrderNum) || _bones.TryGenerateBonemarkView(0))
            {
                boneOrderNum = _bones.OrderNum;
            }
            else
            {
                Debug.Log("骨骼视图数据获取失败");
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
                    // 顶部按钮与信息框
                    case "LastBoneFamilyBtn":
                        userInterface.GetComponent<Button>().onClick.AddListener(
                            () => SwitchBoneFamily(false)
                        );
                        break;
                    case "NextBoneFamilyBtn":
                        userInterface.GetComponent<Button>().onClick.AddListener(
                            () => SwitchBoneFamily(true)
                        );
                        break;
                    case "TipsInfo":
                        _tipsText = userInterface.GetComponent<Text>();
                        break;

                    // 底部响应与历史数据信息框
                    case "ClickedInfo":
                        _cilckedText = userInterface.GetComponent<Text>();
                        break;
                    case "HistoryInfo":
                        _historyText = userInterface.GetComponent<Text>();
                        break;

                    // 底部操作面板
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
                    default:
                        Debug.LogWarning($"Unrecognized UI object: {userInterface.name}");
                        break;
                }
            }

            // 界面显示初始化
            _markIndex = -1;
            InfomationDisplay("点击任意骨性标志生成数据");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                AddBonemark();
            }
            else if (Input.GetKeyDown(KeyCode.PageUp))
            {
                SwitchMarkIndex(false);
            }
            else if (Input.GetKeyDown(KeyCode.PageDown))
            {
                SwitchMarkIndex(true);
            }
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
                InfomationDisplay("获取骨性标志点位失败");
                return;
            }

            InfomationDisplay();
        }

        private void SwitchBoneFamily(bool advanceOrBack)
        {
            if (_bones.TryGenerateBonemarkView(_bones.OrderNum + (advanceOrBack ? 1 : -1)))
            {
                _boneMarkManager.BoneFamilyChanged();
                _camCtrl.ResetZoomState();
                _markIndex = -1;
                _bonemark = null;
                currentMarkName = "";
                boneOrderNum = _bones.OrderNum;
                InfomationDisplay($"第 {_bones.OrderNum + 1} 个骨骼族群已加载");
                Debug.Log($"加载模型 {string.Join(",", _bones.Family)}");
            }
            else
            {
                InfomationDisplay($"{(advanceOrBack ? "下" : "上")}一个骨骼族群加载失败");
            }
        }

        private void SwitchBonemarkMode()
        {
            _boneMarkManager.SwitchBonemarkMode();
            _boneMarkManager.SetBonemark();
        }

        private void LoadingMarksData()
        {
            if (_boneMarkManager.MarkType == 0) return;

            _bones.FindAllBonemarks(_boneMarkManager.MarkType);
            _markIndex = -1;
            InfomationDisplay("加载数据库成功");
        }

        private void SwitchMarkIndex(bool advanceOrBack)
        {
            _markIndex += advanceOrBack ? 1 : -1;
            var len = _bones.BonemarksList.Count;

            if (_markIndex >= len)
            {
                _markIndex = -1;
            }
            else if (_markIndex < -1)
            {
                _markIndex += len + 1;
            }

            if (HasHistory)
            {
                currentMarkName = _bones.BonemarksList[_markIndex].Name;
                InfomationDisplay();
            }
            else
            {
                currentMarkName = "";
                InfomationDisplay("当前无标签数据");
            }
        }

        private void AddBonemark()
        {
            if (HasClicked)
            {
                _bonemark.Name = currentMarkName;
                _bonemark.Position = _camCtrl.GetMainCameraPostion();
                _bonemark.Rotation = _camCtrl.GetMainCameraRotation();

                if (_bones.SaveUpdateMark(_bonemark, ref _markIndex))
                {
                    _bonemark = null;
                    InfomationDisplay("当前点位已添加");
                }
            }
            else if (HasHistory)
            {
                _bones.BonemarksList[_markIndex].Name = currentMarkName;
                InfomationDisplay("当前记录点的名称已更新");
            }
            else
            {
                InfomationDisplay("无法执行添加");
            }
        }

        private void DelBonemark()
        {
            if (HasHistory)
            {
                _bones.DeleteMark(_markIndex);
                InfomationDisplay("该条记录已删除");
                if (_markIndex > 0)
                {
                    _markIndex--;
                }
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
                var mark = _bones.BonemarksList[_markIndex];
                _camCtrl.SetCameraTransform(mark.Position, mark.Rotation);
            }
            else
            {
                _boneMarkManager.InitCameraTransform(new BodyStruct(_bones.Family[0]));
            }
        }

        private void SaveBonemarks()
        {
            _bones.SaveAllBonemarks();
            _bonemark = null;
            _markIndex = -1;
            InfomationDisplay("所有标志已保存");
        }

        private bool MatchNewOld()
        {
            if (!HasClicked || !HasHistory)
                return false;

            var oldBonemark = _bones.BonemarksList[_markIndex];
            if (oldBonemark.BePainting && _bonemark.BePainting)
            {
                if (
                    oldBonemark.Type == _bonemark.Type
                    && oldBonemark.Value == _bonemark.Value
                    && BoneMarkManager.IsSameColor(oldBonemark.Color, _bonemark.Color)
                )
                {
                    return true;
                }
            }
            else if (oldBonemark.BeForamen && _bonemark.BeForamen)
            {
                if (oldBonemark.Type == _bonemark.Type && oldBonemark.PlaneValue == _bonemark.PlaneValue)
                {
                    return true;
                }
            }

            return false;
        }

        private void InfomationDisplay(string tips = "")
        {
            _tipsText.text = tips;

            if (HasClicked)
            {
                _cilckedText.text = string.Join("\n",
                    $"Type: {_bonemark.Type}",
                    $"Value: {_bonemark.Value}",
                    $"Name: {_bonemark.Name}",
                    $"Color: {_bonemark.Color}",
                    $"Uv: {_bonemark.Uvx}, {_bonemark.Uvy}",
                    $"Plane Value: {_bonemark.PlaneValue}"
                );
                _historyText.color = MatchNewOld()
                    ? new Color(0.37831f, 0.64899f, 0.51972f, 1.0f)
                    : Color.white;
            }
            else
            {
                _cilckedText.text = "<无目标点>";
            }

            if (HasHistory)
            {
                var mark = _bones.BonemarksList[_markIndex];
                _historyText.text = string.Join("\n",
                    $"Id: {mark.Id}",
                    $"Type: {mark.Type}",
                    $"Value: {mark.Value}",
                    $"Name: {mark.Name}",
                    $"Color: {mark.Color}",
                    $"Uv: {mark.Uvx}, {mark.Uvy}",
                    $"Plane Value: {mark.PlaneValue}",
                    $"Position: {mark.CameraPositionX}, {mark.CameraPositionY}, {mark.CameraPositionZ}",
                    $"Rotation: {mark.CameraRotationX}, {mark.CameraRotationY}, {mark.CameraRotationZ}"
                );
                _historyText.color = _boneMarkManager.ColorCodeVerification(mark)
                    ? new Color(0.6039216f, 0.8117647f, 1.0f, 1.0f)
                    : Color.white;
            }
            else
            {
                _historyText.text = "<新建>";
            }
        }
    }
}
