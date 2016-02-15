using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MyDropMe : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image Container;
    private Color normalColor, highlightColor;

    /// <summary>
    /// is area liked or disliked
    /// </summary>
    public derlin.symbiosart.datas.ImageState StateWhenDropped;

    /// <summary>
    /// Child icon (like or dislike)
    /// </summary>
    public UnityEngine.UI.Image iconComponent;

    public Manager Manager;

    public void Start()
    {
        if (Container != null)
        {
            normalColor = Container.color;
            highlightColor = new Color(normalColor.r, normalColor.g, normalColor.b, 1f);
        }
        iconComponent.enabled = false;
    }

    public void OnDrop(PointerEventData data)
    {
        Container.color = normalColor;
        Cell cell = getDropCell(data);

        if (cell != null)
        {
            Debug.Log("image " + cell.name + " => " + name);
            cell.Image.State = StateWhenDropped;
            cell.Image = Manager.GetNextImage(cell.Image);
        }
    }

    public void OnPointerEnter(PointerEventData data)
    {
        Debug.Log("ENTER "+ data.position);
        Cell cell = getDropCell(data);
        if (cell != null)
        {
            iconComponent.enabled = true;
            Container.color = highlightColor;
        }
        else
        {
            Debug.Log("no cell comp");
        }
    }

    public void OnPointerExit(PointerEventData data)
    {
        Debug.Log("EXIT " + data.position);
        Container.color = normalColor;
        iconComponent.enabled = false;
    }


    private Cell getDropCell(PointerEventData data)
    {
        var originalObj = data.pointerDrag;
        Debug.Log(originalObj);
        if (originalObj == null)
            return null;

        var cell = originalObj.GetComponent<Cell>();
        if (cell == null)
            return null;

        return cell;
    }
}
