using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JetBrains.Annotations;
using Plugins.C_.models.Atlas;


namespace Plugins.C_
{
    public class AtlasCanvas : MonoBehaviour
    {
        public string atlasName;
        public List<Row> labelsMatrix;

        public AtlasItem atlas; // 缓存图谱对象
        private int _currentGroupIndex; // 当前打开的组的角标
        [CanBeNull] private AtlasLabel _activeLabel; // 缓存一个标签点

        private Text _activeInfo;
        private Text _atlasInfo;
        private MainCameraContraller _camCtrl;

        private static readonly HashSet<string> ValidRoots = new()
        {
            "BodyMaleStatic(Clone)",
            "BodyFemaleStatic(Clone)",
            "ForamensMale(Clone)",
            "ForamensFemale(Clone)"
        };

        private void Awake()
        {
            // 初始化图谱数据
            atlas = JsonUtility.FromJson<AtlasItem>(
                File.ReadAllText(Path.Combine(Application.dataPath, $"Atlas_database/atlas_{atlasName}.json"))
            );
            _currentGroupIndex = -1;
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
                    case "SetLeftBtn":
                        userInterface.GetComponent<Button>().onClick.AddListener(() => AddLabel(0));
                        break;
                    case "SetRightBtn":
                        userInterface.GetComponent<Button>().onClick.AddListener(() => AddLabel(1));
                        break;
                    case "ChangeGroupBtn":
                        userInterface.GetComponent<Button>().onClick.AddListener(ChangeGroup);
                        break;
                    case "CreateGroupsBtn":
                        userInterface.GetComponent<Button>().onClick.AddListener(CreateGroup);
                        break;
                    case "SaveAtlasBtn":
                        userInterface.GetComponent<Button>().onClick.AddListener(SaveAtlas);
                        break;
                    case "ResetCarmeraBtn":
                        userInterface.GetComponent<Button>().onClick.AddListener(ResetAtlasCarmera);
                        break;
                    case "SaveAtlasCarmeraBtn":
                        userInterface.GetComponent<Button>().onClick.AddListener(SaveAtlasItemCarmera);
                        break;
                    case "AtlasInfo":
                        _atlasInfo = userInterface.GetComponent<Text>();
                        break;
                    case "ActiveInfo":
                        _activeInfo = userInterface.GetComponent<Text>();
                        break;
                }
            }

