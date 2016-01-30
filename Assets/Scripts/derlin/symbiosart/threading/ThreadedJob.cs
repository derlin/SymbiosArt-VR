using System.Collections;


namespace derlin.symbiosart.threading
{
    public class ThreadedJob
    {
        private bool isDone = false;
        private object @lock = new object();
        private System.Threading.Thread thread = null;

        protected bool IsDone
        {
            get
            {
                bool tmp;
                lock (@lock)
                {
                    tmp = isDone;
                }
                return tmp;
            }
            set
            {
                lock (@lock)
                {
                    isDone = value;
                }
            }
        }

        public virtual void Start()
        {
            thread = new System.Threading.Thread(Run);
            thread.Start();
        }


        public virtual void Abort()
        {
            thread.Abort();
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
