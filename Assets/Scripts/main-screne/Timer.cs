using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Utility: class allowing to easily schedule a job to run
/// periodically.
/// </summary>
public class Timer : MonoBehaviour
{
    public delegate IEnumerator Tick();

    private static List<TimerFunc> delegates = new List<TimerFunc>();


    void Update()
    {
        if (delegates.Count == 0) return;
        foreach (var del in delegates)
        {
            if(Time.time - del.LastTime >= del.Delay)
            {
                StartCoroutine(del.TickFunc());
                del.LastTime = Time.time;
                if (!del.Repeat) del.Pause();
            }
        } 
    }


    public class TimerFunc
    {
        public float Delay { get; set; }
        public Tick TickFunc { get; set; }
        public bool Repeat { get; set; }
        public float LastTime { get; set; }
        public bool Running { get; set; }


        public TimerFunc(Tick callback, float time, bool repeat)
        {
            TickFunc = callback;
            Delay = time;
            Repeat = repeat;
        }

        public void Start()
        {
            LastTime = Time.time;
            delegates.Add(this);
            Running = true;
        }

        public void Pause()
        {
            delegates.Remove(this);
            Running = false;
        }
    }
}