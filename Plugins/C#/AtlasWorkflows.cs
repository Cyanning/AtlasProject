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
    public class PairLocaltion
    {
        public string l;
        public string r;
    }

    public class AtlasWorkflows : MonoBehaviour
    {
        public string atlasName;
        public List<PairLocaltion> labelMatrix;

        private Text _infoPanel;

        private AtlasItem _atlas;
        private int _activeGroupIndex;
        private AtlasLabel _activeLable;
        private MainCameraContraller _camCtrl;

        private static readonly HashSet<string> ValidRoots = new()
        {
            "BodyMaleStatic(Clone)", "BodyFemaleStatic(Clone)", "ForamensMale(Clone)", "ForamensFemale(Clone)"
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

            // 绑定主相机脚本
            _camCtrl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<MainCameraContraller>();

            // 初始化数据
            LoadAtlas();
            _atlas.groups = new List<AtlasGroup>();
            labelMatrix = new List<PairLocaltion>();

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
                labelMatrix.Add(new PairLocaltion { l = _activeLable.name });
            }
            else
            {
                labelMatrix[targetLocaltionOrderNum].l = _activeLable.name;
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
                labelMatrix.Add(new PairLocaltion { r = _activeLable.name });
            }
            else
            {
                labelMatrix[targetLocaltionOrderNum].r = _activeLable.name;
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
            if (_atlas.groups.Count == 0 || labelMatrix.Count == 0) return;
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
                Debug.Log($"没有发现 {atlasName} 的数据文件");
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

        private int[] GetLocaltionOrderNum()
        {
            // 从上至下找出为空的标签位置，若无空位返回值为-1
            LabelMatrixConvertToLabels();
            var orderNums = new[] { -1, -1 };
            for (var i = 0; i < labelMatrix.Count; i++)
            {
                if (string.IsNullOrEmpty(labelMatrix[i].l))
                {
                    orderNums[0] = i;
                }

                if (string.IsNullOrEmpty(labelMatrix[i].r))
                {
                    orderNums[1] = i;
                }
            }

            return orderNums;
        }

        private void LabelMatrixConvertToLabels()
        {
            // 将标签位置矩阵记录的位置赋值给标签实例
            foreach (var lable in _atlas.groups.SelectMany(static group => group.labels))
            {
                for (var i = 0; i < labelMatrix.Count; i++)
                {
                    if (lable.name == labelMatrix[i].l) lable.location = 0;
                    else if (lable.name == labelMatrix[i].r) lable.location = 1;
                    else continue;

                    lable.orderNum = i;
                    break;
                }
            }
        }

        private void LabelsConvertToLabelMatrix() // 根据标签实例的数据生成标签矩阵
        {
            labelMatrix.Clear();
            var labelsMap =
                _atlas.groups.SelectMany(static group => group.labels)
                    .GroupBy(static label => label.orderNum)
                    .ToDictionary(
                        static lab => lab.Key,
                        static lab => lab.ToList()
                    );

            var i = 0;
            var flag = true;
            while (flag)
            {
                if (labelsMap.TryGetValue(i, out var labels))
                {
                    var apair = new PairLocaltion();
                    foreach (var label in labels)
                    {
                        if (label.location == 0) apair.l = label.name;
                        else if (label.location == 1) apair.r = label.name;
                    }

                    labelMatrix.Add(apair);
                }
                else
                {
                    flag = false;
                    foreach (var key in labelsMap.Keys)
                    {
                        if (i >= key) continue;
                        labelMatrix.Add(new PairLocaltion());
                        flag = true;
                        break;
                    }
                }

                i++;
            }
        }
    }
}
