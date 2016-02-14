using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DetailedViewPanel : MonoBehaviour
{
    public Manager Manager;
    public RawImage FullImage;
    public GameObject LikeButtonComp, DislikeButtonComp;
    RectTransform parent;

    private float buttonSize;
    private Cell currentCell = null;

    void Start()
    {
        // fill parent
        parent = GetComponentInParent<RectTransform>();
        GetComponent<RectTransform>().sizeDelta = parent.sizeDelta;

        buttonSize = LikeButtonComp.GetComponent<RectTransform>().rect.height;
       
        // add callbacks

        LikeButtonComp.GetComponent<Button>().onClick.AddListener(() =>
       {
           currentCell.Image.State = derlin.symbiosart.datas.ImageState.LIKED;
           currentCell.Image = Manager.GetNextImage(currentCell.Image);
           hide();
       });

        DislikeButtonComp.GetComponent<Button>().onClick.AddListener(() =>
        {
            currentCell.Image.State = derlin.symbiosart.datas.ImageState.DISLIKED;
            currentCell.Image = Manager.GetNextImage(currentCell.Image);
            hide();
        });

        GetComponent<Button>().onClick.AddListener(hide);

        // hide

        hide();

    }


    public void show(Cell cell)
    {
        currentCell = cell;
        scaleAndSetTexture(currentCell.Image.Texture, parent.rect.width - buttonSize, parent.rect.height - buttonSize);
        float w = FullImage.gameObject.GetComponent<RectTransform>().rect.width;
        float h = FullImage.gameObject.GetComponent<RectTransform>().rect.height;

        // like button on bottom left
        LikeButtonComp.transform.localPosition = new Vector2(-w / 2, (-h + buttonSize) / 2);
        DislikeButtonComp.transform.localPosition = new Vector2(w / 2, (-h + buttonSize) / 2);

        //transform.position = Vector2.zero;
        gameObject.SetActive(true);
    }

    public void hide()
    {
        currentCell = null;
        gameObject.SetActive(false);
    }

    //------------------------------

    private void scaleAndSetTexture(Texture2D texture, float maxWidth, float maxHeight)
    {

        float h = texture.height, w = texture.width;
        float finalH = h, finalW = w;

        if (h > maxHeight || w > maxWidth) // if scaling necessary
        {
            if ((h / maxHeight) < (w / maxWidth))
            {
                finalW = maxWidth;
                finalH = Mathf.CeilToInt((h / w) * finalW);
            }
            else
            {
                finalH = maxHeight;
                finalW = (w / h) * finalH;
            }

        }


        FullImage.texture = texture;
        RectTransform rectTransform = FullImage.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(finalW, finalH);
    }
}
