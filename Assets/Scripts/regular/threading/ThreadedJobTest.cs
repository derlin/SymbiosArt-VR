using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using UnityEngine;
using symbiosart.datas;

namespace symbiosart.threading
{
    class ThreadedJobTest : ThreadedJob
    {
        List<Exception> errors = new List<Exception>();

        public List<ImageMetas> Metas;

        protected override void DoInBackground()
        {
            using (WebClient webClient = new WebClient())
            {
                webClient.Headers["Content-Type"] = "application/json";
                string jsonString = webClient.UploadString(constants.WebCs.ImagesUrl(10), "{}");

                Metas = ImageMetas.FromJsonArray(jsonString);
                foreach (var m in Metas)
                {
                    downloadImage(m);
                }
            }
        }

        private void downloadImage(ImageMetas metas)
        {
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    string path = "C:\\Users\\Dulcolax\\AppData\\LocalLow\\DefaultCompany\\SA\\New";//Path.Combine(UserUtils.CurrentUser.BasePath, "test");
                    if (string.IsNullOrEmpty(metas.Format) || metas.Format.ToLower().Contains("png"))
                    {
                        path = Path.Combine(path, metas.Id + ".png");
                    }
                    else
                    {
                        path = Path.Combine(path, metas.Id + "." + metas.Format);
                    }


                    var uri = new Uri(metas.Url.Replace("https://", "http://"));
                    webClient.DownloadFile(uri, path);
                    //webClient.DownloadFileAsync(new Uri(metas.Url), path);
                }
            }
            catch (Exception e)
            {
                errors.Add(e);
            }
        }


        protected override void OnFinished()
        {
            Debug.Log("metas " + Metas.Count);
        }
    }
}
