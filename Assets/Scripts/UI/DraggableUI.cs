using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableUI : MonoBehaviour, IDragHandler
{
    private RectTransform rectTransform;
    private Canvas        canvas;

    private void Awake()
    {
        rectTransform   = GetComponent<RectTransform>();
        var parentTransform = transform;
        while (parentTransform != null)
        {
            canvas = parentTransform.GetComponent<Canvas>();
            if (canvas != null)
            {
                break;
            }
            parentTransform = parentTransform.parent;
        }

    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta/canvas.scaleFactor;
    }
}

