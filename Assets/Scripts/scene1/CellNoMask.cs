using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class CellNoMask : MonoBehaviour
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
        rectTransformComp = rawImageComp.GetComponent<RectTransform>();

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
