using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Plugins.C_.models;


namespace Plugins.C_
{
    [Serializable]
    public class Row
    {
        public string left;
        public string right;
    }

    public class AtlasWorkflows : MonoBehaviour
    {
        public string atlasName;
        public List<Row> matrix;

        private MainCameraContraller _camCtrl;
        private Text _infoPanel;

        private AtlasItem _atlas; // 缓存图谱对象
        private int _activeGroupIndex; // 当前打开的组的角标
        private AtlasLabel _activeLable; // 缓存一个标签点

        private static readonly HashSet<string> ValidRoots = new()
        {
            "BodyMaleStatic(Clone)",
            "BodyFemaleStatic(Clone)",
            "ForamensMale(Clone)",
            "ForamensFemale(Clone)"
        };

        public AtlasItem GetAtlas()
        {
            if (_atlas == null)
            {
                LoadAtlas();
            }

            return _atlas;
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
                        userInterface.GetComponent<Button>().onClick.AddListener(SetPoiontAsLeft);
                        break;
                    case "SetRightBtn":
                        userInterface.GetComponent<Button>().onClick.AddListener(SetPoiontAsRight);
                        break;
                    case "ChangeGroupBtn":
                        userInterface.GetComponent<Button>().onClick.AddListener(ChangeGroup);
                        break;
                    case "CreateGroupsBtn":
                        userInterface.GetComponent<Button>().onClick.AddListener(CreateGroups);
                        break;
                    case "SaveAtlasBtn":
                        userInterface.GetComponent<Button>().onClick.AddListener(SaveAtlas);
                        break;
                    case "GetAtlasCarmeraBtn":
                        userInterface.GetComponent<Button>().onClick.AddListener(GetAtlasCarmera);
                        break;
                    case "SaveAtlasCarmeraBtn":
                        userInterface.GetComponent<Button>().onClick.AddListener(SaveAtlasCarmera);
                        break;
                    case "LableInfo":
                        _infoPanel = userInterface.GetComponent<Text>();
                        break;
                }
            }

            // 初始化数据
            LoadAtlas();
            _atlas.groups = new List<AtlasGroup>();
            matrix = new List<Row>();

            // 读取点文件
            var dataPath = Path.Combine(Application.dataPath, $"Atlas_database/Atlas_{_atlas.name}_lables");
            if (Directory.Exists(dataPath))
            {
                foreach (var filepath in Directory.EnumerateFiles(dataPath)) // 读取历史数据
                {
                    // 检查后缀
                    if (!filepath.EndsWith(".json")) continue;

                    // 检查文件名前后缀是否完整
                    var startIdx = filepath.LastIndexOf("No_", StringComparison.Ordinal);
                    var endIdx = filepath.LastIndexOf("_Group_lables", StringComparison.Ordinal);
                    if (startIdx == -1 || endIdx == -1) continue;

                    // 遍历标签组文件并存储为对象
                    _atlas.groups.Add(JsonUtility.FromJson<AtlasGroup>(File.ReadAllText(filepath)));
                }

                // 生成标签矩阵
                if (_atlas.groups.Count > 0) LabelsConvertToLabelMatrix();
            }
            // 若不存在创建文件夹
            else Directory.CreateDirectory(dataPath);

