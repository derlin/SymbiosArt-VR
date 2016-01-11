using symbiosart.constants;
using symbiosart.datas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace symbiosart.threading
{
    public class FetchMetasThread : ThreadedJob
    {
        private string json;
        public List<ImageMetas> Metas { get; private set; }

        public FetchMetasThread(string tagsVector)
        {
            json = tagsVector;
        }

        protected override void DoInBackground()
        {
            Metas = downloadMetas(Config.NBR_METAS_PER_FETCH);
        }


        private List<ImageMetas> downloadMetas(int nbr)
        {
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.Headers["Content-Type"] = "application/json";
                    string jsonString = webClient.UploadString(WebCs.ImagesUrl(nbr), json);
                    return ImageMetas.FromJsonArray(jsonString);
                }
            }
            catch (Exception e)
            {
                return new List<ImageMetas>();
            }
        }
    }
}
