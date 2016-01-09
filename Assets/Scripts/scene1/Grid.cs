using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class Grid : MonoBehaviour {

    public GameObject ImagePreviewMgrObject;

    public GameObject CellPrefab;

    private List<Cell> cells = new List<Cell>();

    public List<Cell> Cells { get {return cells; } }

    private static readonly float MIN_DISPLAY_TIME = 60; // 1 minute
    // Use this for initialization
    public void SetupGrid()
    {
        PreviewManager mgr = ImagePreviewMgrObject.GetComponent<PreviewManager>();
        int nbrCols, nbrRows;
        computeGridSize(out nbrCols, out nbrRows);

        for (int c = 0; c < nbrCols; c++)
        {
            for (int r = 0; r < nbrRows; r++)
            {
                int i = (c * nbrRows) + r;
                GameObject cellObj = GameObject.Instantiate(CellPrefab);
                cellObj.name = "cell" + i;
                cellObj.transform.SetParent(transform, false);

                var cell = cellObj.GetComponent<Cell>();
                cellObj.GetComponent<Button>().onClick.AddListener(() => mgr.VisibleCell = cell);
                cells.Add(cell);
            }
        }
    }


    public Cell getRandomCell()
    {
        // randomize
        cells.Sort((a, b) => 1 - 2 * Random.Range(0, 1));

        Cell oldest = null;

        // get a random cell old enough
        foreach (var cell in cells)
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
        //Debug.Log("rows: " + nbrRows + ", cols: " + nbrCols);
    }
}
