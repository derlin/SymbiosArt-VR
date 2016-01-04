using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CellsGenerator : MonoBehaviour {

    public GameObject CellPrefab;

    public delegate void CellButtonCallBack(int i);

    public CellButtonCallBack CellBtnCallback { get; set; }

    // Use this for initialization
    public Cell[] SetupGrid()
    {

        int nbrCols, nbrRows;
        computeGridSize(out nbrCols, out nbrRows);

        Cell[] cells = new Cell[nbrRows * nbrCols];

        for (int c = 0; c < nbrCols; c++)
        {
            for (int r = 0; r < nbrRows; r++)
            {
                int i = (c * nbrRows) + r;
                GameObject cell = GameObject.Instantiate(CellPrefab);
                cell.name = "cell" + i;
                cell.transform.SetParent(transform, false);

                cell.GetComponent<Button>().onClick.AddListener(() => {
                    Debug.Log("cell " + i + " pressed.");
                    if (CellBtnCallback != null) { CellBtnCallback(i); }
                } );

                cells[i] = cell.GetComponent<Cell>();
            }
        }

        return cells;
    }


    private void computeGridSize(out int nbrCols, out int nbrRows)
    {
        Rect rect = GetComponent<RectTransform>().rect;
        GridLayoutGroup glg = GetComponent<GridLayoutGroup>();
        nbrCols = Mathf.FloorToInt(rect.width / (glg.cellSize.x + (glg.spacing.x / 2)));
        nbrRows = Mathf.FloorToInt(rect.height / (glg.cellSize.y + (glg.spacing.y / 2)));
        //Debug.Log("rows: " + nbrRows + ", cols: " + nbrCols);
    }
}
