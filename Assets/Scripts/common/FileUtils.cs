using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using System;

public class FileUtils : MonoBehaviour
{

    // default encoding to utf8 (when converting string to byte on file write)
    private static Encoding encoding = Encoding.UTF8;
    public static Encoding FileEncoding { get { return encoding; } set { encoding = value; } }

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
