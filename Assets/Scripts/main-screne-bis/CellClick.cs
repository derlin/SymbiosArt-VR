using UnityEngine;
using System.Collections;

public class CellClick : MonoBehaviour {

    public DetailedViewPanel DetailPanel;

    private Cell cellComp;

    void Start()
    {
        cellComp = GetComponent<Cell>();
    }

	public void onClick()
    {
        DetailPanel.show(cellComp);
    }
}
