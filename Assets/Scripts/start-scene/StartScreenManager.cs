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


    // list of available users (dict <id, name>)
    private List<Username> usernames;

    
    void Start()
    {
        StatusText.Working();
        StartCoroutine(getUsersList(setupDropdown));
        Dropdown.gameObject.SetActive(false);
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
        else // existing profile, get it from the API
        {
            GetComponentInChildren<Button>().enabled = false;
            var name = Dropdown.options[i].text;
            // get the profile id from the name
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
            // load the profile in a coroutine
            StartCoroutine(loadUser(id, userLoaded));
            StatusText.Working();
        }
    }

    // =================================================

    // coroutine callback
    void setupDropdown(string error)
    {
        if (error != null)
        {
            Debug.Log(error); // TODO: better handle the error ?
            StatusText.Done(error);
            return;
        }
        
        // ok: usernames is initialised, create the dropdown optioins
        Dropdown.gameObject.SetActive(true);
        Dropdown.options.Clear();
        Dropdown.options.Add(new Dropdown.OptionData("new..."));
        foreach (var u in usernames)
        {
            Dropdown.options.Add(new Dropdown.OptionData(u.Name));
        }
        Dropdown.RefreshShownValue();
        StatusText.Done();
        Debug.Log(JsonUtility.ToJson(usernames));
    }

    // coroutine callback
    void userLoaded(User user, string error)
    {
        if (error == null)
        {
            // user initialised, launch the main scene
            Debug.Log(user);
            SceneManager.LoadScene(Config.MAIN_SCENE_NAME);
        }
        else
        {
            StatusText.Done(error);
        }

        Debug.Log("user: " + JsonUtility.ToJson(user));
    }

    // =================================================

    // coroutine: load the list of profiles from the server
    IEnumerator getUsersList(Action<string> complete)
    {
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

    // coroutine: get the user profile from its id
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
    // data structure for the unmarshalling of the /user/all API call

    [Serializable]
    public class Username
    {
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}

