using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using LgOctEngine.CoreClasses;

public class StartScreenManager : MonoBehaviour {

    WebUtils wu;
    Dropdown dropdown;

    void Start()
    {
        wu = FindObjectOfType<WebUtils>();
        wu.Get(wu.GetUrl("/rest/user/all"), setupDropdown);
        dropdown = GetComponentInChildren<Dropdown>();
    }

    void setupDropdown(byte[] data, string error)
    {
        if(error != null)
        {
            Debug.Log(error);
            return;
        }

        string json = FileUtils.FileEncoding.GetString(data);
        var userNames = LgJsonNode.CreateFromJsonString<LgJsonArray<string>>(json);

        var dropdown = GetComponentInChildren<Dropdown>();
        dropdown.options.Clear();
        dropdown.options.Add(new Dropdown.OptionData("new..."));

        for (int i = 0; i < userNames.Count; i++)
        {
            dropdown.options.Add(new Dropdown.OptionData(userNames[i]));
        }
        dropdown.RefreshShownValue();
    }


    public void OnStartButtonClick()
    {
        int i = dropdown.value;
        if(i == 0)
        {
            UserUtils.NewUser();
        }
        else
        {
            var name = dropdown.options[i].text;
            UserUtils.LoadUser(name);
        }
        Application.LoadLevel(1);
    }

}
