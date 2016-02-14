using UnityEngine;
using System.Collections;

/// <summary>
/// Component making a gameobject spin. Useful
/// to imitate a loading gif with a static image.
/// </summary>
public class Spinner : MonoBehaviour
{
    // show the image and make it spin
    public void Show()
    {
        gameObject.SetActive(true);
        iTween.RotateBy(gameObject, iTween.Hash("amount", new Vector3(0,0,1),
            "time", 1f,
            "easetype", "linear",
            "looptype", iTween.LoopType.loop));
    }

    // stop the spinning and hide the image
    public void Hide()
    {
        iTween.Stop(gameObject);
        gameObject.SetActive(false);
    }
}
