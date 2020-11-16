﻿using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    /// <summary>
    /// Basic draggable UI element
    /// </summary>
    public class DraggableUI : MonoBehaviour, IDragHandler
    {
        // todo constrain to screen and update to not use evenData.delta
        private Canvas        canvas;
        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            Transform parentTransform = transform;
            while (parentTransform != null)
            {
                canvas = parentTransform.GetComponent<Canvas>();
                if (canvas != null) break;
                parentTransform = parentTransform.parent;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }
    }
}