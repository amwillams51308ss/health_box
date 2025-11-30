using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // ★ 全域開關：哪時候允許拖曳
    public static bool DragEnabled = false;

    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // ★ 如果沒開啟拖曳模式，就直接略過
        if (!DragEnabled)
            return;

        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!DragEnabled)
            return;

        if (canvas == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvas.worldCamera,
            out var localPos);

        rectTransform.localPosition = localPos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!DragEnabled)
            return;

        canvasGroup.blocksRaycasts = true;
    }
}
