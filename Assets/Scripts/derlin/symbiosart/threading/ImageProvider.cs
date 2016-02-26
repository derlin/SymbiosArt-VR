using derlin.symbiosart.constants;
using derlin.symbiosart.datas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;

namespace derlin.symbiosart.threading
{

    public class ImagesProvider
    {
        /// <summary>
        /// Used by the background thread to cache the downloaded 
        /// image + metas. Note that we can't store the Texture 
        /// directly, since Unity methods can be called only from
        /// the main thread.    
        /// </summary>
        private class CachedImg
        {
            public ImageMetas Metas { get; internal set; }
            public byte[] Bytes { get; internal set; }
        }

        /// <summary>
        /// Used to start/stop the background thread
        /// </summary>
        public bool Running { get; set; }

        /// <summary>
        /// Get the number of images currently in cache.
        /// </summary>
        public int ImagesCount { get { return imagesQueue.Count; } }

        /// <summary>
        /// Get one image. This method is blocking
        /// if none is available.
        /// </summary>
        public Image NextImage
        {
            get
            {
                var img = imagesQueue.Dequeue();
                seenIds.Add(img.Metas.Id);
                return new Image(img.Metas, img.Bytes);
            }
        }

        // the cachedImage "queue"
        private FixedSizedQueue<CachedImg> imagesQueue;

        private int sleepTime = Config.FETCH_METAS_INTERVAL * 1000; // pause between fetches
        private List<string> seenIds = new List<string>(); // avoid duplicates during one session
        private Thread thread = null; // worker thread


        public ImagesProvider(int nbrCells)
        {
            this.imagesQueue = new FixedSizedQueue<CachedImg>(nbrCells * 2);
            this.thread = new Thread(run);
            Debug.Log("starting image provider");
            Start();
        }

        public void Start()
        {
            Running = true;
            this.thread.Start();
        }

        public void Stop()
        {
            Running = false;
            thread.Abort();
        }

        // ==========================
        // Worker thread methods
        // ==========================

        private void run()
        {
            while (Running)
            {
                // compute how many images are missing
                var nbr = imagesQueue.Capacity - imagesQueue.Count;
                if (nbr <= 0) nbr = Config.NBR_METAS_PER_FETCH; // if none missing, default to X images

                var metas = threadDownloadMetas(nbr);

                // foreach metas, if not already evaluated, download the corresponding image
                for (int i = metas.Count - 1; i >= 0; i--)
                {
                    var m = metas[i];
                    if (!seenIds.Contains(m.Id) && !User.CurrentUser.AlreadyMarked(m.Id))
                    {
                        threadDownloadOneImage(m);
                    }
                }

                // wait a bit
                Thread.Sleep(sleepTime);
            }// end while

            Debug.Log("image provider: worker thread stopped.");
        }


        // retrieve metas from the server
        private List<ImageMetas> threadDownloadMetas(int nbr)
        {
            try
            {
                using (WebClient metasWebClient = new WebClient())
                {
                    metasWebClient.Encoding = Encoding.UTF8;
                    metasWebClient.Headers["Content-Type"] = "application/json";
                    string jsonString = metasWebClient.UploadString(WebCs.ImageSuggestionsUrl(nbr), 
                        User.CurrentUser.TagsVectorAsJson());
                    return ImageMetas.FromJsonArray(jsonString);
                }
            }
            catch (Exception e)
            {
                // TODO: handle error properly
                Debug.Log("error downloading metas " + e);
                Debug.Log(User.CurrentUser.TagsVectorAsJson());
                return new List<ImageMetas>();
            }
        }

        // download one image from the url in the meta
        private void threadDownloadOneImage(ImageMetas metas)
        {
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    var uri = new Uri(metas.Url.Replace("https://", "http://"));
                    webClient.DownloadDataCompleted += ((o, e) =>
                    {
                        if (e.Error == null)
                        {
                            var image = new CachedImg();
                            image.Metas = (ImageMetas)e.UserState;
                            image.Bytes = e.Result;
                            imagesQueue.Enqueue(image);
                        }
                        else
                        {
                            Debug.Log(e.Error);
                        }
                    });
                    webClient.DownloadDataAsync(uri, metas);
                }

            }
            catch (Exception e)
            {
                Debug.Log("error downloading " + metas.Url);
                Debug.Log(e);
            }

        }
    }
}
