using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LgOctEngine.CoreClasses;
using System;
using UnityEngine.Assertions;

public class DataDefinitions : MonoBehaviour
{
    // =============================================================
    public class User
    {
        private Dictionary<string, int> tagsVector = new Dictionary<string, int>();
        private ICollection<string> imagesLiked = new HashSet<string>();
        private ICollection<string> imagesDisliked = new HashSet<string>();
        private Dictionary<string, bool> imagesSeen = new Dictionary<string, bool>();

        public string Name { get; set; }
        public Dictionary<string, int> TagsVector { get { return tagsVector; } }
        public int ImagesSeenCount { get { return imagesSeen.Count; } }
        public ICollection<string> ImageLiked { get { return imagesLiked; } }
        public ICollection<string> ImageDisliked { get { return imagesDisliked; } }

        public User()
        {
            Name = "User";
        }


        protected void updateTagsVector(string[] tags, bool liked)
        {
            int wheight = liked ? 1 : -1;
            foreach (var tag in tags)
            {
                if (tagsVector.ContainsKey(tag))
                {
                    tagsVector[tag] += wheight;
                }
                else
                {
                    tagsVector[tag] = wheight;
                }
            }
        }

        protected void updateTagsVector(LgJsonArray<string> tags, bool liked)
        {
            int wheight = liked ? 1 : -1;
            for(int i = 0; i < tags.Count; i++)
            {
                var tag = tags.GetAt(i);
                if (tagsVector.ContainsKey(tag))
                {
                    tagsVector[tag] += wheight;
                }
                else
                {
                    tagsVector[tag] = wheight;
                }
            }
        }

        public bool isAlreadySeen(string id)
        {
            return imagesSeen.ContainsKey(id);
        }

        public void MarkAsSeen(string id)
        {
            imagesSeen[id] = true;
        }

        public void MarkAsLiked(DataDefinitions.ImageMetas image)
        {
            Assert.IsFalse(imagesLiked.Contains(image.Id));
            updateTagsVector(image.Tags, true);
            imagesLiked.Add(image.Id);
        }

        public void MarkAsDisliked(DataDefinitions.ImageMetas image)
        {
            Assert.IsFalse(imagesDisliked.Contains(image.Id));
            updateTagsVector(image.Tags, false);
            imagesDisliked.Add(image.Id);
        }

        public string DumpTagsVector()
        {
            string s = "TagsVector:\n";
            foreach (var entry in tagsVector)
            {
                s += String.Format("    {0}: {1}\n", entry.Key, entry.Value);
            }
            return s;
        }
    }
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
            return LgJsonNode.ConvertToString(this);

        }

        public static ImageMetas FromJson(string json)
        {
            return LgJsonNode.CreateFromJsonString<ImageMetas>(json);
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
