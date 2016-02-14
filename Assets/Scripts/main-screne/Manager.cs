using UnityEngine;
using System.Collections;
using derlin.symbiosart.threading;
using derlin.symbiosart.constants;
using derlin.symbiosart.datas;


/// <summary>
/// Manager of the main scene
/// </summary>
public class Manager : MonoBehaviour {

    public PausePanel PausePanel; // the panel to show when ESC is pressed
    public Grid Grid; // the grid holding the cells
    private ImagesProvider imagesProvider; // the image provider (worker thread)
    private ReplaceRandomCellWorker replaceRandomCellWorker; 

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
        // show pause panel if ESC is pressed
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            PausePanel.Toggle();
        }
    }

    void OnDestroy()
    {
        // don't forget to explicitely kill the worker thread
        if (imagesProvider != null)
        {
            imagesProvider.Stop();
        }
    }

    // evaluate the oldImage and return a new one
    public Image GetNextImage(Image oldImage)
    {
        if(oldImage.State == ImageState.LIKED) User.CurrentUser.MarkAsLiked(oldImage.Metas);
        else if(oldImage.State == ImageState.DISLIKED) User.CurrentUser.MarkAsDisliked(oldImage.Metas);
        return imagesProvider.NextImage;
    }
    

    // ==============

    // coroutine called on start: assign an image to each cell
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

        // function called at each timer tick
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