            // 界面显示初始化
            LabelsConvertToLabellabelsMatrix();
            UpdateActiveInfo();
        }

        // 获取与模型交互的信息
        public void LateUpdate()
        {
            if (!_camCtrl.Clicked(out var raycast)) return;

            var clickedModel = raycast.transform;
            if (!ValidRoots.Contains(clickedModel.root.name)) return;

            var prefabNames = clickedModel.name.Split("~");
            var timestamp = new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds();
            var raycastPoint = raycast.point;
            _activeLabel = new AtlasLabel
            {
                name = prefabNames[0] + "_" + timestamp.ToString()[^4..],
                value = Convert.ToInt32(prefabNames[1]),
                pointPositionX = raycastPoint.x,
                pointPositionY = raycastPoint.y,
                pointPositionZ = raycastPoint.z,
            };
            UpdateActiveInfo();
        }

        // 设置标签位置
        private void AddLabel(int location)
        {
            // 确保有标签点位信息和启用中的标签组
            if (_activeLabel is null) return;
            if (_currentGroupIndex < 0)
            {
                UpdateActiveInfo("请先创建标签组");
                return;
            }

            // 添加标签数据
            var orderNum = GetOrderNum(location);
            labelsMatrix[orderNum][location] = _activeLabel.name;
            labelsMatrix[orderNum].SetState(location,true);
            _activeLabel.location = 1;
            _activeLabel.orderNum = orderNum;
            atlas.groups[_currentGroupIndex].labels.Add(_activeLabel);

            UpdateActiveInfo("当前标签已添加成功");
        }

        // 从上至下找出为空的标签位置, 返回是否有历史空位，附带输出序号
        private int GetOrderNum(int location)
        {
            var orderNum = 0;
            while (orderNum < labelsMatrix.Count)
            {
                if (string.IsNullOrEmpty(labelsMatrix[orderNum][location]))
                    return orderNum;
                orderNum++;
            }

            labelsMatrix.Add(new Row());
            return orderNum;
        }

        private void CreateGroup()
        {
            var pos = _camCtrl.GetMainCameraPostion();
            var rot = _camCtrl.GetMainCameraRotation();
            atlas.groups.Add(new AtlasGroup
            {
                cameraPositionX = pos.x,
                cameraPositionY = pos.y,
                cameraPositionZ = pos.z,
                cameraRotationX = rot.x,
                cameraRotationY = rot.y,
                cameraRotationZ = rot.z,
                labels = new List<AtlasLabel>()
            });
            _currentGroupIndex = atlas.groups.Count - 1;
            UpdateActiveInfo("创建标签组成功");
        }

        // 切换当前标签组
        private void ChangeGroup()
        {
            // 循环角标
            _currentGroupIndex = (_currentGroupIndex + 1) % atlas.groups.Count;

            // 标签组视角
            var group = atlas.groups[_currentGroupIndex];
            _camCtrl.SetCameraTransform(
                group.cameraPositionX, group.cameraPositionY, group.cameraPositionZ,
                group.cameraRotationX, group.cameraRotationY, group.cameraRotationZ
            );
            SetLabelsMatrixStates();
            UpdateActiveInfo("已切换标签组");
        }

        // 设置图谱初始视角
        private void SaveAtlasItemCarmera()
        {
            var pos = _camCtrl.GetMainCameraPostion();
            var rot = _camCtrl.GetMainCameraRotation();
            atlas.cameraPositionX = pos.x;
            atlas.cameraPositionY = pos.y;
            atlas.cameraPositionZ = pos.z;
            atlas.cameraRotationX = rot.x;
            atlas.cameraRotationY = rot.y;
            atlas.cameraRotationZ = rot.z;
            UpdateActiveInfo("图谱初始视角已记录");
        }

        // 视角复位
        private void ResetAtlasCarmera()
        {
            _camCtrl.SetCameraTransform(
                atlas.cameraPositionX, atlas.cameraPositionY, atlas.cameraPositionZ,
                atlas.cameraRotationX, atlas.cameraRotationY, atlas.cameraRotationZ
            );
        }

        private void SaveAtlas()
        {
            atlas.name = atlasName;
            LabellabelsMatrixConvertToLabels();

            File.WriteAllText(
                Path.Combine(Application.dataPath, $"Atlas_database/Atlas_{atlas.name}.json"),
                JsonUtility.ToJson(atlas)
            );

            _currentGroupIndex = -1;
            UpdateActiveInfo("图谱存储成功");
        }

        // 更新显示的数据
        private void UpdateActiveInfo(string tipsInfo = null)
        {
            var groupsSum = atlas.groups.Count;
            var groupOrder = "-";
            var labelsSum = "-";
            if (groupsSum > 0 && _currentGroupIndex >= 0)
            {
                groupOrder = $"{_currentGroupIndex + 1}";
                labelsSum = $"{atlas.groups[_currentGroupIndex].labels.Count}";
            }

            _atlasInfo.text =
                $"当前标签组序号/总组数: {groupOrder} / {groupsSum}\n当前组的标签总数: {labelsSum}";

            if (tipsInfo is not null)
            {
                _activeInfo.text = tipsInfo;
                _activeLabel = null;
            }
            else if (_activeLabel is not null)
            {
                _activeInfo.text =
                    $"Value: {_activeLabel.value}\n" +
                    $"Name: {_activeLabel.name}\n" +
                    $"X: {_activeLabel.pointPositionX}\n" +
                    $"Y: {_activeLabel.pointPositionY}\n" +
                    $"Z: {_activeLabel.pointPositionZ}\n";
            }
            else
            {
                _activeInfo.text = "-";
            }
        }

        // 将标签位置矩阵记录的位置赋值给标签实例
        private void LabellabelsMatrixConvertToLabels()
        {
            // 将标签全部提取出来名字映射，变成 { name: atlasLabel} 的结构
            var labelsMap = atlas.groups
                .SelectMany(static group => group.labels)
                .ToDictionary(static lab => lab.name);

            for (var i = 0; i < labelsMatrix.Count; i++)
            {
                for (var location = 0; location < 2; location++)
                {
                    var key = labelsMatrix[i][location];
                    if (!string.IsNullOrEmpty(key) && labelsMap.TryGetValue(key, out var label))
                    {
                        label.orderNum = i;
                        label.location = location;
                    }
                }
            }
        }

        // 根据标签实例的数据生成标签矩阵
        private void LabelsConvertToLabellabelsMatrix()
        {
            labelsMatrix = new List<Row>();
            if (atlas.groups.Count == 0) return;

            // 将标签全部提取出来按上下顺序编组，变成 { row_num: labels[2]} 的结构
            var labelsMap = atlas.groups
                .SelectMany(static group => group.labels)
                .GroupBy(static label => label.orderNum)
                .ToDictionary(
                    static lab => lab.Key, static lab => lab.ToList()
                );

            foreach (var item in labelsMap.OrderBy(static kv => kv.Key))
            {
                var row = item.Value;
                var newRow = new Row();
                foreach (var cell in row)
                    newRow[cell.location] = cell.name;
                labelsMatrix.Add(newRow);
            }
        }

        //根据当前开启的组 设置row的标签状态
        private void SetLabelsMatrixStates()
        {
            var labelNames =
                atlas.groups[_currentGroupIndex].labels.Select(static l => l.name).ToHashSet();

            foreach (var row in labelsMatrix)
                for (var location = 0; location < 2; location++)
                    row.SetState(location, labelNames.Contains(row[location]));
        }
    }
}
