using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class DropArea : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    Color32 iconHoverColor, iconNormalColor;

    /// <summary>
    /// is area liked or disliked
    /// </summary>
    public derlin.symbiosart.datas.ImageState StateWhenDropped;

    /// <summary>
    /// Child icon (like or dislike)
    /// </summary>
    public UnityEngine.UI.Image iconComponent;

    public Manager Manager;


    void Start()
    {
        Color32 orig = iconComponent.color;
        iconNormalColor = new Color32(orig.r, orig.g, orig.b, orig.a);
        iconHoverColor = new Color32(iconNormalColor.r, iconNormalColor.g, iconNormalColor.b, 255);
    }

    public void OnDrop(PointerEventData ptr)
    {
        var g = ptr.pointerDrag.GetComponent<Cell>();
        if (g != null)
        {
            Debug.Log("image " + g.name + " => " + name);
            g.Image.State = StateWhenDropped;
            g.Image = Manager.GetNextImage(g.Image);
        }
        OnPointerExit(ptr);
    }


    public void OnPointerEnter(PointerEventData ptr)
    {
        if (ptr != null && ptr.dragging) iconComponent.color = iconHoverColor;
    }

    public void OnPointerExit(PointerEventData data)
    {
        iconComponent.color = iconNormalColor;
    }
}
