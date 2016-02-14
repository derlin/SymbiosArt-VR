using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

/// <summary>
/// Component defining a DropArea, either like or dislike.
/// </summary>
public class DropArea : MonoBehaviour
{
    // colors of the (dis)like icon
    Color32 iconHoverColor, iconNormalColor;

    /// <summary>
    /// is area liked or disliked when dropped in this zone
    /// </summary>
    public derlin.symbiosart.datas.ImageState StateWhenDropped;

    /// <summary>
    /// Child icon (like or dislike)
    /// </summary>
    public UnityEngine.UI.Image iconComponent;

    // the manager
    public Manager Manager;


    void Start()
    {
        // initialise the colors
        Color32 orig = iconComponent.color;
        iconNormalColor = new Color32(orig.r, orig.g, orig.b, orig.a);
        iconHoverColor = new Color32(iconNormalColor.r, iconNormalColor.g, iconNormalColor.b, 255);
    }

    // on drop: update the image state get a new image from the manager
    public void OnDrop(BaseEventData eventData)
    {
        var ptr = eventData as PointerEventData;
        var cell = ptr.pointerDrag.GetComponent<Cell>();
        if (cell != null)
        {
            Debug.Log("image " + cell.name + " => " + name);
            cell.Image.State = StateWhenDropped;
            cell.Image = Manager.GetNextImage(cell.Image);
        }
        OnPtrLeave(eventData);
    }

    // pointer enter: opacify the image icon
    public void OnPtrEnter(BaseEventData eventData)
    {
        var ptr = eventData as PointerEventData;
        if (ptr != null && ptr.dragging) iconComponent.color = iconHoverColor;
    }

    // pointer leave: blur the image icon
    public void OnPtrLeave(BaseEventData eventData)
    {
        iconComponent.color = iconNormalColor;
    }
}
