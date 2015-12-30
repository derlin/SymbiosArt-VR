﻿using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;
using System.Collections.Generic;
using LgOctEngine.CoreClasses;
using System.Text;

public class UserUtils : MonoBehaviour {

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

        public string TagsVectorAsJson()
        {
            StringBuilder sb = new StringBuilder("{");
            foreach (var entry in tagsVector)
            {
                sb.Append(string.Format("\"{0}\": {1}, ", entry.Key, entry.Value));
            }

            sb.Remove(sb.Length - 2, 2);
            sb.Append("}");
            return sb.ToString();
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
                s += string.Format("    {0}: {1}\n", entry.Key, entry.Value);
            }
            return s;
        }
    }

}
