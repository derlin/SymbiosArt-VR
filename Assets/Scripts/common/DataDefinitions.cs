using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LgOctEngine.CoreClasses;
using System;
using UnityEngine.Assertions;

public class DataDefinitions : MonoBehaviour
{


    // =============================================================

    [Serializable]
    public class Image
    {
        public Texture2D Texture { get; set; }
        public ImageMetas metas;
    }

    // =============================================================

    [Serializable]
    public class ImageMetas : LgJsonDictionary
    {
        // types
        public string Id { get { return GetValue<string>("id", ""); } set { SetValue<string>("id", value); } }

        public string Url { get { return GetValue<string>("url", ""); } set { SetValue<string>("url", value); } }

        public string Owner { get { return GetValue<string>("owner", ""); } set { SetValue<string>("owner", value); } }

        public LgJsonArray<string> Tags
        {
            get { return GetNode<LgJsonArray<string>>("tags"); }
            set { SetNode<LgJsonArray<string>>("tags", value); }
        }

        public string ToJson()
        {
            return this.Serialize();
        }

        public static ImageMetas FromJson(string json)
        {
            return LgJsonNode.CreateFromJsonString<ImageMetas>(json);
        }

        public static List<ImageMetas> FromJsonArray(string json)
        {
            var array = LgJsonNode.CreateFromJsonString<LgJsonArray<ImageMetas>>(json);
            List<ImageMetas> list = new List<ImageMetas>();
            for (int i = 0; i < array.Count; i++)
            {
                list.Add(array.GetAt(i));
            }

            return list;
        }
    }


    /*public class ImageMetas
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public List<string> Tags { get; set; }


        public ImageMetas() { }

        public ImageMetas(string id, string url, List<string> tags)
        {
            Id = id;
            Url = url;
            Tags = tags;
        }

        public ImageMetas(LgJsonDictionary dict)
        {
            Id = dict.GetValue<string>("id", "");
            Url = dict.GetValue<string>("url", "");
            var tagsnode = dict.GetNode<LgJsonArray<string>>("tags");
            if (tagsnode != null && tagsnode.Count > 0)
            {
                Tags = new List<string>(tagsnode.ToArray<string>());
            }
            else
            {
                Tags = new List<string>();
            }
        }


        public 


        public static ImageMetas FromJson(string json)
        {
            var dict = LgJsonNode.CreateFromJson<LgJsonDictionary>(json);
            return new ImageMetas(dict);
        }
    }*/
}
