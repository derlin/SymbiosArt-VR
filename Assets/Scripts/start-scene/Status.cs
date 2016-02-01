using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Status : MonoBehaviour
{

    public Spinner Spinner;
    public Text StatusText;

    public bool IsWorking { get { return Spinner.enabled;  } }

    // Use this for initialization
    void Start()
    {
        Spinner.Hide();
        StatusText.text = "";
    }

    public void Working()
    {
        StatusText.enabled = false;
        Spinner.Show();
    }

    public void Done()
    {
        Done("");
    }

    public void Done(string msg)
    {
        StatusText.text = msg;
        StatusText.enabled = true;
        Spinner.Hide();
    }
}
