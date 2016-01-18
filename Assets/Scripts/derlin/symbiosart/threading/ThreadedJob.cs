using System.Collections;


namespace derlin.symbiosart.threading
{
    public class ThreadedJob
    {
        private bool m_IsDone = false;
        private object m_Handle = new object();
        private System.Threading.Thread m_Thread = null;

        protected bool IsDone
        {
            get
            {
                bool tmp;
                lock (m_Handle)
                {
                    tmp = m_IsDone;
                }
                return tmp;
            }
            set
            {
                lock (m_Handle)
                {
                    m_IsDone = value;
                }
            }
        }

        public virtual void Start()
        {
            m_Thread = new System.Threading.Thread(Run);
            m_Thread.Start();
        }


        public virtual void Abort()
        {
            m_Thread.Abort();
        }

        // Do your threaded task. DON'T use the Unity API here
        protected virtual void DoInBackground() { }

        // This is executed by the Unity main thread when the job is finished
        protected virtual void OnFinished() { }

        public virtual bool IsFinished()
        {
            if (IsDone)
            {
                OnFinished();
                return true;
            }
            return false;
        }


        IEnumerator WaitFor()
        {
            while (!IsFinished())
            {
                yield return null;
            }
        }


        private void Run()
        {
            DoInBackground();
            IsDone = true;
        }
    }
}
