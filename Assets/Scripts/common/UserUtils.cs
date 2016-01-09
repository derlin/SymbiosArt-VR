﻿using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;
using System.Collections.Generic;
using LgOctEngine.CoreClasses;
using System.Text;
using System.IO;

public class UserUtils : MonoBehaviour
{

    public static User CurrentUser { get; set; }

    void Start()
    {
        Debug.Log("USER '" + CurrentUser + "'");
    }

    public static void NewUser()
    {
        CurrentUser = new User();
    }

    public static void LoadUser(string name)
    {
        CurrentUser = new User();
        CurrentUser.Name = name;
        var www = WebUtils.INSTANCE;

        www.Get(www.GetUrl("/user/" + name), (b, e) =>
        {
            if (e != null)
            {
                Debug.Log("arg!!! error loading user " + name + " : " + e);
                Application.LoadLevel(0);
            }
            else
            {
                CurrentUser = User.FromJson(FileUtils.FileEncoding.GetString(b));
                Debug.Log("user vector: " + CurrentUser.TagsVector);
            }

        });


    }

    public class User
    {
        private Dictionary<string, int> tagsVector = new Dictionary<string, int>();
        private ICollection<string> imagesLiked = new HashSet<string>();
        private ICollection<string> imagesDisliked = new HashSet<string>();

        public string BasePath { get { return Path.Combine(Application.persistentDataPath, Name); } }
        public string Name { get; set; }
        public Dictionary<string, int> TagsVector { get { return tagsVector; } }
        public string TagsVectorAsJson { get { return GlobalUtils.DictToJson(tagsVector); } }
        public ICollection<string> ImageLiked { get { return imagesLiked; } }
        public ICollection<string> ImageDisliked { get { return imagesDisliked; } }

        public User()
        {
            Name = "New";
        }

        public static User FromJson(string json)
        {
            User user = new User();
            var dict = LgJsonNode.CreateFromJsonString<LgJsonDictionary>(json);
            var name = dict.GetValue<string>("name", null);
            var tv = dict.GetValue<Dictionary<string, int>>("tagsvector", null);
            if (tv == null || string.IsNullOrEmpty(name))
            {
                Debug.Log("error deserializing user");
                return user;
            }

            user.Name = name;
            user.tagsVector = tv;

            return user;
        }


        public string ToJson()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("{");
            builder.Append(GlobalUtils.JsonStringEntry("name", Name));
            builder.Append(GlobalUtils.JsonObjectEntry("tags_vector",
                GlobalUtils.DictToJson(tagsVector)));
            builder.Append(GlobalUtils.JsonObjectEntry("liked_ids",
                GlobalUtils.CollectionToJson(ImageLiked)));
            builder.Append(GlobalUtils.JsonObjectEntry("disliked_ids",
                GlobalUtils.CollectionToJson(imagesDisliked)));
            builder.Append("}");

            return builder.ToString();
        }

        protected void updateTagsVector(LgJsonArray<string> tags, bool liked)
        {
            int wheight = liked ? 1 : -1;
            for (int i = 0; i < tags.Count; i++)
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
                s += string.Format("    {0}: {1}\n", entry.Key, entry.Value);
            }
            return s;
        }
    }

}
