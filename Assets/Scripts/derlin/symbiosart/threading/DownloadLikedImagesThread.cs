using derlin.symbiosart.constants;
using derlin.symbiosart.datas;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using UnityEngine;

namespace derlin.symbiosart.threading
{
    public class DownloadLikedImagesJob : ThreadedJob
    {
        private string path;


        public DownloadLikedImagesJob(string path)
        {
            this.path = path;
        }

        protected override void OnFinished()
        {
            Debug.Log("Download Finished.");
        }


        // Do your threaded task. DON'T use the Unity API here
        protected override void DoInBackground()
        {
            List<string> ids = new List<string>(User.CurrentUser.LikedIds);
            foreach (var id in ids)
            {
                getAndDownload(id);
            }
        }


        private void getAndDownload(string id)
        {
            using (WebClient webClient = new WebClient())
            {
                webClient.Encoding = Encoding.UTF8;
                webClient.Headers["Content-Type"] = "application/json";
                webClient.DownloadStringCompleted += (o, e) =>
                {
                    if (e.Error != null)
                    {
                        UnityEngine.Debug.Log("error downloading image " + id);
                    }
                    else
                    {
                        var metas = ImageMetas.FromJson(e.Result);
                        var uri = new Uri(metas.Url.Replace("https://", "http://"));
                        webClient.DownloadFileAsync(uri, Path.Combine(path, id + "." + metas.Format));
                    }
                };

                webClient.DownloadStringAsync(new Uri(WebCs.ImageDetailsUrl(id)));

            }
        }

    }
}
