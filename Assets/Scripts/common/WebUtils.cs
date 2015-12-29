using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class WebUtils : MonoBehaviour
{

    public delegate void CallBack(byte[] data, string error);


    //------------------------------------------------------------

    public void Get(string url, CallBack cb)
    {
        StartCoroutine(RequestAsync(url, null, cb));
    }

    //------------------------------------------------------------

    public void Post(string url, string jsonData, CallBack cb)
    {
        StartCoroutine(RequestAsync(url, Encoding.UTF8.GetBytes(jsonData), cb));
    }

    // ------------------------------------------------------------------

    IEnumerator RequestAsync(string url, byte[] rawData, CallBack cb)
    {
        WWW www;
        if (rawData != null)
        {
            www = new WWW(url, rawData);
        }
        else
        {
            www = new WWW(url);
        }
        yield return www;

        if (www.error != null)
        {
            cb(null, www.error.ToString());
        }
        else
        {
            cb(www.bytes, null);
        }


        www.Dispose();
        www = null;
    }
   

}

