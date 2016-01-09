using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class WebUtils : MonoBehaviour
{
    public static WebUtils INSTANCE { get; private set; }

    public static string ServiceUrl { get { return "http://error-418.com:8680/rest"; } }
    public static string ImagesUrl(int nbr){ return ServiceUrl + "/images/suggestions/" + nbr; }
    public static string UsersUrl{ get { return ServiceUrl + "/user/"; } }

    private Dictionary<string, string> headers = new Dictionary<string, string>();
    public delegate void CallBack(byte[] data, string error);

    void Awake()
    {
        headers["Content-Type"] = "application/json";
        INSTANCE = this;
    }

    public string GetUrl(string endPoint)
    {
        if (endPoint[0] == '/')
        {
            endPoint = endPoint.Substring(1);
        }

        return ServiceUrl + endPoint;
    }

    //------------------------------------------------------------

    public void Get(string url, CallBack cb)
    {
        StartCoroutine(RequestAsync(url, new byte[] { }, cb));
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
        if (rawData.Length > 0)
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

