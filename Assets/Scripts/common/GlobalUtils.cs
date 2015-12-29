using UnityEngine;
using System.Collections;
using System.IO;

public class GlobalUtils : MonoBehaviour {

    


    public static void ScaleAndSetTexture(UnityEngine.UI.RawImage ri, Texture2D texture, float maxWidth, float maxHeight)
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
