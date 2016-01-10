using symbiosart.datas;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace symbiosart.threading
{
    public class StartWorker : ThreadedJob
    {
        private User user;
        private int nbr;

        //public SafeList<ImageMetas> safeList = new SafeList<ImageMetas>();

        private List<ImageMetas> m_metas = new List<ImageMetas>();
        public List<ImageMetas> Metas { get { return m_metas; } }

        private List<Exception> m_errors = new List<Exception>();
        public List<Exception> Exceptions { get { return m_errors; } }

        public StartWorker(User user, int nbr)
        {
            this.user = user;
            this.nbr = nbr;
        }


        protected override void DoInBackground()
        {
            loadAlreadyCachedMetas();

            if (nbr > 0)
            {
                var metas = downloadMetas();
                foreach (var m in metas)
                {
                    if (downloadImage(m))
                    {
                        File.WriteAllText(m.MetaFile(user.CachePath), m.ToJson());
                        m_metas.Add(m);
                    }
                }
            }

        }

        private List<ImageMetas> downloadMetas()
        {
            try {
                using (WebClient webClient = new WebClient())
                {
                    webClient.Headers["Content-Type"] = "application/json";
                    string jsonString = webClient.UploadString(constants.WebCs.ImagesUrl(nbr), user.TagsVectorAsJson);
                    return ImageMetas.FromJsonArray(jsonString);
                }
            }catch(Exception e)
            {
                m_errors.Add(e);
                return null;
            }
        }

        private void loadAlreadyCachedMetas()
        {
            if (Directory.Exists(user.CachePath))
            {
                foreach (var f in Directory.GetFiles(user.CachePath, "*.json"))
                {
                    // load metas
                    var m = ImageMetas.FromJson(File.ReadAllText(f));

                    // ensure the image file exists
                    if (File.Exists(f.Replace("json", m.Format)))
                    {
                        m_metas.Add(m);
                        nbr--;
                    }
                    else // no image file, delete meta
                    {
                        File.Delete(f);
                    }
                }
            }
        }

        private bool downloadImage(ImageMetas metas)
        {
            try
            {

                using (WebClient webClient = new WebClient())
                {
                    

                    //webClient.DownloadFileCompleted += (o,e) => { for async
                    //    if (e.Error)
                    //    {

                    //    }
                    //};

                    var path = metas.ImageFile(user.CachePath);
                    var uri = new Uri(metas.Url.Replace("https://", "http://"));
                    webClient.DownloadFile(uri, path);
                    return true;
                }
            }
            catch (Exception e)
            {
                m_errors.Add(e);
                return false;
            }
        }
    }
}


