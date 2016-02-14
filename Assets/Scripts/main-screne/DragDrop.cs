using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// component for implementing the drag functionality.
/// Upon drag, the RawImage component is cloned and becomes the dragged gameobject.
/// </summary>
public class DragDrop : MonoBehaviour
{
    // the overlay panel to show upon drag
    public GameObject OverlayPanel { get; set; }

    // the cloned RawImage component dragged
    private GameObject draggedObject;

    // for the animation on drop
    private Vector2 startDragPosition;

    // during drag: update the position
    public void OnDrag(BaseEventData eventData)
    {
        var pointerData = eventData as PointerEventData;
        if (pointerData == null) { return; }

        var currentPosition = draggedObject.transform.position;
        currentPosition.x += pointerData.delta.x;
        currentPosition.y += pointerData.delta.y;
        draggedObject.transform.position = currentPosition;
    }

    // start drag: clone the rawImage/create the dragged object
    public void BeginDrag(BaseEventData eventData)
    {
        OverlayPanel.SetActive(true);
        draggedObject = Instantiate(GetComponentInChildren<RawImage>().gameObject);
        draggedObject.transform.SetParent(OverlayPanel.transform);
        draggedObject.transform.SetAsFirstSibling();

        var pointerData = eventData as PointerEventData;
        if (pointerData == null) { return; }
        draggedObject.transform.position = pointerData.position;
        startDragPosition = draggedObject.transform.position;
    }

    // end drag: if dropped above a dropArea, fade animation. else,
    // make the dragged object go back to its start position before disappearing.
    public void EndDrag(BaseEventData eventData)
    {
        var pointerData = eventData as PointerEventData;
        var dropArea = pointerData.pointerCurrentRaycast.gameObject.GetComponent<DropArea>();

        if (dropArea != null) 
        {
            AnimateDropInArea();
        }
        else
        {
            AnimateDropOutsideArea();
        }
    }

    // animation on drop in a dropArea
    public void AnimateDropInArea()
    {
        iTween.RotateBy(draggedObject, iTween.Hash("amount", new Vector3(0f, 0.25f, 0f), // 1 = 360°
            "time", .3f, "onComplete", "cleanup", "onCompleteTarget", gameObject));
    }

    // animation on drop outside a dropArea
    public void AnimateDropOutsideArea()
    {
        iTween.MoveTo(draggedObject, iTween.Hash("x", startDragPosition.x, "y", startDragPosition.y,
        "easetype", iTween.EaseType.easeOutSine, "time", .3f, "onComplete", "cleanup", "onCompleteTarget", gameObject));
    }

    // destroy the dragged object on drop
    private void cleanup()
    {
        OverlayPanel.SetActive(false);
        if (draggedObject != null)
        {
            DestroyImmediate(draggedObject);
            draggedObject = null;
        }
    }
}
