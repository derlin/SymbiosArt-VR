using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LgOctEngine.CoreClasses;
using System;
using UnityEngine.Assertions;

public class DataDefinitions : MonoBehaviour
{

    public enum ImageState
    {
        UNKNOWN,LIKED,DISLIKED
    };
    // =============================================================

    [Serializable]
    public class Image
    {
        public Texture2D Texture { get; set; }
        public ImageMetas metas;
        public ImageState state;

        public void SetTexture(byte[] rawData)
        {
            Texture = new Texture2D(0, 0);
            Texture.LoadImage(rawData);
        }
    }


    // =============================================================

    [Serializable]
    public class ImageMetas : LgJsonDictionary, IComparable<ImageMetas>
    {
        // types
        public string Id { get { return GetValue<string>("_id", ""); } set { SetValue<string>("_id", value); } }

        public string Url { get { return GetValue<string>("url", ""); } set { SetValue<string>("url", value); } }

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

        public int CompareTo(ImageMetas other)
        {
            return Id.CompareTo(other.Id);
        }
    }
}
