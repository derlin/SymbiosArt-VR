using UnityEngine;
using System.Collections;

public class Spinner : MonoBehaviour
{


    public void Show()
    {
        iTween.RotateBy(gameObject, iTween.Hash("amount", new Vector3(0,0,1),
            "time", 1f,
            "easetype", "linear",
            "looptype", iTween.LoopType.loop));
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        iTween.Stop(gameObject);
        gameObject.SetActive(false);
    }
}
