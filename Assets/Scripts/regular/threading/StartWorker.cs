using symbiosart.datas;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace symbiosart.threading
{
    public class StartWorker : ThreadedJob
    {
        private User user;
        private int nbr;

        //public List<ImageMetas> m_metas = new List<ImageMetas>();
        //public List<ImageMetas> Metas { get { return m_metas; } }

        public SafeQueue<ImageMetas> Queue = new SafeQueue<ImageMetas>();

        private SafeQueue<object> asyncTasks = new SafeQueue<object>();

        private List<Exception> m_errors = new List<Exception>();
        public List<Exception> Exceptions { get { return m_errors; } }

        public StartWorker(User user, int nbr)
        {
            this.user = user;
            this.nbr = nbr;
        }


        protected override void DoInBackground()
        {
            var ids = loadAlreadyCachedMetas();

            if (ids.Count < nbr)
            {
                var metas = downloadMetas();
                if(metas == null)
                {
                    return;
                }

                foreach (var m in metas)
                {
                    if (ids.Contains(m.Id)) continue;
                    ids.Add(m.Id);
                    cacheOne(m);
                    asyncTasks.Enqueue(new object());
                }
            }

        }

        protected override void OnFinished()
        {
            while(asyncTasks.Count > 0)
            {
                Thread.Sleep(1000);
            }
        }

        private List<ImageMetas> downloadMetas()
        {
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.Headers["Content-Type"] = "application/json";
                    string jsonString = webClient.UploadString(constants.WebCs.ImagesUrl(nbr), user.TagsVectorAsJson);
                    return ImageMetas.FromJsonArray(jsonString);
                }
            }
            catch (Exception e)
            {
                m_errors.Add(e);
                return null;
            }
        }

        private List<string> loadAlreadyCachedMetas()
        {
            List<string> ids = new List<string>();
            if (Directory.Exists(user.CachePath))
            {
                foreach (var f in Directory.GetFiles(user.CachePath, "*.json"))
                {
                    // load metas
                    var m = ImageMetas.FromJson(File.ReadAllText(f));

                    // ensure the image file exists
                    if (File.Exists(f.Replace("json", m.Format)))
                    {
                        Queue.Enqueue(m);
                        ids.Add(m.Id);
                    }
                    else // no image file, delete meta
                    {
                        File.Delete(f);
                    }
                }
            }

            return ids;
        }

        private void cacheOne(ImageMetas metas)
        {
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadFileCompleted += (o, e) =>
                    {
                        if (e.Error == null)
                        {
                            File.WriteAllText(metas.MetaFile(user.CachePath), metas.ToJson());
                            Queue.Enqueue(metas);
                            asyncTasks.Dequeue();
                        }
                    };

                    var path = metas.ImageFile(user.CachePath);
                    var uri = new Uri(metas.Url.Replace("https://", "http://"));
                    webClient.DownloadFileAsync(uri, path);
                }
            }
            catch (Exception e)
            {
                m_errors.Add(e);
            }
        }
    }
}


