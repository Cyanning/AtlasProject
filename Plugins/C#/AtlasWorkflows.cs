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

        public Text infoPanel;
        public Button setLeftBtn;
        public Button setRightBtn;
        public Button changeGroupBtn;
        public Button createGroupsBtn;
        public Button saveAtlasBtn;
        public Button getAtlasCarmeraBtn;
        public Button saveAtlasCarmeraBtn;

        private MainCameraContraller _camCtrl;
        private int _activeGroupIndex;
        private List<AtlasLableGroup> _groups;
        private AtlasLable _activeLable;

        private readonly HashSet<string> _validRoots = new()
        {
            "BodyMaleStatic", "BodyFemaleStatic", "ForamensMale", "ForamensFemale"
        };

        private void Start()
        {
            //按钮绑定事件
            setLeftBtn.onClick.AddListener(SetPoiontAsLeft);
            setRightBtn.onClick.AddListener(SetPoiontAsRight);
            changeGroupBtn.onClick.AddListener(ChangeGroup);
            createGroupsBtn.onClick.AddListener(CreateGroups);
            saveAtlasBtn.onClick.AddListener(SaveAtlas);
            getAtlasCarmeraBtn.onClick.AddListener(GetAtlasCarmera);
            saveAtlasCarmeraBtn.onClick.AddListener(SaveAtlasCarmera);

            labelMatrix = new List<PairLocaltion>();
            _groups = new List<AtlasLableGroup>();

            _camCtrl = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<MainCameraContraller>();

            // 读取点文件
            var dataPath = Path.Combine(Application.dataPath, $"Atlas_database/Atlas_{atlasName}_lables");
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
                    _groups.Add(JsonUtility.FromJson<AtlasLableGroup>(File.ReadAllText(filepath)));
                }

                // 生成标签矩阵
                if (_groups.Count > 0) LabelsConvertToLabelMatrix();
            }
            // 若不存在创建文件夹
            else Directory.CreateDirectory(dataPath);

            UpdateInfoPanel();
            Debug.Log("数据初始化完成");
        }

        public Atlas GetAtlas()
        {
            return JsonUtility.FromJson<Atlas>(
                File.ReadAllText(Path.Combine(Application.dataPath, $"Atlas_database/atlas_{atlasName}.json"))
            );
        }

        // 传入射线坐标
        public void Clicked(RaycastHit raycast)
        {
            var clickedModel = raycast.transform;
            Debug.Log(clickedModel);

            if (!_validRoots.Contains(clickedModel.root.name)) return;
            var raycastPoint = raycast.point;

            var prefabNames = clickedModel.name.Split("~");
            var timestamp = new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds();
            _activeLable = new AtlasLable
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

            _groups[_activeGroupIndex].lables.Add(_activeLable);
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

            _groups[_activeGroupIndex].lables.Add(_activeLable);
            _activeLable = null;
            Debug.Log("当前标签已添加");
        }

        private void CreateGroups()
        {
            var pos = _camCtrl.GetMainCameraPostion();
            var rot = _camCtrl.GetMainCameraRotation();
            _groups.Add(new AtlasLableGroup
            {
                lables = new List<AtlasLable>(),
                cameraPositionX = pos.x,
                cameraPositionY = pos.y,
                cameraPositionZ = pos.z,
                cameraRotationX = rot.x,
                cameraRotationY = rot.y,
                cameraRotationZ = rot.z
            });
            _activeGroupIndex = _groups.Count - 1;
            UpdateInfoPanel();
            Debug.Log("创建组成功");
        }

        private void ChangeGroup()
        {
            // 切换当前标签组
            if (_activeGroupIndex >= 0 && _activeGroupIndex < _groups.Count - 1) _activeGroupIndex++;
            else _activeGroupIndex = 0;

            _camCtrl.SetCameraTransform(
                _groups[_activeGroupIndex].cameraPositionX,
                _groups[_activeGroupIndex].cameraPositionY,
                _groups[_activeGroupIndex].cameraPositionZ,
                _groups[_activeGroupIndex].cameraRotationX,
                _groups[_activeGroupIndex].cameraRotationY,
                _groups[_activeGroupIndex].cameraRotationZ
            );

            UpdateInfoPanel();
        }

        private void SaveAtlas()
        {
            if (_groups.Count == 0 || labelMatrix.Count == 0) return;
            LabelMatrixConvertToLabels();

            for (var i = 0; i < _groups.Count; i++)
            {
                File.WriteAllText(
                    Path.Combine(
                        Application.dataPath, $"Atlas_database/Atlas_{atlasName}_lables/No_{i}_Group_lables.json"
                    ),
                    JsonUtility.ToJson(_groups[i])
                );
            }

            _activeLable = null;
            UpdateInfoPanel();
            Debug.Log("图谱存储成功");
        }

        private void SaveAtlasCarmera()
        {
            var atlas = GetAtlas();
            var pos = _camCtrl.GetMainCameraPostion();
            var rot = _camCtrl.GetMainCameraRotation();
            atlas.cameraPositionX = pos.x;
            atlas.cameraPositionY = pos.y;
            atlas.cameraPositionZ = pos.z;
            atlas.cameraRotationX = rot.x;
            atlas.cameraRotationY = rot.y;
            atlas.cameraRotationZ = rot.z;

            File.WriteAllText(
                Path.Combine(Application.dataPath, $"Atlas_database/Atlas_{atlasName}.json"),
                JsonUtility.ToJson(atlas)
            );
            Debug.Log("图谱初始视角已设置");
        }

        private void GetAtlasCarmera()
        {
            // 设置初始视角
            var atlas = GetAtlas();
            _camCtrl.SetCameraTransform(
                atlas.cameraPositionX, atlas.cameraPositionY, atlas.cameraPositionZ,
                atlas.cameraRotationX, atlas.cameraRotationY, atlas.cameraRotationZ
            );
        }

        private void UpdateInfoPanel()
        {
            // 更新显示的数据
            var content = $"Group Order Number: {_activeGroupIndex + 1} / {_groups.Count}\n";

            if (_groups.Count > 0)
            {
                content += $"Labels Number: {_groups[_activeGroupIndex].lables.Count}\n";
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

            infoPanel.text = content;
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
            foreach (var lable in _groups.SelectMany(static group => group.lables))
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
                _groups.SelectMany(static group => group.lables)
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
