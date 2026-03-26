using UnityEngine;
using UnityEngine.UI;

public class GridExpand : MonoBehaviour
{
    public GridLayoutGroup grid;
    public int columns = 2;
    public float spacing = 2f;

    void Update()
    {
        if (!grid) return;

        RectTransform rt = grid.GetComponent<RectTransform>();
        float totalWidth = rt.rect.width;

        float totalSpacing = spacing * (columns - 1);
        float cellWidth = (totalWidth - totalSpacing) / columns;

        grid.cellSize = new Vector2(cellWidth, grid.cellSize.y);
    }
}
