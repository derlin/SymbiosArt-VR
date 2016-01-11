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

    private StartWorker startJob;

    private UserManager userMgr;

    private float lastFetchTime;
    private FetchMetasThread fetchMetasThread;
    private ReplaceRandomCellWorker replaceRandomCellWorker;

    List<ImageMetas> cachedImages = new List<ImageMetas>();

    List<string> seenIds = new List<string>();

    MetasQueue availMetas = new MetasQueue();

    // Use this for initialization
    void Start () {
        // create user
        userMgr = new UserManager(StartScreenManager.User);

        // setup grid
        grid = GridObject.GetComponent<Grid>();
        grid.SetupGrid();

        // load start images in background
        var nbr = grid.Cells.Count + Config.NBR_CACHED_IMAGES;
        startJob = new StartWorker(userMgr.User, nbr);
        startJob.Start();
        StartCoroutine(setupCells());

        // setup bg workers
        replaceRandomCellWorker = new ReplaceRandomCellWorker(this);
        lastFetchTime = Time.time;
    }


    // Update is called once per frame
    void Update () {

        if(fetchMetasThread != null)
        {
            if (fetchMetasThread.IsFinished())
            {
                var cnt = 0;
                foreach (var m in fetchMetasThread.Metas)
                {
                    if (!seenIds.Contains(m.Id))
                    {
                        availMetas.Enqueue(m);
                        cnt++;
                    }
                }
                Debug.Log("fetched " + cnt + " new metas. ");
                fetchMetasThread = null;
                lastFetchTime = Time.time;
            }
        }
        else
        {
            if (Time.time - lastFetchTime >= Config.FETCH_METAS_INTERVAL)
            {
                fetchMetasThread = new FetchMetasThread(userMgr.User.TagsVectorAsJson);
                fetchMetasThread.Start();
            }
        }
    }


    public Image GetNextImage(Image oldImage)
    {
        if(oldImage.state == ImageState.LIKED) userMgr.Like(oldImage);
        else if(oldImage.state == ImageState.DISLIKED) userMgr.Dislike(oldImage);


        Assert.IsTrue(cachedImages.Count > 0);

        if (availMetas.Count > 0)
            new CacheOneThread(userMgr.User.CachePath, availMetas.Dequeue(), (m, e) => {
                if (e == null) cachedImages.Add(m);
            }).Start();

        var cached = cachedImages[cachedImages.Count - 1]; cachedImages.Remove(cached);
        return userMgr.GetCached(cached);
    }
    

    // ==============

    IEnumerator setupCells()
    {
        var cellIndex = 0;
        var cells = grid.Cells;

        while(!startJob.IsFinished() || startJob.Queue.Count > 0)
        {
            for(ImageMetas m = startJob.Queue.Dequeue(); m != null; m = startJob.Queue.Dequeue())
            {
                //Debug.Log("got one meta from bg thread " + Time.time);
                if (cellIndex < cells.Count)
                {
                    cells[cellIndex].Image = userMgr.GetCached(m);
                    cellIndex++;
                }
                else
                {
                    cachedImages.Add(m);
                }
            }
            //Debug.Log("yielding");
            yield return null; // new WaitForSeconds(1);
        }

        startJob = null;
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
