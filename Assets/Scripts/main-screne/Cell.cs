using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;
using derlin.symbiosart.datas;

/// <summary>
/// component managing one cell of the image grid.
/// </summary>
public class Cell : MonoBehaviour
{
    /// <summary>
    /// The image currently displayed, if any.
    /// </summary>
    public Image Image { get { return image; } set { image = value; SetTexture(image.Texture); } }

    /// <summary>
    /// The time (in seconds) the current image is being displayed.
    /// </summary>
    public float DisplayTime { get { return Time.time - lastUpdateTime; } }

    /// <summary>
    /// True if the current image is in interaction with the user (in preview or dragged)
    /// </summary>
    public bool IsInPreview { get; set; }

    // --------------------------------------------------------

    private static readonly float FADE_EFFECT_DURATION = 0.5f;

    // the image to display
    private Image image;

    // the image component 
    UnityEngine.UI.RawImage rawImageComp;

    // the rect transform of the raw image comp
    RectTransform rectTransformComp;

    // the cell width and height (constant)
    private int cellW = -1, cellH = -1;

    // last time the texture was changed
    private float lastUpdateTime;


    void Start()
    {
        // get a reference to the inner RawImage component and its transform
        rawImageComp = GetComponentInChildren<UnityEngine.UI.RawImage>();
        rectTransformComp = rawImageComp.GetComponent<RectTransform>();

        // make it invisible (no image)
        rawImageComp.enabled = false;
        rawImageComp.CrossFadeAlpha(0f, 0, false);
    }

    // get the exact size of this cell (used to scale the images later)
    private void computeCellSize()
    {
        var glg = GetComponentInParent<UnityEngine.UI.GridLayoutGroup>();
        var prect = glg.cellSize;
        cellW = Mathf.FloorToInt(prect.x);
        cellH = Mathf.FloorToInt(prect.y);
    }

    // set the texture in display, scaling it to fit the cell
    void SetTexture(Texture texture)
    {
        if (cellH < 0 || cellW < 0) computeCellSize();

        //Debug.Log(cellW + " " + cellH);
        float h = texture.height, w = texture.width;

        int finalH, finalW;
        if ((h / cellH) < (w / cellW))
        {
            finalW = cellW;
            finalH = Mathf.CeilToInt((h / w) * finalW);
        }
        else
        {
            finalH = cellH;
            finalW = Mathf.CeilToInt((w / h) * finalH);
        }

        rectTransformComp.sizeDelta = new Vector2(finalW, finalH);
        StartCoroutine(FadeInTexture(texture));
    }

    // fade out / fade in between texture changes
    IEnumerator FadeInTexture(Texture texture)
    {
        rawImageComp.enabled = true;
        if (rawImageComp.texture != null)
        {
            rawImageComp.CrossFadeAlpha(.1f, FADE_EFFECT_DURATION, false);
            yield return new WaitForSeconds(FADE_EFFECT_DURATION);
        }
        rawImageComp.texture = texture;
        lastUpdateTime = Time.time; // texture change !
        rawImageComp.CrossFadeAlpha(1, FADE_EFFECT_DURATION, false);
    }

}
