using symbiosart.utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace symbiosart.constants
{
    public class Config
    {
        public static readonly float FETCH_METAS_INTERVAL = 15; // in seconds
        public static readonly int NBR_METAS_PER_FETCH = 2; // in seconds
        public static readonly int NBR_CACHED_IMAGES = 20; // in seconds

    }

    public class WebCs
    {
        public static string ServiceUrl { get { return "http://error-418.com:8680/rest"; } }
        public static string ImagesUrl(int nbr) { return ServiceUrl + "/images/suggestions/" + nbr; }
        public static string UsersUrl(string action) { return ServiceUrl + "/user/" + action; }
    }

    public class FileCs
    {
        // default encoding to utf8 (when converting string to byte on file write)
        private static Encoding encoding = Encoding.UTF8;

        public static Encoding DefaultEncoding { get { return encoding; } set { encoding = value; } }

        public static readonly string AppDataPath = FileUtils.CombineAndCreateDir(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SymbiosArt"
            );

        public static readonly string LIKED_DIRECTORY = "liked";
        public static readonly string DISLIKED_DIRECTORY = "disliked";
        public static readonly string CACHE_DIRECTORY = "cache";

    }
}
