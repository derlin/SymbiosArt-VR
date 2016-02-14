using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Component for user feedback.
/// Shows a spinner ("loading gif") when working and 
/// a potential error text on done.
/// </summary>
public class Status : MonoBehaviour
{
    // components
    public Spinner Spinner; // the Spinner 
    public Text StatusText; // the UI.text holding the error text

    public bool IsWorking { get { return Spinner.enabled;  } }

    // hide everything
    void Start()
    {
        Spinner.Hide();
        StatusText.text = "";
    }

    // show the spinner, hide the text
    public void Working()
    {
        StatusText.enabled = false;
        Spinner.Show();
    }

    // hide the spinner and the text
    public void Done()
    {
        Done("");
    }

    // hide the spinner and show an error text
    public void Done(string msg)
    {
        StatusText.text = msg;
        StatusText.enabled = true;
        Spinner.Hide();
    }
}
