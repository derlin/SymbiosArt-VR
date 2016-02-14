using System.Collections;

/// <summary>
/// abstract class for easily creating a thread from a Unity component.
/// Only the doInBackground method is actually executed in another thread.
/// The interaction with the Unity components can be done in the OnFinished method.
/// </summary>
namespace derlin.symbiosart.threading
{
    public class ThreadedJob
    {
        private bool isDone = false;
        private object @lock = new object();
        private System.Threading.Thread thread = null;

        // keep track of the thread status
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

        // launch the job
        public virtual void Start()
        {
            thread = new System.Threading.Thread(() =>
            {
                DoInBackground();
                IsDone = true;
            });
            thread.Start();
        }

        // kill the job
        public virtual void Abort()
        {
            thread.Abort();
        }

        // Do your threaded task. DON'T use the Unity API here
        protected virtual void DoInBackground() { }

        // This is executed by the Unity main thread when the job is finished
        protected virtual void OnFinished() { }

        // check if the thread is still running
        public virtual bool IsFinished()
        {
            if (IsDone)
            {
                OnFinished();
                return true;
            }
            return false;
        }

        // wait for the thread to finish (blocking call)
        IEnumerator WaitFor()
        {
            while (!IsFinished())
            {
                yield return null;
            }
        }
    }
}
