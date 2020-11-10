using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// A custom grid layout for UI elements, will give even spacing 
    /// </summary>
    public class FixedGridLayout : LayoutGroup
    {
        public enum Alignment
        {
            Center,
        }

        /// <value>
        /// The size of each cell
        /// </value>
        public Vector2 cellSize = new Vector2(50, 50);
        /// <value>
        /// The spacing between each cell, will be slightly adjusted to fit within the bounds of the rect transform
        /// </value>
        public Vector2 spacing;
        public Alignment alignment;

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            float width = rectTransform.rect.width;

            int   columns = Mathf.FloorToInt(width / (cellSize.x + spacing.x));
            float offset  = (width - columns       * cellSize.x) / (columns + 1);

            var rowIdx = 0;
            for (var i = 0; i < rectChildren.Count; i++)
            {
                int columnIdx = i % columns;
                if (i % columns == 0 && i != 0) rowIdx++;

                float         xPos  = (cellSize.x + offset) * columnIdx + offset;
                float         yPos  = (cellSize.y + offset) * rowIdx    + offset;
                RectTransform child = rectChildren[i];

                SetChildAlongAxis(child, 0, xPos, cellSize.x);
                SetChildAlongAxis(child, 1, yPos, cellSize.y);
            }

            float height = Mathf.CeilToInt(rectChildren.Count / (float) columns) * (cellSize.y + offset);
            height                         = Mathf.Max(height, rectTransform.rect.height);
            rectTransform.sizeDelta        = new Vector2(0, height);
            rectTransform.anchoredPosition = new Vector2(0, -height / 2);
        }

        public override void CalculateLayoutInputVertical()
        {
        }

        public override void SetLayoutHorizontal()
        {
        }

        public override void SetLayoutVertical()
        {
        }
    }
}
