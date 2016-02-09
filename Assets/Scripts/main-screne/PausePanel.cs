using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using derlin.symbiosart.datas;
using derlin.symbiosart.constants;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.IO;

public class PausePanel : MonoBehaviour
{

    public InputField NameInputField;
    public Button SaveButton, QuitButton, ExportLikedButton;
    public Text StatusText;

    derlin.symbiosart.threading.DownloadLikedImagesJob downloadImagesThread;
    bool saving, isShowing;

    void Start()
    {
        StatusText.gameObject.SetActive(false);

        QuitButton.onClick.AddListener(OnQuitButtonClicked);
        SaveButton.onClick.AddListener(OnSaveButtonClicked);
        ExportLikedButton.onClick.AddListener(OnExportButtonClicked);

        NameInputField.onValueChanged.AddListener((t) =>
            SaveButton.interactable = !(string.IsNullOrEmpty(t) || t == User.DEFAULT_NAME));
        Hide();
    }

    public void Toggle()
    {
        if (isShowing) Hide();
        else Show();
    }

    void Show()
    {
        gameObject.SetActive(true);
        saving = false;
        isShowing = true;
        NameInputField.text = User.CurrentUser.Name == User.DEFAULT_NAME ? "" : User.CurrentUser.Name;
        SaveButton.interactable = !string.IsNullOrEmpty(NameInputField.text);
        Time.timeScale = 0; // pause the game
        Debug.Log("pause panel show");
    }

    void Hide()
    {
        Time.timeScale = 1; // the game starts again
        isShowing = false;
        gameObject.SetActive(false);
        Debug.Log("pause panel hide");
    }

    void Update()
    {
        if (isShowing && saving)
        {
            if (StatusText.text.EndsWith("...")) StatusText.text.Replace("...", "");
            else StatusText.text += ".";
        }

        if(downloadImagesThread != null && downloadImagesThread.IsFinished())
        {
            StatusText.text = "Images exported.";
            downloadImagesThread = null;
        }
    }

    private void OnSaveButtonClicked()
    {
        if (saving) return;
        var username = NameInputField.text;
        var user = User.CurrentUser;

        StatusText.gameObject.SetActive(true);
        StatusText.text = "saving";

        StartCoroutine(saveUser(username, user));
    }

    public void OnExportButtonClicked()
    {
        if(downloadImagesThread != null)
        {
            StatusText.text = "Already exporting images...";
            return;
        }

        var path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures);
        //var path = EditorUtility.SaveFolderPanel("Save textures to directory", "", "");
        if (path.Length != 0)
        {
            path = Path.Combine(path, "symbiosart-liked-images");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            downloadImagesThread = new derlin.symbiosart.threading.DownloadLikedImagesJob(path);
            downloadImagesThread.Start();
            StatusText.text = "Exporting images...";
        }
    }

    public void OnQuitButtonClicked()
    {
        if (saving) return;
        Debug.Log(User.CurrentUser.TagsVectorAsJson());
        SceneManager.LoadScene(Config.START_SCENE_NAME);
    }

    // ============================
    IEnumerator saveUser(string username, User user)
    {
        saving = true;
        string oldName = user.Name;
        user.Name = username; // update user 

        // do the request
        var headers = new Dictionary<string, string>();
        headers["Content-Type"] = "application/json";
        WWW www = new WWW(WebCs.UsersUrl(), FileCs.DefaultEncoding.GetBytes(user.ToJson()), headers);
        yield return www;

        // check result
        saving = false;
        if (www.error != null)
        {
            user.Name = oldName; // revert
            Debug.Log(www.error);
            StatusText.text = "Error saving user : " + www.error;
        }
        else
        {
            user.Id = www.text;
            StatusText.text = "Saved.";
        }
    }
}
