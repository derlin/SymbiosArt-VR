using UnityEngine;
using System.Collections;

public class Spinner : MonoBehaviour
{


    public void Show()
    {
        gameObject.SetActive(true);
        iTween.RotateBy(gameObject, iTween.Hash("amount", new Vector3(0,0,1),
            "time", 1f,
            "easetype", "linear",
            "looptype", iTween.LoopType.loop));
    }

    public void Hide()
    {
        iTween.Stop(gameObject);
        gameObject.SetActive(false);
    }
}
