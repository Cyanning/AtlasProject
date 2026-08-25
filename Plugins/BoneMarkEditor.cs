using System.Collections.Generic;
using System.IO;
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
        private const string OrderNumTempPath = @"D:\AnatomyLibrary\Bonemarks\BoneMarkEditorTemp\cogfigInit.json";

        public string currentMarkName;
        public bool ignoreRepeating;

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
            var initData = ReadBoneOrderNum();
            _bones = new BonemarksServer(initData.gender, initData.orderNum);
        }

        private void Start()
        {
            // 绑定主相机脚本
            _camCtrl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<MainCameraContraller>();

            //按钮绑定事件
            for (var i = 0; i < transform.childCount; i++)
            {
                var userInterface = transform.GetChild(i);
                switch (userInterface.name)
                {
                    // 顶部按钮与信息框
                    case "LastBoneFamilyBtn":
                        userInterface.GetComponent<Button>().onClick.AddListener(
                            () => SwitchBoneFamily(-1)
                        );
                        break;
                    case "NextBoneFamilyBtn":
                        userInterface.GetComponent<Button>().onClick.AddListener(
                            () => SwitchBoneFamily(1)
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
                            () => SwitchMarkIndex(-1)
                        );
                        break;
                    case "NextMarkDataBtn":
                        userInterface.GetComponent<Button>().onClick.AddListener(
                            () => SwitchMarkIndex(1)
                        );
                        break;
                    case "LoadMarksDataBtn":
                        userInterface.GetComponent<Button>().onClick.AddListener(LoadingMarksData);
                        break;
                    case "ChangeMapsBtn":
                        userInterface.GetComponent<Button>().onClick.AddListener(SwitchToIdentfier);
                        break;
                    case "ChangeBonesBtn":
                        userInterface.GetComponent<Button>().onClick.AddListener(() => SwitchBonemarkMode());
                        break;
                    case "ResetCarmeraBtn":
                        userInterface.GetComponent<Button>().onClick.AddListener(ResetToMarkCarmera);
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

            // 骨性标志显示初始化
            _boneMarkManager = gameObject.AddComponent<BoneMarkManager>();
            _markIndex = -1;
            boneOrderNum = _bones.OrderNum;
            InfomationDisplay("点击任意骨性标志生成数据");
        }

        private void LateUpdate()
        {
            if (Input.GetKeyUp(KeyCode.Q))
            {
                SwitchMarkIndex(-1);
                ResetToMarkCarmera();
            }
            else if (Input.GetKeyUp(KeyCode.E))
            {
                SwitchMarkIndex(1);
                ResetToMarkCarmera();
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                AddBonemark();
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                SaveBonemarks();
            }
            else if (Input.GetKeyUp(KeyCode.N))
            {
                SwitchMarkIndex(_bones.BonemarksList.Count);
            }
            else if (Input.GetKeyDown(KeyCode.M) || Input.GetKeyUp(KeyCode.M))
            {
                SwitchToIdentfier();
            }
            else
            {
                for (var i = 0; i < 5; i++)
                {
                    // KeyCode.Alpha0 到 Alpha9 枚举值是连续的
                    var key = KeyCode.Alpha0 + i;
                    if (Input.GetKeyUp(key))
                    {
                        // 直接加载骨性标志和数据
                        SwitchBonemarkMode(i);
                        LoadingMarksData();
                    }
                }
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

        private void SwitchBoneFamily(int vector)
        {
            int nextIndex;
            // 编号变量大于输入的变化值
            if (boneOrderNum != _bones.OrderNum)
            {
                nextIndex = boneOrderNum;
            }
            else
            {
                nextIndex = _bones.OrderNum + vector;
            }

            if (_bones.TryGenerateBonemarkView(nextIndex))
            {
                // 数据初始化
                _boneMarkManager.BoneFamilyChanged();
                _camCtrl.ResetZoomState();
                _markIndex = -1;
                _bonemark = null;
                currentMarkName = "";
                boneOrderNum = _bones.OrderNum;

                //状态输出
                SaveBoneOrderNum();
                InfomationDisplay($"第 {_bones.OrderNum + 1} 个骨骼族群已加载");
                Debug.Log($"加载模型 {string.Join(",", _bones.Family)}");
            }
            else
            {
                InfomationDisplay("骨骼族群加载失败");
            }
        }

        /// <summary>
        /// 切换id贴图或是外观贴图
        /// </summary>
        private void SwitchToIdentfier()
        {
            _boneMarkManager.DisplayIdentifiedTexture();
            InfomationDisplay("切换贴图模式成功");
        }

        /// <summary>
        /// 切换骨骼标志类型
        /// </summary>
        private void SwitchBonemarkMode(int markType = -1)
        {
            _boneMarkManager.SwitchBonemarkMode(markType);
            _bones.ClearBonemarksCache();
            _markIndex = -1;
            _bonemark = null;
            InfomationDisplay($"切换到标志类型: {_boneMarkManager.MarkType}");
        }

        private void LoadingMarksData()
        {
            if (_boneMarkManager.MarkType == 0) return;

            _bones.FindAllBonemarks(_boneMarkManager.MarkType);
            _markIndex = -1;
            InfomationDisplay("加载数据库成功");
        }

        /// <summary>
        /// 切换当前标志
        /// </summary>
        /// <param name="vector">相对当前标志变换的index数值</param>
        private void SwitchMarkIndex(int vector)
        {
            _markIndex += vector;
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
                    InfomationDisplay("当前目标点位已添加");
                }
                else
                {
                    InfomationDisplay("当前目标点位添加失败");
                }
            }
            else if (HasHistory)
            {
                _bones.BonemarksList[_markIndex].Name = currentMarkName;
                InfomationDisplay("当前记录点的名称已更新");
            }
            else
            {
                InfomationDisplay("无效的添加");
            }
        }

        private void DelBonemark()
        {
            if (HasHistory)
            {
                _bones.DeleteMark(_markIndex);
                if (_markIndex > 0)
                {
                    _markIndex--;
                }
                InfomationDisplay("该条记录已删除");
            }
            else
            {
                InfomationDisplay("未指定删除条目");
            }
        }

        // 视角复位
        private void ResetToMarkCarmera()
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
            var info = _bones.SaveAllBonemarks(ignoreRepeating);
            // 每次使用后重置 防止错误保存
            ignoreRepeating = false;
            _bonemark = null;

            InfomationDisplay("已保存" + (string.IsNullOrEmpty(info) ? "" : $" - 警告: {info}"));
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
                _cilckedText.color = MatchNewOld()
                    ? new Color(0.45246f, 0.74343f, 0.2714f, 1.0f)
                    : Color.white;

                var text = $"Type: {_bonemark.Type}" +
                           $"\nValue: {_bonemark.Value}";

                if (_bonemark.BePainting)
                {
                    text += $"\nColor: {_bonemark.Color}" +
                            $"\nUv: {_bonemark.Uvx}, {_bonemark.Uvy}";
                }
                else if (_bonemark.BeForamen)
                {
                    text += $"\nPlane Value: {_bonemark.PlaneValue}";
                }

                _cilckedText.text = text;
            }
            else
            {
                _cilckedText.color = Color.white;
                _cilckedText.text = "<无目标点>";
            }

            if (HasHistory)
            {
                var mark = _bones.BonemarksList[_markIndex];
                _historyText.color = _boneMarkManager.ColorCodeVerification(mark)
                    ? new Color(0.6039216f, 0.8117647f, 1.0f, 1.0f)
                    : Color.white;

                var text = $"Id: {mark.Id}, Type: {mark.Type}" +
                           $"\nValue: {mark.Value}, Name: {mark.Name}";

                if (mark.BePainting)
                {
                    text += $"\nColor: {mark.Color}" +
                            $"\nUv: {mark.Uvx}, {mark.Uvy}";
                }
                else if (mark.BeForamen)
                {
                    text += $"\nPlane Value: {mark.PlaneValue}";
                }

                text += $"\nPosition: {mark.CameraPositionX}, {mark.CameraPositionY}, {mark.CameraPositionZ}" +
                        $"\nRotation: {mark.CameraRotationX}, {mark.CameraRotationY}, {mark.CameraRotationZ}";

                _historyText.text = text;
            }
            else
            {
                _historyText.color = Color.white;
                _historyText.text = "<新建>";
            }
        }

        private void SaveBoneOrderNum()
        {
            if (!File.Exists(OrderNumTempPath))
            {
                var folder = Path.GetDirectoryName(OrderNumTempPath);

                if (!string.IsNullOrEmpty(folder))
                    Directory.CreateDirectory(folder);
                else
                    return;
            }

            File.WriteAllText(OrderNumTempPath, $"{_bones.Gender};{boneOrderNum}");
        }

        private static (int gender, int orderNum) ReadBoneOrderNum()
        {
            var genedr = 0;
            var orderNum = 0;

            if (File.Exists(OrderNumTempPath))
            {
                var text = File.ReadAllText(OrderNumTempPath).Split(";");
                if (text.Length == 2)
                {
                    genedr = int.Parse(text[0]);
                    orderNum = int.Parse(text[1]);
                }
            }

            return (genedr, orderNum);
        }
    }
}
