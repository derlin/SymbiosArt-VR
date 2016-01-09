using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;

public class ProfileManager : MonoBehaviour
{
    public GameObject GridObject;
    Grid grid;
    private int cellSetup = 0;

    static readonly int NBR_CACHED_IMAGES = 3;

    List<string> seenIds = new List<string>();

    List<DataDefinitions.ImageMetas> cachedImages = new List<DataDefinitions.ImageMetas>();
    MetasQueue availMetas = new MetasQueue();

    FetchMetasWorker fetchMetasWorker;
    ReplaceRandomCellWorker replaceCellWorker;
    FileUtils fileUtils;
    WebUtils webUtils;


    // Use this for initialization
    void Start()
    {
        // setup user
        fileUtils = GetComponent<FileUtils>();

        if(UserUtils.CurrentUser == null) // TODO
        {
            UserUtils.CurrentUser = new UserUtils.User();
        }

        // setup web
        webUtils = GetComponent<WebUtils>();

        // setup grid
        grid = GridObject.GetComponent<Grid>();
        grid.SetupGrid();

        // fetch metas + launch the worker
        replaceCellWorker = new ReplaceRandomCellWorker(this);
        fetchMetasWorker = new FetchMetasWorker(this);

        // fetch start images to populate the availMetas
        StartCoroutine(fetchMetasWorker.fetchMetas(100, (bs, err) => {

            cellSetup = grid.Cells.Count - 1;
            for (int i = NBR_CACHED_IMAGES + grid.Cells.Count; i >= 0; i--)
            {
                StartCoroutine(cacheOneImage());
            }
            fetchMetasWorker.Running = true;
            replaceCellWorker.Running = true;
        }));



    }

    // Update is called once per frame
    void Update()
    {

    }

    // ===================================================== 

    public DataDefinitions.Image GetNextImage(DataDefinitions.Image oldImage)
    {
        if (oldImage != null) handleOldImage(oldImage);
        Assert.IsTrue(cachedImages.Count > 0);
        int index = Random.Range(0, cachedImages.Count);
        var next = cachedImages[index];
        cachedImages.Remove(next);
        StartCoroutine(cacheOneImage());
        return fileUtils.GetCachedImage(next);
    }

    private void handleOldImage(DataDefinitions.Image image)
    {
        switch (image.state)
        {
            case DataDefinitions.ImageState.LIKED:
                UserUtils.CurrentUser.MarkAsLiked(image.metas);
                fileUtils.MoveToLiked(image);
                break;

            case DataDefinitions.ImageState.DISLIKED:
                UserUtils.CurrentUser.MarkAsDisliked(image.metas);
                fileUtils.MoveToDisliked(image);
                break;

            default:
                fileUtils.UncacheImage(image);
                break;
        }
    }


    IEnumerator cacheOneImage()
    {
        Debug.Log("cache one image");

        var metas = availMetas.Dequeue();

        Debug.Log("getting image " + metas.Url);
        webUtils.Get(metas.Url, (bs, err) => {
            if (err != null)
            {
                // error: try again
                Debug.LogError("failed to fetch image!!!" + err);
                cacheOneImage();
            }
            else
            {
                // ok: save the new image
                DataDefinitions.Image img = new DataDefinitions.Image();
                img.metas = metas;
                img.SetTexture(bs);
                Debug.Log("caching image : " + img.metas.Id + " " + img.Texture);
                fileUtils.CacheImage(img);
                if (cellSetup >= 0)
                {
                    grid.Cells[cellSetup--].Image = img;
                }
                else
                {
                    cachedImages.Add(img.metas);
                }
            }
            
        });
        yield return null;
    }
    // ===================================================== 
    class FetchMetasWorker
    {
        private static readonly float FETCH_INTERVAL = 20;
        private static readonly int FETCH_NBR = 5;
        private ProfileManager mgr;
        private Timer.TimerFunc timerfunc;
        private WebUtils.CallBack cb = null;

        public bool Running {
            get { return timerfunc.Running;  }
            set { if (value) timerfunc.Start(); else timerfunc.Pause(); }
        }

        public FetchMetasWorker(ProfileManager pm)
        {
            mgr = pm;
            timerfunc = new Timer.TimerFunc(fetchMetas, FETCH_INTERVAL, true);
        }

        public IEnumerator fetchMetas()
        {

            string json = UserUtils.CurrentUser.TagsVectorAsJson;
            mgr.webUtils.Post(WebUtils.ImagesUrl(FETCH_NBR), json, webCallback);
            yield return null; // TODO
        }

        public IEnumerator fetchMetas(int nbr, WebUtils.CallBack cb)
        {
            this.cb = cb;
            string json = UserUtils.CurrentUser.TagsVectorAsJson;
            mgr.webUtils.Post(WebUtils.ImagesUrl(nbr), json, webCallback);
            yield return null; // TODO
        }

        public void webCallback(byte[] bs, string err)
        {

            if (err != null)
            {
                Debug.LogError("ERROR FETCHING METAS: " + err);
                return;
            }

            List<DataDefinitions.ImageMetas> metas = DataDefinitions.ImageMetas.FromJsonArray(FileUtils.FileEncoding.GetString(bs));
            int cnt = 0;

            foreach (var meta in metas)
            {
                if (!mgr.seenIds.Contains(meta.Id))
                {
                    mgr.seenIds.Add(meta.Id);
                    mgr.availMetas.Enqueue(meta);
                    cnt++;
                }
            }

            if(cb != null)
            {
                cb(bs, err);
                cb = null;
            }

        }
    }// end class


    // ===================================================== 
    class ReplaceRandomCellWorker
    {
        private static readonly float REPLACE_FREQ = 10; // 30 seconds
        private ProfileManager mgr;
        private Timer.TimerFunc timerFunc;
        public bool Running
        {
            get { return timerFunc.Running; }
            set { if (value) timerFunc.Start(); else timerFunc.Pause(); }
        }

        public ReplaceRandomCellWorker(ProfileManager mgr)
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
    // ===================================================== 

    class MetasQueue : List<DataDefinitions.ImageMetas>
    {
        private static readonly int MAX_CAPACITY = 100;

        public DataDefinitions.ImageMetas Dequeue()
        {
            if (Count == 0) return null;

            var metas = this[Count - 1];
            RemoveAt(Count - 1);
            return metas;
        }

        public bool Enqueue(DataDefinitions.ImageMetas metas)
        {
            if (Contains(metas)) return false;
            Add(metas);
            if (Count > MAX_CAPACITY) RemoveAt(0);
            return true;
        }

        public void EnqueueAll(ICollection<DataDefinitions.ImageMetas> coll)
        {
            foreach (var metas in coll)
            {
                Enqueue(metas);
            }
        }
    }
}
