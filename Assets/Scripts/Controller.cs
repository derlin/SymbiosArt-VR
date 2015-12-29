using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    private static Controller instance;
    public static Controller GetInstance { get { return instance;  } }

    public GameObject Grid, ImagePreviewManager;

    private UserUtils.User user;
    private Cell[] cells;
    private CacheManager cacheManager;
    private List<DataDefinitions.Image> startImages;

    void Awake()
    {
        instance = this;
    }

    // Use this for initialization
    void Start()
    {
        // create user and configure cache
        user = new UserUtils.User();
        cacheManager = GetComponent<CacheManager>();
        cacheManager.UserDirectory = user.Name;

        // create the images grid
        cells = Grid.GetComponent<CellsGenerator>().SetupGrid();

        // load static start images
        startImages = cacheManager.GetStartImages();
        Debug.Log(startImages.Count);

        // assign images to cells
        assignRandomStartImages();

        // add button actions
        registerCallbacks();
    }

    private void registerCallbacks()
    {

        var imagePreviewMgr = ImagePreviewManager.GetComponent<ImagePreviewManager>();
        imagePreviewMgr.OnCloseCallback = (i, a) => {
            if(a == global::ImagePreviewManager.ActionType.DISLIKE)
            {
                user.MarkAsDisliked(cells[i].Image.metas);
                nextImage(cells[i]);
            }
            else if(a == global::ImagePreviewManager.ActionType.LIKE)
            {
                user.MarkAsLiked(cells[i].Image.metas);
                nextImage(cells[i]);
            }

            Debug.Log(user.DumpTagsVector());
        };

        Grid.GetComponent<CellsGenerator>().CellBtnCallback = (i) => imagePreviewMgr.Show(cells[i].Image, i);
    }

    private void nextImage(Cell cell)
    {
        DataDefinitions.Image img;
        do
        {
            int i = Random.Range(0, startImages.Count);
            img = startImages[i];
        } while (user.isAlreadySeen(img.metas.Id));

        user.MarkAsSeen(img.metas.Id);
        cell.Image = img;
    }


    private void assignRandomStartImages()
    {
        Assert.IsTrue(cells.Length <= startImages.Count);

        foreach (var cell in cells)
        {
            DataDefinitions.Image img;
            do
            {
                int i = Random.Range(0, startImages.Count);
                img = startImages[i];
            } while (user.isAlreadySeen(img.metas.Id));

            user.MarkAsSeen(img.metas.Id);
            cell.Image = img;
        }
    }

}
