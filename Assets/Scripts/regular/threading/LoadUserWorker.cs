using LgOctEngine.CoreClasses;
using symbiosart.constants;
using symbiosart.datas;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using UnityEngine;

namespace symbiosart.threading
{

    public class LoadUserWorker : ThreadedJob
    {
        private string username;
        public User User { get; private set; }
        public Exception Exception { get; private set; }

        public LoadUserWorker(string username)
        {
            this.username = username;
        }

        protected override void DoInBackground()
        {
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    string url = constants.WebCs.UsersUrl(username);
                    string json = webClient.DownloadString(url);
                    User = User.FromJson(json);
                }
            }
            catch (Exception e)
            {
                Exception = e;
            }
        }


    }
}
