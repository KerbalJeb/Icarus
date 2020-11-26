using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    /// <summary>
    ///     Basic draggable UI element
    /// </summary>
    public class DraggableUI : MonoBehaviour, IDragHandler, IPointerDownHandler
    {
        private RectTransform draggableAreaRectTransform = null;

        private RectTransform draggableRectTransform = null;
        // todo constrain to screen and update to not use evenData.delta

        private Vector2 startingLocalMousePosition;
        private Vector3 startingPanelLocalPosition;


        private void Awake()
        {
            draggableRectTransform = transform.parent as RectTransform;
            Canvas    canvas          = null;
            Transform parentTransform = transform;
            while (parentTransform != null)
            {
                canvas = parentTransform.GetComponent<Canvas>();
                if (canvas != null) break;
                parentTransform = parentTransform.parent;
            }

            if (canvas is null)
            {
                Debug.Log("Canvas not found");
                return;
            }

            draggableAreaRectTransform = canvas.transform as RectTransform;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(draggableAreaRectTransform, eventData.position,
                                                                        eventData.pressEventCamera,
                                                                        out Vector2 localMousePos))
            {
                Vector3 offset = localMousePos - startingLocalMousePosition;
                draggableRectTransform.localPosition = startingPanelLocalPosition + offset;
            }

            ClampMovement();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            startingPanelLocalPosition = draggableRectTransform.localPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(draggableAreaRectTransform, eventData.position,
                                                                    eventData.pressEventCamera,
                                                                    out startingLocalMousePosition);
        }

        private void ClampMovement()
        {
            Vector3 localPosition = draggableRectTransform.localPosition;
            Vector3 pos           = localPosition;
            Rect    rect          = draggableAreaRectTransform.rect;
            Rect    rect1         = draggableRectTransform.rect;

            Vector3 minPos = rect.min - rect1.min;
            Vector3 maxPos = rect.max - rect1.max;

            pos.x = Mathf.Clamp(localPosition.x, minPos.x, maxPos.x);
            pos.y = Mathf.Clamp(localPosition.y, minPos.y, maxPos.y);

            localPosition                        = pos;
            draggableRectTransform.localPosition = localPosition;
        }
    }
}
