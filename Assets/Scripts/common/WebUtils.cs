using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class WebUtils : MonoBehaviour
{

    public string ServiceUrl { get { return "http://192.168.0.23:8680/";  } }

    private Dictionary<string,string> headers = new Dictionary<string, string>();
    public delegate void CallBack(byte[] data, string error);

    void Awake()
    {
        headers["Content-Type"] = "application/json";
    }


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
            www = new WWW(url, rawData, headers);
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

