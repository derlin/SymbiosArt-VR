using UnityEngine;
using System.Collections;
using symbiosart.utils;
using symbiosart.threading;
using symbiosart.constants;
using System.Collections.Generic;
using symbiosart.datas;
using System;
using UnityEngine.Assertions;
using Assets.Scripts.regular.threading;
using System.Net;

public class Manager : MonoBehaviour {

    public GameObject GridObject;
    private Grid grid;

    private ImagesProvider imagesProvider;

    private User user;

    private ReplaceRandomCellWorker replaceRandomCellWorker;

    List<ImageMetas> cachedImages = new List<ImageMetas>();

    List<string> seenIds = new List<string>();

    MetasQueue availMetas = new MetasQueue();

    // Use this for initialization
    void Start () {
        // create user
        user = StartScreenManager.User;
        if (user == null) user = new User();

        // setup grid
        grid = GridObject.GetComponent<Grid>();
        grid.SetupGrid();

        // load start images in background
        var nbr = grid.Cells.Count + Config.NBR_CACHED_IMAGES;
        imagesProvider = new ImagesProvider(user, grid.Cells.Count);
        StartCoroutine(setupCells());

        // setup bg workers
        replaceRandomCellWorker = new ReplaceRandomCellWorker(this);
    }

    void OnDestroy()
    {
        if (imagesProvider != null)
        {
            imagesProvider.Stop();
        }
    }


    public Image GetNextImage(Image oldImage)
    {
        if(oldImage.State == ImageState.LIKED) user.MarkAsLiked(oldImage.Metas);
        else if(oldImage.State == ImageState.DISLIKED) user.MarkAsDisliked(oldImage.Metas);
        return imagesProvider.NextImage;
    }
    

    // ==============

    IEnumerator setupCells()
    {
        foreach (var c in grid.Cells)
        {
            yield return new WaitWhile(() => imagesProvider.ImagesCount == 0);
            c.Image = imagesProvider.NextImage;
            Debug.Log(c.name + " " + c.Image);
        }
        
        Debug.Log("Setup done.");
        replaceRandomCellWorker.Running = true;
    }


    // ===================================================== 
    class ReplaceRandomCellWorker
    {
        private static readonly float REPLACE_FREQ = 10; // 30 seconds
        private Manager mgr;
        private Timer.TimerFunc timerFunc;
        public bool Running
        {
            get { return timerFunc.Running; }
            set { if (value) timerFunc.Start(); else timerFunc.Pause(); }
        }

        public ReplaceRandomCellWorker(Manager mgr)
        {
            this.mgr = mgr;
            timerFunc = new Timer.TimerFunc(replaceOne, REPLACE_FREQ, true);
        }

        public IEnumerator replaceOne()
        {
            Debug.Log("REPLACING ONE CELL");
            var cell = mgr.grid.getRandomCell();
            var newimg = mgr.GetNextImage(cell.Image);
            cell.Image = newimg;
            yield return null;
        }
    }

}
