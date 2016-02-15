using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DragDrop : MonoBehaviour
{

    public GameObject OverlayPanel { get; set; }

    private GameObject draggedObject;

    private Vector3 startDragPosition;

    public void OnDrag(BaseEventData eventData)
    {
        var pointerData = eventData as PointerEventData;
        if (pointerData == null) { return; }


        var currentPosition = draggedObject.transform.position;
        currentPosition.x += pointerData.delta.x;
        currentPosition.y += pointerData.delta.y;
        draggedObject.transform.position = currentPosition;
    }


    public void BeginDrag(BaseEventData eventData)
    {
        OverlayPanel.SetActive(true);
        draggedObject = Instantiate(GetComponentInChildren<RawImage>().gameObject);
        draggedObject.transform.SetParent(OverlayPanel.transform);
        draggedObject.transform.SetAsFirstSibling();

        var pointerData = eventData as PointerEventData;
        if (pointerData == null) { return; }
        var rect = draggedObject.GetComponent<RectTransform>();
        //rect.sizeDelta = Vector3.one;
        rect.position = new Vector3(pointerData.position.x, pointerData.position.y, 0);
        //draggedObject.transform.position.Set(pointerData.position.x, pointerData.position.y, 0);
        //startDragPosition = draggedObject.transform.position; 
        startDragPosition = rect.position;
    }

    public void EndDrag(BaseEventData eventData)
    {
        cleanup();
        //var pointerData = eventData as PointerEventData;
        //var dropArea = pointerData.pointerCurrentRaycast.gameObject.GetComponent<DropArea>();

        //if (dropArea != null) 
        //{
        //    AnimateDropInArea();
        //}
        //else
        //{
        //    AnimateDropOutsideArea();
        //}
    }

    public void AnimateDropInArea()
    {
        iTween.RotateBy(draggedObject, iTween.Hash("amount", new Vector3(0f, 0.25f, 0f), // 1 = 360°
            "time", .3f, "onComplete", "cleanup", "onCompleteTarget", gameObject));
    }


    public void AnimateDropOutsideArea()
    {
        iTween.MoveTo(draggedObject, iTween.Hash("x", startDragPosition.x, "y", startDragPosition.y,
        "easetype", iTween.EaseType.easeOutSine, "time", .3f, "onComplete", "cleanup", "onCompleteTarget", gameObject));
    }

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
