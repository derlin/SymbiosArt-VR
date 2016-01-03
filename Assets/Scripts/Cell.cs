using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class Cell : MonoBehaviour
{

    public DataDefinitions.Image Image { get { return image; } set { image = value; SetTexture(image.Texture); } }

    private DataDefinitions.Image image;
    RawImage rawImageComp;
    RectTransform rectTransformComp;
    private int cellW = -1, cellH = -1;


    void Awake()
    {
        rawImageComp = GetComponentInChildren<RawImage>();
        rawImageComp.enabled = false;
        rawImageComp.CrossFadeAlpha(0f, 0, false);
        rectTransformComp = GetComponentInChildren<RectTransform>();

    }

    private void computeCellSize()
    {
        var glg = GetComponentInParent<GridLayoutGroup>();
        Assert.IsNotNull(glg);

        var prect = glg.cellSize;
        cellW = Mathf.FloorToInt(prect.x);
        cellH = Mathf.FloorToInt(prect.y);
    }


    void SetTexture(Texture texture)
    {
        if (cellH < 0 || cellW < 0) computeCellSize();

        //Debug.Log(cellW + " " + cellH);
        float h = texture.height, w = texture.width;

        int finalH, finalW;
        if ((h / cellH) > (w / cellW))
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
        //Debug.Log(string.Format("h={0}, w={1}, fh={2}, fw={3}", h, w, finalH, finalW));
        StartCoroutine(FadeInTexture(texture));
    }

    IEnumerator FadeInTexture(Texture texture)
    {
        float dur = 0.5f;
        rawImageComp.enabled = true;
        if (rawImageComp.texture != null)
        {
            rawImageComp.CrossFadeAlpha(.1f, dur, false);
            yield return new WaitForSeconds(dur);
        }
        rawImageComp.texture = texture;
        rawImageComp.CrossFadeAlpha(1, dur, false);
    }

}
