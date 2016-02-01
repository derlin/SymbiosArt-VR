using UnityEngine;
using UnityEngine.UI;
using derlin.symbiosart.datas;
using derlin.symbiosart.constants;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

public class StartScreenManager : MonoBehaviour
{

    // components
    public Dropdown Dropdown;
    public Status StatusText;
    private float dots;

    // script state
    private enum State { LOADING_LIST, WAITING, LOADING_USER }
    private State state;

    // list of available users (dict <id, name>)
    private List<Username> usernames;


    void Start()
    {
        StatusText.Working();
        StartCoroutine(getUsersList(setupDropdown));
        
    }


    public void OnQuitButtonClick()
    {
        Application.Quit();
    }

    public void OnStartButtonClick()
    {
        int i = Dropdown.value;
        if (i == 0) // "New..." option
        {
            User.NewUser();
            SceneManager.LoadScene(Config.MAIN_SCENE_NAME);
        }
        else
        {
            GetComponentInChildren<Button>().enabled = false;
            var name = Dropdown.options[i].text;
            string id = null;
            foreach (var u in usernames)
            {
                if (name.Equals(u.Name))
                {
                    id = u.Id;
                    break;
                }
            }
            Debug.Assert(id != null);
            StartCoroutine(loadUser(id, userLoaded));
            StatusText.Working();
        }
    }

    // =================================================

    void setupDropdown(string error)
    {
        if (error != null)
        {
            Debug.Log(error); // TODO
            StatusText.Done(error);
            return;
        }

        var dropdown = GetComponentInChildren<Dropdown>();
        dropdown.options.Clear();
        dropdown.options.Add(new Dropdown.OptionData("new..."));
        foreach (var u in usernames)
        {
            dropdown.options.Add(new Dropdown.OptionData(u.Name));
        }
        dropdown.RefreshShownValue();
        StatusText.Done();
    }

    void userLoaded(User user, string error)
    {
        if (error == null)
        {
            Debug.Log(user);
            SceneManager.LoadScene(Config.MAIN_SCENE_NAME);
        }
        else
        {
            StatusText.Done(error);
        }
    }

    // =================================================

    IEnumerator getUsersList(Action<string> complete)
    {
        dots = 0;
        WWW www = new WWW(WebCs.UsersUrl("all"));
        yield return www;

        if (www.error != null)
        {
            complete(www.error);
        }
        else
        {
            try
            {
                usernames = JsonConvert.DeserializeObject<List<Username>>(www.text);
                complete(null);
            }
            catch (Exception e)
            {
                complete(e.Message);
            }
        }
    }

    // =================================================

    IEnumerator loadUser(string username, Action<User, string> complete)
    {
        WWW www = new WWW(WebCs.UsersUrl(username));
        yield return www;

        if (www.error != null)
        {
            complete(null, www.error);
        }
        else
        {
            try
            {
                var user = User.FromJson(www.text);
                complete(user, null);
            }
            catch (Exception e)
            {
                Debug.Log(e);
                complete(null, e.Message);
            }
        }
    }

    // ==========================================================

    public class Username
    {
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}

