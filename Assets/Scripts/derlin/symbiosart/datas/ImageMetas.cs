using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace derlin.symbiosart.datas
{

    public class ImageMetas : IComparable<ImageMetas>
    {
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("tags")]
        public List<string> Tags { get; set; }

        [JsonProperty("originalFormat")]
        public string Format { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static ImageMetas FromJson(string json)
        {
            return JsonConvert.DeserializeObject<ImageMetas>(json);
        }

        public static List<ImageMetas> FromJsonArray(string json)
        {
            return JsonConvert.DeserializeObject<List<ImageMetas>>(json);
        }

        public int CompareTo(ImageMetas other)
        {
            return Id.CompareTo(other.Id);
        }
    }
}
