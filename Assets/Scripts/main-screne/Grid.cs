using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Grid : MonoBehaviour
{

    private static readonly float MIN_DISPLAY_TIME = 60; // 1 minute

    /// <summary>
    /// The prefab to use to instantiate the cells
    /// </summary>
    public GameObject CellPrefab;

    /// <summary>
    /// The overlay panel to show during drag of a cell
    /// (cell is a prefab => cannot reference this comp
    /// directly in the DragDrop script of the cell)
    /// </summary>
    public GameObject OverlayPanel;

    /// <summary>
    /// The cells
    /// </summary>
    public List<Cell> Cells { get; private set; }

    /// <summary>
    /// Initialize the grid (instantiate the cells)
    /// </summary>
    public void SetupGrid()
    {
        int nbrCols, nbrRows;
        computeGridSize(out nbrCols, out nbrRows);

        Cells = new List<Cell>();

        for (int c = 0; c < nbrCols; c++)
        {
            for (int r = 0; r < nbrRows; r++)
            {
                GameObject cellObj = GameObject.Instantiate(CellPrefab);
                cellObj.transform.SetParent(transform, false);

                int i = (c * nbrRows) + r;
                cellObj.name = "cell" + i;

                cellObj.GetComponent<DragDrop>().OverlayPanel = OverlayPanel;
                Cells.Add(cellObj.GetComponent<Cell>());
            }
        }
    }

    /// <summary>
    /// Get one random cell whose image has been visible for a while
    /// </summary>
    /// <returns></returns>
    public Cell getRandomCell()
    {
        // randomize
        Cells.Sort((a, b) => 1 - 2 * Random.Range(0, 1));

        Cell oldest = null;

        // get a random cell old enough
        foreach (var cell in Cells)
        {
            if (cell.IsInPreview) continue;
            if (cell.DisplayTime > MIN_DISPLAY_TIME) return cell;
            if (oldest == null || cell.DisplayTime > oldest.DisplayTime) oldest = cell;
        }

        // if none is old enough, take the oldest one
        return oldest;
    }

    // =================== private utils

    private void computeGridSize(out int nbrCols, out int nbrRows)
    {
        Rect rect = GetComponent<RectTransform>().rect;
        GridLayoutGroup glg = GetComponent<GridLayoutGroup>();
        nbrCols = Mathf.FloorToInt(rect.width / (glg.cellSize.x + (glg.spacing.x / 2)));
        nbrRows = Mathf.FloorToInt(rect.height / (glg.cellSize.y + (glg.spacing.y / 2)));
    }
}
