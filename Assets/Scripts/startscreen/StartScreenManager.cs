using UnityEngine;
using UnityEngine.UI;
using LgOctEngine.CoreClasses;
using symbiosart.datas;
using symbiosart.constants;
using symbiosart.threading;
using System.Collections;
using System;
using System.Collections.Generic;

public class StartScreenManager : MonoBehaviour
{

    public static User User;
    LoadUserWorker loadUserWorker;

    private Dropdown dropdown;
    private Text textHolder;
    private float dots;

    void Start()
    {
        dropdown = GetComponentInChildren<Dropdown>();
        textHolder = GetComponentInChildren<Text>();
        StartCoroutine(getUsers(setupDropdown));
    }

    void Update()
    {
        if (loadUserWorker != null)
        {
            if (loadUserWorker.IsFinished())
            {
                User = loadUserWorker.User;
                Application.LoadLevel(1);
            }
            else
            {
                if (dots == 3) { textHolder.text = "Loading"; dots = 0; }
                else { textHolder.text += "."; dots++; }
            }
        }
    }

    void setupDropdown(List<string> userNames, string error)
    {
        if (error != null)
        {
            Debug.Log(error);
            return;
        }

        var dropdown = GetComponentInChildren<Dropdown>();
        dropdown.options.Clear();
        dropdown.options.Add(new Dropdown.OptionData("new..."));

        foreach (var name in userNames)
        {
            dropdown.options.Add(new Dropdown.OptionData(name));

        }
        dropdown.RefreshShownValue();
    }


    public void OnStartButtonClick()
    {
        int i = dropdown.value;
        if (i == 0)
        {
            User = new User();
            Application.LoadLevel(1);
        }
        else
        {
            GetComponentInChildren<Button>().enabled = false;
            var name = dropdown.options[i].text;
            loadUserWorker = new LoadUserWorker(name);
            loadUserWorker.Start();
            dots = 0;
        }
    }

    // =================================================
    IEnumerator getUsers(Action<List<string>, string> complete)
    {
        WWW www = new WWW(WebCs.UsersUrl("all"));
        yield return www;
        if (www.error != null)
        {
            complete(null, www.error);
        }
        else
        {
            var result = LgJsonNode.CreateFromJsonString<LgJsonArray<string>>(www.text);
            List<string> list = new List<string>();
            for (int i = 0; i < result.Count; i++)
            {
                list.Add(result[i]);
            }
            complete(list, null);

        }
    }
}

