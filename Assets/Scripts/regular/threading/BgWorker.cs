using symbiosart.constants;
using symbiosart.datas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace symbiosart.threading
{

    public class ImagesProvider
    {
        public class CachedImg
        {
            public ImageMetas Metas { get; internal set; }
            public byte[] Bytes { get; internal set; }
        }

        public bool Running { get; set; }
        public int ImagesCount { get { return imagesQueue.Count; } }

        public Image NextImage
        {
            get
            {
                var img = imagesQueue.Dequeue();
                seenIds.Add(img.Metas.Id);
                return new Image(img.Metas, img.Bytes);
            }
        }

        private FixedSizedSafeQueue<CachedImg> imagesQueue;
        private User user;
        private int sleepTime = 30 * 1000;
        private List<string> seenIds = new List<string>();
        private Thread thread = null;


        public ImagesProvider(User user, int nbrCells)
        {
            this.user = user;
            this.imagesQueue = new FixedSizedSafeQueue<CachedImg>(nbrCells * 2);
            this.thread = new Thread(run);
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
            UnityEngine.Debug.Log("image provider stopped");
        }

        // ==========================
        private void run()
        {
            WebClient metasWebClient = new WebClient();

            List<ImageMetas> imageMetas = null;

            while (Running)
            {
                try
                {
                    var nbr = imagesQueue.Capacity - imagesQueue.Count;
                    if (nbr <= 0) nbr = 10;

                    metasWebClient.Headers["Content-Type"] = "application/json";
                    string jsonString = metasWebClient.UploadString(WebCs.ImagesUrl(nbr), user.TagsVectorAsJson);
                    imageMetas = ImageMetas.FromJsonArray(jsonString);
                    for(int i = imageMetas.Count - 1; i >= 0 ; i--)
                    {
                        var m = imageMetas[i];
                        if (seenIds.Contains(m.Id)) continue;
                        using (WebClient webClient = new WebClient())
                        {
                            var uri = new Uri(m.Url.Replace("https://", "http://"));
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
                                    UnityEngine.Debug.Log(e.Error);
                                }
                            });
                            webClient.DownloadDataAsync(uri, m);
                            //var datas = webClient.DownloadData(uri);
                            //var image = new CachedImg();
                            //image.Metas = m;
                            //image.Bytes = datas;
                            //imagesQueue.Enqueue(image);
                        }

                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log(e);
                }

                Thread.Sleep(sleepTime);

            }// end while

            UnityEngine.Debug.Log("image provider: run thread stopped.");
        }
    }
}
