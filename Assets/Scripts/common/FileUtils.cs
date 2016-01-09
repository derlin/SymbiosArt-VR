using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using System;

public class FileUtils : MonoBehaviour
{

    public string LikedImgDirectory { get { return Path.Combine(UserUtils.CurrentUser.BasePath, "liked"); } }
    public string DislikedImgDirectory { get { return Path.Combine(UserUtils.CurrentUser.BasePath, "disliked"); } }
    public string CachedDirectory { get { return Path.Combine(UserUtils.CurrentUser.BasePath, "cache"); } }

    // default encoding to utf8 (when converting string to byte on file write)
    private static Encoding encoding = Encoding.UTF8;

    public static Encoding FileEncoding { get { return encoding; } set { encoding = value; } }


    // ================================== instance methods

    public void CacheImage(DataDefinitions.Image image)
    {
        if (!Directory.Exists(CachedDirectory)) Directory.CreateDirectory(CachedDirectory); 
        SaveTextToFile(image.metas.ToJson(), Path.Combine(CachedDirectory, image.metas.Id + ".json"));
        SaveTextureToFile(image.Texture, Path.Combine(CachedDirectory, image.metas.Id + ".png"));
    }

    public void UncacheImage(DataDefinitions.Image image)
    {
        File.Delete(Path.Combine(CachedDirectory, image.metas.Id + ".json"));
        File.Delete(Path.Combine(CachedDirectory, image.metas.Id + ".png"));
    }

    public DataDefinitions.Image GetCachedImage(DataDefinitions.ImageMetas metas)
    {
        DataDefinitions.Image image = new DataDefinitions.Image();

        image.metas = metas;
        image.Texture = loadTextureFromDisc(Path.Combine(CachedDirectory, metas.Id + ".png"));
        return image;
    }


    public void MoveToLiked(DataDefinitions.Image image)
    {
        moveTo(image, LikedImgDirectory);
    }

    public void MoveToDisliked(DataDefinitions.Image image)
    {
        moveTo(image, DislikedImgDirectory);
    }

    private void moveTo(DataDefinitions.Image image, string to)
    {
        if (!Directory.Exists(to)) Directory.CreateDirectory(to);
        string imgFile = image.metas.Id + ".png";
        string metasFile = image.metas.Id + ".json";
        File.Move(Path.Combine(CachedDirectory, imgFile), Path.Combine(to, imgFile));
        File.Move(Path.Combine(CachedDirectory, metasFile), Path.Combine(to, metasFile));
    }
    // ================================== save utils

    public static void SaveTextureToFile(Texture2D texture, string filepath)
    {
        SaveToFile(texture.EncodeToJPG(), filepath);
    }

    public static void SaveTextToFile(string text, string filepath)
    {
        SaveToFile(FileEncoding.GetBytes(text), filepath);
    }

    public static void SaveToFile(byte[] bytes, string filepath)
    {
        try
        {
            File.WriteAllBytes(filepath, bytes);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    
    }

    // ================================== load utils

    public static Texture2D loadTextureFromDisc(string filepath)
    {
        Texture2D texture = null;
        byte[] fileData = loadFromDisc(filepath);

        if (fileData != null)
        {
            texture = new Texture2D(2, 2);
            texture.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }
        return texture;
    }

    public static string loadTextFromDisc(string filepath)
    {
        return FileEncoding.GetString(loadFromDisc(filepath));
    }

    public static byte[] loadFromDisc(string filepath)
    {
        byte[] fileData = null;

        if (File.Exists(filepath))
        {
            fileData = File.ReadAllBytes(filepath);
        }
        return fileData;
    }

}
