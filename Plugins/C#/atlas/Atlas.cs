using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Plugins.C_.atlas
{
    public class Atlas : MonoBehaviour
    {

        [Header("基础设置")] public int panelCount = 7;

        //public GameObject cube;
        public GameObject panelPrefab;

        [Header("连线设置")] public Gradient lineColor;
        public AnimationCurve lineWidth = AnimationCurve.Linear(0, 0.01f, 1, 0.03f);

        [Header("拖拽限制")] public float marginPercentage = 0.05f;

        private List<PointInfo> panels = new List<PointInfo>();
        private List<LineRenderer> lines = new List<LineRenderer>();
        private Rect safeArea;

        // 对象池优化
        private Queue<GameObject> panelPool = new Queue<GameObject>();
        private int POOL_SIZE = 7;
        private float LINE_WIDTH = .0002f;

        void Start()
        {

            InitializePool();
            CalculateSafeArea();
            CreateUI();
            InitLines();
        }

        void InitializePool()
        {
            POOL_SIZE = panelCount;
            for (int i = 0; i < POOL_SIZE; i++)
            {
                GameObject obj = Instantiate(panelPrefab);
                obj.SetActive(false);
                panelPool.Enqueue(obj);
            }
        }

        GameObject GetPooledPanel()
        {
            if (panelPool.Count > 0)
            {
                GameObject obj = panelPool.Dequeue();
                obj.SetActive(true);
                return obj;
            }

            return Instantiate(panelPrefab);
        }

        void CalculateSafeArea()
        {
            RectTransform canvasRect = GetComponent<RectTransform>();
            safeArea = Screen.safeArea;

            // 转换为Canvas本地坐标
            Vector2 min = safeArea.position / canvasRect.localScale.x;
            Vector2 max = (safeArea.position + safeArea.size) / canvasRect.localScale.x;

            safeArea = Rect.MinMaxRect(
                min.x + marginPercentage * Screen.width,
                min.y + marginPercentage * Screen.height,
                max.x - marginPercentage * Screen.width,
                max.y - marginPercentage * Screen.height
            );
        }

        void CreateUI()
        {
            // 原有创建逻辑替换为对象池版本
            Transform left = GameObject.Find("LeftContainer").transform;
            Transform right = GameObject.Find("RightContainer").transform;

            for (int i = 0; i < panelCount; i++)
            {
                bool isLeft = i < panelCount / 2 ? true : false;
                Debug.Log("info isLeft = " + isLeft);
                GameObject panel = GetPooledPanel();

                Text textInfo = panel.transform.Find("PanelItem/TxtContent").GetComponent<Text>();
                Button imgHide = panel.transform.Find("PanelItem/BtnHide").GetComponent<Button>();
                Button imgInfo = panel.transform.Find("PanelItem/BtnInfo").GetComponent<Button>();

                panel.transform.SetParent(isLeft ? left : right, false);
                //重置锚点 ，将连线放在右侧或者左侧，不指向中心
                RectTransform rectTransform = panel.GetComponent<RectTransform>();
                rectTransform.pivot = new Vector2(
                    isLeft ? 1 : 0,
                    0.5f
                );

                string idStr = Convert.ToString(i);
                imgHide.name = idStr;
                imgInfo.name = idStr;
                //AddDragEvents(panel);
                PointInfo info = new PointInfo();
                info.did = idStr;
                info.content = "content_" + i;
                info.panelObject = panel;
                info.hideObject = imgHide;
                info.infoObject = imgInfo;
                info.textObject = textInfo;
                info.isLeft = isLeft;
                panels.Add(info);

                EventTrigger triggerInfo = imgInfo.gameObject.AddComponent<EventTrigger>();
                EventTrigger.Entry entryInfo = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerClick
                };
                entryInfo.callback.AddListener((data) =>
                {
                    string id = data.selectedObject.name;
                    Debug.Log("info id = " + id);
                    OnItemClick(id, 1);
                });
                triggerInfo.triggers.Add(entryInfo);

                EventTrigger triggerHide = imgHide.gameObject.AddComponent<EventTrigger>();
                EventTrigger.Entry entryHide = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerClick
                };
                entryHide.callback.AddListener((data) =>
                {
                    string id = data.selectedObject.name;
                    Debug.Log("hide id = " + id);
                    OnItemClick(id, 0);
                });
                triggerHide.triggers.Add(entryHide);
            }

            Canvas.ForceUpdateCanvases();

        }

        //type = 0 隐藏，1显示信息
        void OnItemClick(string id, int type)
        {
            PointInfo clickItem = panels.Find(i => i.did.Equals(id));
            if (clickItem != null)
            {
                if (type == 0)
                {
                    clickItem.isHide = true;
                    clickItem.panelObject.SetActive(false);
                }
                else
                {
                    bool isLeft = clickItem.isLeft;
                    panels.FindAll(item =>
                        item.isLeft == isLeft && clickItem.did != item.did
                    ).ForEach(item =>
                    {
                        //Debug.Log("hide id = "+item.did + "is left "+item.isLeft);
                        item.panelObject.SetActive(false);
                    });
                    string txtTest =
                        "title\ncontentcontentcontentcontentcontent\n\ncontentcontentcontent\ncontentcontentcontent\nncontent\ncontentcontent";
                    InfoBoxManager.Instance.ShowInfoBox(clickItem.panelObject, txtTest, isLeft);
                }

            }

        }

        //关闭信息窗口，并恢复展示信息后隐藏的部分item,不包含单独隐藏的
        void CloseInfoPop(bool isLeft)
        {
            panels.ForEach(item =>
            {
                if (!item.isHide)
                {
                    if (isLeft)
                    {
                        item.panelObject.SetActive(true);
                    }
                }
            });
        }

        void AddDragEvents(GameObject panel)
        {
            // 原有拖拽逻辑增加边界检测
            EventTrigger trigger = panel.AddComponent<EventTrigger>();

            var drag = new EventTrigger.Entry { eventID = EventTriggerType.Drag };
            drag.callback.AddListener((data) =>
            {
                OnDrag((PointerEventData)data);
                ClampPosition(panel.GetComponent<RectTransform>());
            });

            trigger.triggers.Add(drag);
        }

        void OnDrag(PointerEventData data)
        {
            RectTransform rect = data.pointerDrag.GetComponent<RectTransform>();
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rect.parent as RectTransform,
                    data.position,
                    data.pressEventCamera,
                    out Vector2 localPos))
            {
                rect.localPosition = localPos;
            }
        }


        void ClampPosition(RectTransform rect)
        {
            Vector3[] corners = new Vector3[4];
            rect.GetWorldCorners(corners);

            float minX = corners.Min(c => c.x);
            float maxX = corners.Max(c => c.x);
            float minY = corners.Min(c => c.y);
            float maxY = corners.Max(c => c.y);

            Vector3 adjustment = Vector3.zero;
            if (minX < safeArea.xMin) adjustment.x = safeArea.xMin - minX;
            if (maxX > safeArea.xMax) adjustment.x = safeArea.xMax - maxX;
            if (minY < safeArea.yMin) adjustment.y = safeArea.yMin - minY;
            if (maxY > safeArea.yMax) adjustment.y = safeArea.yMax - maxY;

            rect.position += adjustment;
        }

        void InitLines()
        {
            foreach (var panel in panels)
            {
                LineRenderer lr = panel.panelObject.AddComponent<LineRenderer>();
                lr.sortingOrder = 10;
                lr.material = new Material(Shader.Find("Sprites/Default"));
                lr.positionCount = 2;
                lr.widthCurve = lineWidth;
                lr.colorGradient = lineColor;

                //平滑度
                //lr.numCapVertices = 3;
                //lr.numCornerVertices = 5;
                lr.generateLightingData = true;
                lr.startWidth = LINE_WIDTH;
                lr.endWidth = LINE_WIDTH;
                //lr.useWorldSpace = false;
                //lr.alignment = LineAlignment.TransformZ;
                // 性能优化设置
                lr.shadowCastingMode = ShadowCastingMode.Off;
                lr.receiveShadows = false;
                lr.lightProbeUsage = LightProbeUsage.Off;

                lines.Add(lr);
            }

            //cube.AddComponent<CubeDragger>();
        }

        void Update()
        {
            // 旋转操作结束后，再更新
            // 增加动态宽度调整
            for (int i = 0; i < panels.Count; i++)
            {
                PointInfo pointInfo = panels[i];
                bool isLeft = pointInfo.isLeft;
                Vector3 start = pointInfo.panelObject.transform.position;
                //Debug.Log("position = "+start);
                Vector3 end = new Vector3(Convert.ToSingle(pointInfo.PositionX), Convert.ToSingle(pointInfo.PositionY),
                    Convert.ToSingle(pointInfo.PositionZ));
                //Vector3 modelScreenPos = Camera.main.WorldToScreenPoint(start);
                //Vector3 centerAnchor = new Vector3(isLeft ? start.x + 3 * LINE_WIDTH : start.x - 3 * LINE_WIDTH, start.y, start.z);

                lines[i].SetPositions(new[] { start, end });

                // 根据距离动态调整宽度
                float distance = Vector3.Distance(start, end);
                lines[i].widthMultiplier = Mathf.Clamp(1 - distance / 15f, 0.1f, 1f);
            }
        }

        void OnDestroy()
        {
            // 对象池回收
            foreach (var panel in panels)
            {
                panel.panelObject.SetActive(false);
                panelPool.Enqueue(panel.panelObject);
            }
        }
    }

}
