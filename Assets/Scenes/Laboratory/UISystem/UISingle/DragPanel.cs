using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class DragPanel : MonoBehaviour, IPointerDownHandler, IDragHandler
{

    // 鼠标起点  
    private Vector2 originalLocalPointerPosition;
    // 面板起点  
    private Vector3 originalPanelLocalPosition;
    // 当前面板  
    private RectTransform panelRectTransform;
    // 父节点,这个最好是UI父节点，因为它的矩形大小刚好是屏幕大小  
    private RectTransform parentRectTransform;

    private static int siblingIndex = 0;
    void Start()
    {
        panelRectTransform = transform.parent as RectTransform;
        parentRectTransform = panelRectTransform.parent as RectTransform;
    }

    // 鼠标按下  
    public void OnPointerDown(PointerEventData data)
    {
        siblingIndex++;
        panelRectTransform.transform.SetSiblingIndex(siblingIndex);
        // 记录当前面板起点  
        originalPanelLocalPosition = panelRectTransform.localPosition;
        // 通过屏幕中的鼠标点，获取在父节点中的鼠标点  
        // parentRectTransform:父节点  
        // data.position:当前鼠标位置  
        // data.pressEventCamera:当前事件的摄像机  
        // originalLocalPointerPosition:获取当前鼠标起点  
        UnityEngine.RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, data.position, data.pressEventCamera, out originalLocalPointerPosition);
    }
    // 拖动  
    public void OnDrag(PointerEventData data)
    {
        if (panelRectTransform == null || parentRectTransform == null)
            return;
        Vector2 localPointerPosition;
        // 获取本地鼠标位置  
        if (UnityEngine.RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, data.position, data.pressEventCamera, out localPointerPosition))
        {
            // 移动位置 = 本地鼠标当前位置 - 本地鼠标起点位置  
            Vector3 offsetToOriginal = localPointerPosition - originalLocalPointerPosition;
            // 当前面板位置 = 面板起点 + 移动位置  
            panelRectTransform.localPosition = originalPanelLocalPosition + offsetToOriginal;
        }
        ClampToWindow();
    }

    // 限制当前面板在父节点中的区域位置  
    void ClampToWindow()
    {
        // 面板位置  
        Vector3 pos = panelRectTransform.localPosition;

        // 如果是UI父节点，设置面板大小为0，那么最大最小位置为正负屏幕的一半  
        Vector3 minPosition = parentRectTransform.rect.min - panelRectTransform.rect.min;
        Vector3 maxPosition = parentRectTransform.rect.max - panelRectTransform.rect.max;

        pos.x = Mathf.Clamp(panelRectTransform.localPosition.x, minPosition.x, maxPosition.x);
        pos.y = Mathf.Clamp(panelRectTransform.localPosition.y, minPosition.y, maxPosition.y);

        panelRectTransform.localPosition = pos;
    }
}