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
    public Text StatusText;
    private float dots;

    // script state
    private enum State { LOADING_LIST, WAITING, LOADING_USER }
    private State state;

    // list of available users (dict <id, name>)
    private List<Username> usernames;


    void Start()
    {
        state = State.LOADING_LIST;
        StartCoroutine(getUsersList(setupDropdown));
    }

    void Update()
    {
        if (state != State.WAITING)
        {
            if (dots == 3)
            {
                StatusText.text = "Retrieving ";
                StatusText.text += (state == State.LOADING_LIST ? "usernames" : "user infos");
                dots = 0;
            }
            else
            {
                StatusText.text += ".";
                dots++;
            }
        }
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
        }
    }

    // =================================================

    void setupDropdown(string error)
    {
        if (error != null)
        {
            Debug.Log(error); // TODO
            StatusText.text = error;
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
            StatusText.text = error;
        }
    }

    // =================================================

    IEnumerator getUsersList(Action<string> complete)
    {
        dots = 0;
        state = State.LOADING_LIST;
        WWW www = new WWW(WebCs.UsersUrl("all"));
        yield return www;

        if (www.error != null)
        {
            state = State.WAITING;
            complete(www.error);
        }
        else
        {
            try
            {
                usernames = JsonConvert.DeserializeObject<List<Username>>(www.text);
                state = State.WAITING;
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
        state = State.LOADING_USER;
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
                state = State.WAITING;
                complete(user, null);
            }
            catch (Exception e)
            {
                Debug.Log(e);
                state = State.WAITING;
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