            UpdateInfoPanel();
            Debug.Log("数据初始化完成");
        }

        public void Clicked(RaycastHit raycast) // 传入射线坐标
        {
            var clickedModel = raycast.transform;
            if (!ValidRoots.Contains(clickedModel.root.name)) return;

            var raycastPoint = raycast.point;
            var prefabNames = clickedModel.name.Split("~");
            var timestamp = new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds();
            _activeLable = new AtlasLabel
            {
                name = prefabNames[0] + "_" + timestamp.ToString()[^4..],
                value = Convert.ToInt32(prefabNames[1]),
                pointPositionX = raycastPoint.x,
                pointPositionY = raycastPoint.y,
                pointPositionZ = raycastPoint.z,
            };
            UpdateInfoPanel();
        }

        private void SetPoiontAsLeft()
        {
            if (_activeLable == null)
            {
                Debug.Log("当前无标签坐标");
                return;
            }

            _activeLable.location = 0;
            var targetLocaltionOrderNum = GetLocaltionOrderNum()[0];
            if (targetLocaltionOrderNum == -1)
            {
                matrix.Add(new Row { left = _activeLable.name });
            }
            else
            {
                matrix[targetLocaltionOrderNum].left = _activeLable.name;
            }

            _atlas.groups[_activeGroupIndex].labels.Add(_activeLable);
            _activeLable = null;
            Debug.Log("当前标签已添加");
        }

        private void SetPoiontAsRight()
        {
            if (_activeLable == null)
            {
                Debug.Log("当前无标签坐标");
                return;
            }

            _activeLable.location = 1;
            var targetLocaltionOrderNum = GetLocaltionOrderNum()[1];
            if (targetLocaltionOrderNum == -1)
            {
                matrix.Add(new Row { right = _activeLable.name });
            }
            else
            {
                matrix[targetLocaltionOrderNum].right = _activeLable.name;
            }

            _atlas.groups[_activeGroupIndex].labels.Add(_activeLable);
            _activeLable = null;
            Debug.Log("当前标签已添加");
        }

        private void CreateGroups()
        {
            var pos = _camCtrl.GetMainCameraPostion();
            var rot = _camCtrl.GetMainCameraRotation();
            _atlas.groups.Add(new AtlasGroup
            {
                labels = new List<AtlasLabel>(),
                cameraPositionX = pos.x,
                cameraPositionY = pos.y,
                cameraPositionZ = pos.z,
                cameraRotationX = rot.x,
                cameraRotationY = rot.y,
                cameraRotationZ = rot.z
            });
            _activeGroupIndex = _atlas.groups.Count - 1;
            UpdateInfoPanel();
            Debug.Log("创建组成功");
        }

        private void ChangeGroup()
        {
            // 切换当前标签组
            if (_activeGroupIndex >= 0 && _activeGroupIndex < _atlas.groups.Count - 1) _activeGroupIndex++;
            else _activeGroupIndex = 0;

            _camCtrl.SetCameraTransform(
                _atlas.groups[_activeGroupIndex].cameraPositionX,
                _atlas.groups[_activeGroupIndex].cameraPositionY,
                _atlas.groups[_activeGroupIndex].cameraPositionZ,
                _atlas.groups[_activeGroupIndex].cameraRotationX,
                _atlas.groups[_activeGroupIndex].cameraRotationY,
                _atlas.groups[_activeGroupIndex].cameraRotationZ
            );

            UpdateInfoPanel();
        }

        private void SaveAtlas()
        {
            if (_atlas.groups.Count == 0 || matrix.Count == 0) return;
            LabelMatrixConvertToLabels();

            for (var i = 0; i < _atlas.groups.Count; i++)
            {
                File.WriteAllText(
                    Path.Combine(
                        Application.dataPath, $"Atlas_database/Atlas_{_atlas.name}_lables/No_{i}_Group_lables.json"
                    ),
                    JsonUtility.ToJson(_atlas.groups[i])
                );
            }

            _activeLable = null;
            UpdateInfoPanel();
            Debug.Log("图谱存储成功");
        }

        private void LoadAtlas()
        {
            try
            {
                _atlas = JsonUtility.FromJson<AtlasItem>(
                    File.ReadAllText(Path.Combine(Application.dataPath, $"Atlas_database/atlas_{atlasName}.json"))
                );
                atlasName = _atlas.name;
            }
            catch (FileNotFoundException)
            {
                Debug.LogWarning($"没有发现 {atlasName} 的数据文件");
            }
        }

        private void SaveAtlasCarmera()
        {
            var pos = _camCtrl.GetMainCameraPostion();
            var rot = _camCtrl.GetMainCameraRotation();
            _atlas.cameraPositionX = pos.x;
            _atlas.cameraPositionY = pos.y;
            _atlas.cameraPositionZ = pos.z;
            _atlas.cameraRotationX = rot.x;
            _atlas.cameraRotationY = rot.y;
            _atlas.cameraRotationZ = rot.z;

            File.WriteAllText(
                Path.Combine(Application.dataPath, $"Atlas_database/Atlas_{_atlas.name}.json"),
                JsonUtility.ToJson(_atlas)
            );
            Debug.Log("图谱初始视角已设置");
        }

        private void GetAtlasCarmera()
        {
            // 设置初始视角
            _camCtrl.SetCameraTransform(
                _atlas.cameraPositionX, _atlas.cameraPositionY, _atlas.cameraPositionZ,
                _atlas.cameraRotationX, _atlas.cameraRotationY, _atlas.cameraRotationZ
            );
        }

        private void UpdateInfoPanel()
        {
            // 更新显示的数据
            var content = $"Group Order Number: {_activeGroupIndex + 1} / {_atlas.groups.Count}\n";

            if (_atlas.groups.Count > 0)
            {
                content += $"Labels Number: {_atlas.groups[_activeGroupIndex].labels.Count}\n";
            }


            if (_activeLable != null)
            {
                content += "~~~~~~~~~~~~~~~~~~~~~~~~~~~~\n" +
                           $"Value: {_activeLable.value}\n" +
                           $"Name: {_activeLable.name}\n" +
                           $"X: {_activeLable.pointPositionX},\n" +
                           $"Y: {_activeLable.pointPositionY},\n" +
                           $"Z: {_activeLable.pointPositionZ}";
            }

            _infoPanel.text = content;
        }

        // 从上至下找出为空的标签位置，若无空位返回值为-1
        private int[] GetLocaltionOrderNum()
        {
            LabelMatrixConvertToLabels();
            var orderNums = new[] { -1, -1 };
            for (var i = 0; i < matrix.Count; i++)
            {
                if (string.IsNullOrEmpty(matrix[i].left))
                {
                    orderNums[0] = i;
                }

                if (string.IsNullOrEmpty(matrix[i].right))
                {
                    orderNums[1] = i;
                }
            }

            return orderNums;
        }

        // 将标签位置矩阵记录的位置赋值给标签实例
        private void LabelMatrixConvertToLabels()
        {
            var labelsMap = _atlas.groups
                .SelectMany(static group => group.labels)
                .ToDictionary(static lab => lab.name);

            for (var i = 0; i < matrix.Count; i++)
            {
                if (labelsMap.TryGetValue(matrix[i].left, out var labelLeft))
                {
                    labelLeft.orderNum = i;
                    labelLeft.location = 0;
                }

                if (labelsMap.TryGetValue(matrix[i].right, out var labelRight))
                {
                    labelRight.orderNum = i;
                    labelRight.location = 1;
                }
            }
        }

        // 根据标签实例的数据生成标签矩阵
        private void LabelsConvertToLabelMatrix()
        {
            // 讲标签全部提取出来按上下顺序编组，变成 { row_num: label} 的结构
            var labelsMap = _atlas.groups
                .SelectMany(static group => group.labels)
                .GroupBy(static label => label.orderNum)
                .ToDictionary(
                    static lab => lab.Key, static lab => lab.ToList()
                );

            matrix.Clear();
            foreach (var item in labelsMap.OrderBy(static kv => kv.Key))
            {
                var row = item.Value;
                if (row[0].location == 0 && row[1].location == 1)
                    matrix.Add(new Row { left = row[0].name, right = row[1].name });
                else
                    matrix.Add(new Row { left = row[1].name, right = row[0].name });
            }
        }
    }
}
