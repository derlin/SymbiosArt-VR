using UnityEngine;
using System.Collections;
using symbiosart.utils;
using symbiosart.threading;
using symbiosart.constants;
using System.Collections.Generic;
using symbiosart.datas;
using System;

public class Manager : MonoBehaviour {

    public GameObject GridObject;
    private Grid grid;

    private StartWorker startJob;

    private UserManager userMgr;

    List<ImageMetas> cachedImages = new List<ImageMetas>();

    MetasQueue availMetas = new MetasQueue();

    // Use this for initialization
    void Start () {
        // create user
        userMgr = new UserManager(StartScreenManager.User);

        // setup grid
        grid = GridObject.GetComponent<Grid>();
        grid.SetupGrid();

        // setup in background
        var nbr = grid.Cells.Count + Config.NBR_CACHED_IMAGES;
        startJob = new StartWorker(userMgr.User, nbr);
        startJob.Start();
    }
	
	// Update is called once per frame
	void Update () {
	    if(startJob != null)
        {
            if (startJob.IsFinished())
            {
                availMetas.AddRange(startJob.Metas);
                startJob = null;
                StartCoroutine(setupCells());
            }
        }
	}


    public void GetNextImage(Image oldImage, Action<Image> complete)
    {

    }
    

    // ==============

    IEnumerator setupCells()
    {
        foreach (var c in grid.Cells)
        {
            var m = availMetas.Dequeue();
            c.Image = userMgr.GetCached(m);
            yield return c;
        }
    }
   
}
