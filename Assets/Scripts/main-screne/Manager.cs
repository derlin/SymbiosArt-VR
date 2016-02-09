using UnityEngine;
using System.Collections;
using derlin.symbiosart.threading;
using derlin.symbiosart.constants;
using derlin.symbiosart.datas;

public class Manager : MonoBehaviour {

    public PausePanel PausePanel;

    public Grid Grid;

    private ImagesProvider imagesProvider;

    private ReplaceRandomCellWorker replaceRandomCellWorker;

    // Use this for initialization
    void Start () {

        // setup grid
        Grid.SetupGrid();

        // load start images in background
        imagesProvider = new ImagesProvider(Grid.Cells.Count);
        StartCoroutine(setupCells());
        
        // setup bg workers
        replaceRandomCellWorker = new ReplaceRandomCellWorker(this);
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            Debug.Log("manager escape key");
            PausePanel.Toggle();
        }
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
        if(oldImage.State == ImageState.LIKED) User.CurrentUser.MarkAsLiked(oldImage.Metas);
        else if(oldImage.State == ImageState.DISLIKED) User.CurrentUser.MarkAsDisliked(oldImage.Metas);
        return imagesProvider.NextImage;
    }
    

    // ==============

    IEnumerator setupCells()
    {
        foreach (var c in Grid.Cells)
        {
            yield return new WaitWhile(() => imagesProvider.ImagesCount == 0);
            c.Image = imagesProvider.NextImage;
        }
        
        Debug.Log("Setup done.");
        replaceRandomCellWorker.Running = true;
    }


    // ===================================================== 
    class ReplaceRandomCellWorker
    {
        
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
            timerFunc = new Timer.TimerFunc(replaceOne, Config.REPLACE_CELL_FREQ, true);
        }

        public IEnumerator replaceOne()
        {
            Debug.Log("REPLACING ONE CELL");
            var cell = mgr.Grid.getRandomCell();
            var newimg = mgr.GetNextImage(cell.Image);
            cell.Image = newimg;
            yield return null;
        }
    }

}
