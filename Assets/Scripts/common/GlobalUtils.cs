using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Text;

public class GlobalUtils : MonoBehaviour
{

    public static void ScaleAndSetTexture(UnityEngine.UI.RawImage ri, Texture texture, float maxWidth, float maxHeight)
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

    // ================================ json

    public static string DictToJson(Dictionary<string, string> dict)
    {
        if (dict.Count == 0) return "{}";

        StringBuilder sb = new StringBuilder("{");

        foreach (var entry in dict)
        {
            sb.Append(JsonStringEntry(entry.Key, entry.Value));
        }

        sb.Remove(sb.Length - 2, 2);
        sb.Append("}");
        return sb.ToString();
    }

    public static string DictToJson(Dictionary<string, int> dict)
    {
        if (dict.Count == 0) return "{}";
        StringBuilder sb = new StringBuilder("{");

        foreach (var entry in dict)
        {
                sb.Append(JsonObjectEntry(entry.Key, entry.Value));
        }

        sb.Remove(sb.Length - 2, 2);
        sb.Append("}");
        return sb.ToString();
    }

    public static string JsonStringEntry(string key, object value)
    {
        return string.Format("\"{0}\": \"{1}\", ", key, value);
    }

    public static string CollectionToJson(ICollection<string> coll)
    {
        if (coll.Count == 0) return "[]";
        StringBuilder sb = new StringBuilder("[");

        foreach (var entry in coll)
        {
            sb.Append("\"" + entry + "\",");
        }

        sb.Remove(sb.Length - 2, 2);
        sb.Append("]");
        return sb.ToString();
    }

    public static string JsonObjectEntry(string key, object value)
    {
        return string.Format("\"{0}\": {1}, ", key, value);
    }
}
