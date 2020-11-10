using UnityEngine;
using UnityEngine.UI;

public class FixedGridLayout: LayoutGroup
{
    public Vector2   cellSize = new Vector2(50, 50);
    public Vector2   spacing;
    public Alignment alignment;

    public enum Alignment
    {
        Center,
    }

    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();
        float width = rectTransform.rect.width;

        int columns = Mathf.FloorToInt(width / (cellSize.x + spacing.x));
        var offset  = (width - columns       * cellSize.x) / (columns + 1);

        var rowIdx = 0;
        for (int i = 0; i < rectChildren.Count; i++)
        {
            var columnIdx = i % columns;
            if (i % columns == 0 && i != 0) rowIdx++;

            var xPos  = (cellSize.x + offset) * columnIdx + offset;
            var yPos  = (cellSize.y + offset) * rowIdx    + offset;
            var child = rectChildren[i];
            
            SetChildAlongAxis(child, 0, xPos, cellSize.x);
            SetChildAlongAxis(child, 1, yPos, cellSize.y);
        }

        float height = Mathf.CeilToInt(rectChildren.Count / (float) columns) * (cellSize.y + offset);
        height                         = Mathf.Max(height, rectTransform.rect.height);
        rectTransform.sizeDelta        = new Vector2(0, height);
        rectTransform.anchoredPosition = new Vector2(0, -height /2);
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

