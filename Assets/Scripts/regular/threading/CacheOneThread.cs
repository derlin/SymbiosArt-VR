using symbiosart.datas;
using symbiosart.threading;
using System;
using System.IO;
using System.Net;

namespace Assets.Scripts.regular.threading
{
    public class CacheOneThread : ThreadedJob
    {
        private ImageMetas metas;
        private Action<ImageMetas, Exception> complete;
        private string cachePath;
        private Exception exception;

        public CacheOneThread(string cachePath, ImageMetas metas, Action<ImageMetas, Exception> complete)
        {
            this.metas = metas;
            this.complete = complete;
            this.cachePath = cachePath;
        }

        protected override void DoInBackground()
        {
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    var uri = new Uri(metas.Url.Replace("https://", "http://"));
                    webClient.DownloadFile(uri, metas.ImageFile(cachePath));
                    File.WriteAllText(metas.MetaFile(cachePath), metas.ToJson());
                }
            }
            catch (Exception e)
            {
                exception = e;
            }
        }

        protected override void OnFinished()
        {
            if (complete != null) complete(metas, exception);
        }
    }
}
