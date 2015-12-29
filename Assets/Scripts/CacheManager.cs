using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CacheManager : MonoBehaviour
{

    public string BasePath;
    public string UserDirectory = "User";
    private string userprofileFile = "User.json";

    public string ImgDir
    {
        get
        {
            return Path.Combine(basedir, "img");
        }
    }
    public string MetaDir
    {
        get
        {
            return Path.Combine(basedir, "meta");
        }
    }

    private string basedir;

    // ---------------------------------------------------------

    void Start()
    {
        if (string.IsNullOrEmpty(BasePath)) BasePath = Application.persistentDataPath;
        basedir = Path.Combine(BasePath, UserDirectory);
        if (!Directory.Exists(basedir))
        {
            Directory.CreateDirectory(basedir);
            Directory.CreateDirectory(ImgDir);
            Directory.CreateDirectory(MetaDir);
            Debug.Log(basedir + " : directories created.");
        }
        else
        {
            Debug.Log(basedir + " : directories exist.");

        }

    }


    // -------------------------------------------------

    public List<DataDefinitions.Image> GetStartImages()
    {
        List<DataDefinitions.Image> images = new List<DataDefinitions.Image>();

        TextAsset asset = (TextAsset)Resources.Load("StartPictures/metadata", typeof(TextAsset));
        foreach (var line in Regex.Split(asset.text, "\n|\r|\r\n"))
        {
            if (string.IsNullOrEmpty(line)) continue;
            DataDefinitions.Image img = new DataDefinitions.Image();
            img.metas = DataDefinitions.ImageMetas.FromJson(line);
            img.Texture = (Texture2D)Resources.Load("StartPictures/img/im" + img.metas.Id, typeof(Texture2D));
            images.Add(img);
        }

        return images;
    }
    // -------------------------------------------------


    public List<string> GetCachedImages()
    {
        string[] files = Directory.GetFiles(ImgDir);
        return new List<string>(files)
            .ConvertAll<string>(s => s.Substring(0, s.LastIndexOf(".")));
    }

    public void SaveImageToFile(DataDefinitions.Image image)
    {
        FileUtils.SaveTextureToFile(image.Texture, image.metas.Id + ".jpg");
        FileUtils.SaveTextToFile(image.metas.ToJson(), image.metas.Id + ".jpg");
    }



    public DataDefinitions.Image LoadImageFromDisc(string id)
    {
        DataDefinitions.Image img = new DataDefinitions.Image();
        string jsonMetas = FileUtils.loadTextFromDisc(Path.Combine(MetaDir, id + ".json"));
        img.metas = DataDefinitions.ImageMetas.FromJson(jsonMetas);
        img.Texture = FileUtils.loadTextureFromDisc(Path.Combine(ImgDir, id + ".jpg"));

        return img;
    }
}
