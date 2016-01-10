using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using symbiosart.datas;

public class PreviewManager : MonoBehaviour
{
    public GameObject ScriptHolderObject;
    public GameObject RawImageObject;
    public GameObject ButtonLikeObject, ButtonDislikeObject;

    private RectTransform parentRect;
    private float buttonSize;
    private RectTransform thisRectTransform;
    private Manager profileMgr;

    private Cell visibleCell;
    public Cell VisibleCell
    {
        get { return visibleCell; }
        set { visibleCell = value; if(visibleCell != null) show(); }
    }

    void Start()
    {
        thisRectTransform = GetComponent<RectTransform>();
        profileMgr = ScriptHolderObject.GetComponent<Manager>();

        // fill parent
        parentRect = GetComponentInParent<RectTransform>();
        thisRectTransform.sizeDelta = parentRect.sizeDelta;

        buttonSize = ButtonLikeObject.GetComponent<RectTransform>().rect.height;
        Assert.IsTrue(buttonSize == ButtonDislikeObject.GetComponent<RectTransform>().rect.height);

        ButtonLikeObject.GetComponent<Button>().onClick.AddListener(() =>
        {
            VisibleCell.Image.state = ImageState.LIKED;
            next();
        });

        ButtonDislikeObject.GetComponent<Button>().onClick.AddListener(() =>
        {
            VisibleCell.Image.state = ImageState.DISLIKED;
            next();
        });

        GetComponent<Button>().onClick.AddListener(() =>
        {
            next();
        });

        hide();

    }

    private void show()
    {
        VisibleCell.IsInPreview = true;
        var image = VisibleCell.Image;

        scaleAndSetTexture(RawImageObject.GetComponent<RawImage>(),
            image.Texture, parentRect.rect.width - buttonSize, parentRect.rect.height - buttonSize);

        var rect = RawImageObject.GetComponent<RectTransform>().rect;
        float w = rect.width;
        float h = rect.height;

        // like button on bottom left
        ButtonLikeObject.transform.localPosition = new Vector2(-w / 2, (-h + buttonSize) / 2);
        ButtonDislikeObject.transform.localPosition = new Vector2(w / 2, (-h + buttonSize) / 2);

        var originalScale = thisRectTransform.localScale;
        thisRectTransform.localScale = Vector2.zero;

        iTween.ScaleTo(gameObject, iTween.Hash("scale", Vector3.one, "time", .3f, "easeType",
            iTween.EaseType.easeInExpo));
        gameObject.SetActive(true);
    }

    private void next()
    {
        hide();
        profileMgr.GetNextImage(VisibleCell.Image, newImg =>
        {
            VisibleCell.Image = newImg;
            VisibleCell.IsInPreview = false;
            VisibleCell = null;
        });
    }

    private void hide()
    {
        gameObject.SetActive(false);
        thisRectTransform.localScale = Vector3.one;
    }

    // ------------------------------------------

    private void scaleAndSetTexture(RawImage ri, Texture texture, float maxWidth, float maxHeight)
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


        ri.texture = texture;
        RectTransform rectTransform = ri.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(finalW, finalH);
    }

}
