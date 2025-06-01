using UnityEngine.EventSystems;
using UnityEngine;

public class DragMoveHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("��Ҫ�϶���Ŀ��")]
    public RectTransform targetToMove;

    [Header("�Զ�����������")]
    public CustomCursorManager cursorManager;

    private Vector2 offset;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (targetToMove == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            targetToMove.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );

        offset = targetToMove.anchoredPosition - localPoint;

        if (cursorManager == null) return;
        cursorManager._isHolding = true;
        cursorManager.SetDrag();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (targetToMove == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            targetToMove.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );

        targetToMove.anchoredPosition = localPoint + offset;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (cursorManager == null) return;
        cursorManager._isHolding = false;
        cursorManager.SetDefault();
    }

    private void OnDisable()
    {
        cursorManager?.SetDefault();
    }
}
