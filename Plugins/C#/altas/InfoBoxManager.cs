using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class InfoBoxManager : MonoBehaviour
{
    public static InfoBoxManager Instance;
    public GameObject popupPrefab;
    public Canvas rootCanvas;

    private GameObject popup;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void ShowInfoBox(GameObject anchorPos, string content,bool isLeft)
    {
        Debug.Log("anchor position "+anchorPos.transform.position);
        if (popupPrefab == null)
        {
            Debug.LogError("弹窗预制体未配置！");
            return;
        }
        if (popup != null) {
            Destroy(popup);
            popup = null;
        }
        // 实例化弹窗
        popup = Instantiate(popupPrefab, rootCanvas.transform);

        // 获取按钮的屏幕坐标
        //RectTransform buttonRect = anchorPos.GetComponent<RectTransform>();
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(
            rootCanvas.worldCamera,
            //buttonRect.position
            anchorPos.transform.position
        );
        //Vector2 screenPos = anchorPos.transform.position;
        // 转换为Canvas本地坐标
        RectTransform popupRect = popup.GetComponent<RectTransform>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.GetComponent<RectTransform>(),
            screenPos,
            rootCanvas.worldCamera,
            out Vector2 localPos
        );
        Debug.Log("localPos position " + localPos);
        popupRect.pivot = new Vector2(
            isLeft ? 1 : 0,
            1f
        );
        Vector2 newAnchor = new Vector2(0,-44);
        // 设置位置并偏移
        popupRect.anchoredPosition = localPos - newAnchor;

        // 设置内容（网页1的组件获取方式）
        Text contentText = popup.transform.Find("PanelBg/TxtContent").GetComponent<Text>();
        contentText.text = content;

        // 动画效果（网页5的动画实现）
        popup.transform.localScale = Vector3.zero;
        popup.transform.DOScale(Vector3.one, 0.3f)
            .SetEase(Ease.OutBack);

        // 绑定关闭事件（网页2的事件监听）
        Button closeBtn = popup.transform.Find("BtnClose").GetComponent<Button>();
        closeBtn.onClick.AddListener(() => {
            popup.transform.DOScale(Vector3.zero, 0.2f)
                .OnComplete(() => Destroy(popup));
        });
    }

}

