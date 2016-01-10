using symbiosart.constants;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace symbiosart.utils
{
    // ================================== save utils

    public class FileUtils
    {
        public static void SaveTextureToFile(UnityEngine.Texture2D texture, string filepath)
        {
            SaveToFile(texture.EncodeToJPG(), filepath);
        }

        public static void SaveTextToFile(string text, string filepath)
        {
            SaveToFile(FileCs.DefaultEncoding.GetBytes(text), filepath);
        }

        public static void SaveToFile(byte[] bytes, string filepath)
        {
            try
            {
                File.WriteAllBytes(filepath, bytes);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e);
            }

        }

        public static string CombineAndCreateDir(string path1, string path2)
        {
            string path = Path.Combine(path1, path2);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }


        // ================================== load utils

        public static UnityEngine.Texture2D loadTextureFromDisc(string filepath)
        {
            UnityEngine.Texture2D texture = null;
            byte[] fileData = loadFromDisc(filepath);

            if (fileData != null)
            {
                texture = new UnityEngine.Texture2D(2, 2);
                texture.LoadImage(fileData); //..this will auto-resize the texture dimensions.
            }
            return texture;
        }

        public static string loadTextFromDisc(string filepath)
        {
            return FileCs.DefaultEncoding.GetString(loadFromDisc(filepath));
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
    // ================================ json
    public class JsonUtils
    {

        public static string DictToJson(Dictionary<string, string> dict)
        {
            if (dict.Count == 0) return "{}";

            StringBuilder sb = new StringBuilder("{");

            foreach (var entry in dict)
            {
                sb.Append(JsonStringEntry(entry.Key, entry.Value));
            }

            sb.Remove(sb.Length - 2, 2);
            sb.Append("}");
            return sb.ToString();
        }

        public static string DictToJson(Dictionary<string, int> dict)
        {
            if (dict.Count == 0) return "{}";
            StringBuilder sb = new StringBuilder("{");

            foreach (var entry in dict)
            {
                sb.Append(JsonObjectEntry(entry.Key, entry.Value));
            }

            sb.Remove(sb.Length - 2, 2);
            sb.Append("}");
            return sb.ToString();
        }

        public static string JsonStringEntry(string key, object value)
        {
            return string.Format("\"{0}\": \"{1}\", ", key, value);
        }

        public static string CollectionToJson(ICollection<string> coll)
        {
            if (coll.Count == 0) return "[]";
            StringBuilder sb = new StringBuilder("[");

            foreach (var entry in coll)
            {
                sb.Append("\"" + entry + "\",");
            }

            sb.Remove(sb.Length - 2, 2);
            sb.Append("]");
            return sb.ToString();
        }

        public static string JsonObjectEntry(string key, object value)
        {
            return string.Format("\"{0}\": {1}, ", key, value);
        }
    }

    public class DisplayUtils
    {
        public static void ScaleAndSetTexture(UnityEngine.UI.RawImage ri, Texture texture, float maxWidth, float maxHeight)
        {

            float h = texture.height, w = texture.width;
            float finalH = h, finalW = w;

            if (h > maxHeight || w > maxWidth) // if scaling necessary
            {
                if ((h / maxHeight) < (w / maxWidth))
                {
                    finalW = maxWidth;
                    finalH = Mathf.CeilToInt((h / w) * finalW);
                }
                else
                {
                    finalH = maxHeight;
                    finalW = (w / h) * finalH;
                }

            }


            ri.texture = texture;
            RectTransform rectTransform = ri.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(finalW, finalH);
        }
    }
}
