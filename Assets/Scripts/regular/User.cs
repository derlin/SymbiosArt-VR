using System.Collections.Generic;
using LgOctEngine.CoreClasses;
using System.Text;
using System.IO;
using symbiosart.datas;
using System;
using UnityEngine.Assertions;
using symbiosart.utils;
using symbiosart.constants;

namespace symbiosart.datas {

   
    public class User
    {
        // filepaths
        public string BasePath { get { return FileUtils.CombineAndCreateDir(FileCs.AppDataPath, Name); } }
        public string CachePath { get { return FileUtils.CombineAndCreateDir(BasePath, FileCs.CACHE_DIRECTORY); } }
        public string LikedPath { get { return FileUtils.CombineAndCreateDir(BasePath, FileCs.LIKED_DIRECTORY); } }
        public string DislikedPath { get { return FileUtils.CombineAndCreateDir(BasePath, FileCs.DISLIKED_DIRECTORY); } }

        // lists
        private Dictionary<string, int> tagsVector = new Dictionary<string, int>();
        private ICollection<string> imagesLiked = new HashSet<string>();
        private ICollection<string> imagesDisliked = new HashSet<string>();

        // properties
        public string Name { get; set; }
        public Dictionary<string, int> TagsVector { get { return tagsVector; } }
        public string TagsVectorAsJson { get { return JsonUtils.DictToJson(tagsVector); } }
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
            builder.Append(JsonUtils.JsonStringEntry("name", Name));
            builder.Append(JsonUtils.JsonObjectEntry("tags_vector",
                JsonUtils.DictToJson(tagsVector)));
            builder.Append(JsonUtils.JsonObjectEntry("liked_ids",
                JsonUtils.CollectionToJson(ImageLiked)));
            builder.Append(JsonUtils.JsonObjectEntry("disliked_ids",
                JsonUtils.CollectionToJson(imagesDisliked)));
            builder.Append("}");

            return builder.ToString();
        }


        public void MarkAsLiked(ImageMetas metas)
        {
            Assert.IsFalse(imagesLiked.Contains(metas.Id));
            updateTagsVector(metas.Tags, true);
            imagesLiked.Add(metas.Id);
        }

        public void MarkAsDisliked(ImageMetas metas)
        {
            Assert.IsFalse(imagesDisliked.Contains(metas.Id));
            updateTagsVector(metas.Tags, false);
            imagesDisliked.Add(metas.Id);
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
    }

}
