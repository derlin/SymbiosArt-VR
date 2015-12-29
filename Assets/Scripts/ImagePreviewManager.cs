using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class ImagePreviewManager : MonoBehaviour {

    public GameObject RawImageObject;
    public GameObject ButtonLikeObject, ButtonDislikeObject;

    private RectTransform parentRect;
    private float buttonSize;
    private DataDefinitions.Image image;
    private int cellIndex;

    public enum ActionType { HIDE, LIKE, DISLIKE, }
    public delegate void OnClose(int cellIndex, ActionType action);

    public OnClose OnCloseCallback { get; set; }

    public DataDefinitions.Image CurrentDisplayedImage {
        get { return image;  }
    }

    public int CurrentImageCellIndex { get { return cellIndex;  } }

    public void Show(DataDefinitions.Image img, int index)
    {
        image = img;
        cellIndex = index;
        show();
    }

    public void Hide()
    {
        hide();
    }


    void Start () {
        // fill parent
        parentRect = GetComponentInParent<RectTransform>();
        GetComponent<RectTransform>().sizeDelta = parentRect.sizeDelta;

        buttonSize = ButtonLikeObject.GetComponent<RectTransform>().rect.height;
        Assert.IsTrue(buttonSize == ButtonDislikeObject.GetComponent<RectTransform>().rect.height);

        ButtonLikeObject.GetComponent<Button>().onClick.AddListener(() => {
            hide(ActionType.LIKE);
        });

        ButtonDislikeObject.GetComponent<Button>().onClick.AddListener(() => {
            hide(ActionType.DISLIKE);
        });

        GetComponent<Button>().onClick.AddListener(() => {
            hide(ActionType.HIDE);
        });

        hide();
    }

    private void show()
    {
        GlobalUtils.ScaleAndSetTexture(RawImageObject.GetComponent<RawImage>(), 
            image.Texture, parentRect.rect.width - buttonSize, parentRect.rect.height - buttonSize);
        var rect = RawImageObject.GetComponent<RectTransform>().rect;
        float w = rect.width;
        float h = rect.height;

        // like button on bottom left
        ButtonLikeObject.transform.localPosition = new Vector2(-w / 2, (-h + buttonSize) / 2);
        ButtonDislikeObject.transform.localPosition = new Vector2(w / 2, (-h + buttonSize) / 2);

        gameObject.SetActive(true);
    }

    private void hide()
    {
        image = null;
        cellIndex = -1;
        gameObject.SetActive(false);
    }

    private void hide(ActionType action)
    {
        // before hide, since hide resets cellIndex
        if (OnCloseCallback != null) OnCloseCallback(cellIndex, action);
        hide();
    }
}
