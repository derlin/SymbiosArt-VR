using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace derlin.symbiosart.datas
{
    public class User
    {
        public static readonly string DEFAULT_NAME = "New";
        // TODO : only for debug purpose (to launch main scene without null ptr exception)
        private static User u = new User();
        //public static User CurrentUser { get; private set; }
        public static User CurrentUser { get { return u; } private set { u = value; } }



        // ===== static initializers

        public static User NewUser()
        {
            CurrentUser = new User();
            return CurrentUser;
        }

        public static User FromJson(string json)
        {
            CurrentUser = JsonConvert.DeserializeObject<User>(json);
            return CurrentUser;
        }

        // ----------------------------------------------------------

        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("tags_vector")]
        public Dictionary<string, int> TagsVector { get; set; }

        [JsonProperty("liked_ids")]
        public List<string> LikedIds { get; set; }

        [JsonProperty("disliked_ids")]
        public List<string> DislikedIds { get; set; }

        // ------------------------------------------------------

        public User()
        {
            Name = DEFAULT_NAME;
            TagsVector = new Dictionary<string, int>();
            LikedIds = new List<string>();
            DislikedIds = new List<string>();
        }


        public string TagsVectorAsJson(){ return JsonConvert.SerializeObject(TagsVector); }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public bool AlreadyMarked(string id)
        {
            return LikedIds.Contains(id) || DislikedIds.Contains(id);
        }

        public void MarkAsLiked(ImageMetas metas)
        {
            Assert.IsFalse(LikedIds.Contains(metas.Id));
            updateTagsVector(metas.Tags, true);
            LikedIds.Add(metas.Id);
        }

        public void MarkAsDisliked(ImageMetas metas)
        {
            Assert.IsFalse(DislikedIds.Contains(metas.Id));
            updateTagsVector(metas.Tags, false);
            DislikedIds.Add(metas.Id);
        }
        // ------------------------------------------------------
        protected void updateTagsVector(List<string> tags, bool liked)
        {
            int wheight = liked ? 1 : -1;
            foreach (var tag in tags)
            {
                if (TagsVector.ContainsKey(tag)) TagsVector[tag] += wheight;
                else TagsVector[tag] = wheight;
            }

        }
    }
}
