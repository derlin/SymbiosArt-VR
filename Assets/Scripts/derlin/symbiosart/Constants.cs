using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace derlin.symbiosart.constants
{
    public class Config
    {
        public static readonly int FETCH_METAS_INTERVAL = 15; // in seconds
        public static readonly int NBR_METAS_PER_FETCH = 2; // in seconds
        public static readonly float REPLACE_CELL_FREQ = 10; // in seconds

        public static readonly string MAIN_SCENE_NAME = "main-scene-bis";
        public static readonly string START_SCENE_NAME = "start-scene";

    }

    public class WebCs
    {
        public static string ServiceUrl { get { return "http://error-418.com:8680/rest"; } } //"http://localhost:8680/rest"; } }
        public static string ImageSuggestionsUrl(int nbr) { return ServiceUrl + "/images/suggestions/" + nbr; }
        public static string ImageDetailsUrl(string id) { return ServiceUrl + "/images/" + id; }
        public static string UsersUrl(string action) { return ServiceUrl + "/user/" + action; }
        public static string UsersUrl() { return ServiceUrl + "/user"; }
    }

    public class FileCs
    {
        // default encoding to utf8 (when converting string to byte on file write)
        private static Encoding encoding = Encoding.UTF8;

        public static Encoding DefaultEncoding { get { return encoding; } set { encoding = value; } }

    }
}
