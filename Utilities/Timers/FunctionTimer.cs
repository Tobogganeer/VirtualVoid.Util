using System;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualVoid.Util.Timers
{
    public class FunctionTimer// : IDisposable
    {
        public class FunctionTimerManager : MonoBehaviour
        {
            public static FunctionTimerManager instance;

            private List<FunctionTimer> timers = new List<FunctionTimer>();
            private Queue<FunctionTimer> toAdd = new Queue<FunctionTimer>();
            private Queue<FunctionTimer> toRemove = new Queue<FunctionTimer>();

            public void AddTimer(FunctionTimer timer)
            {
                toAdd.Enqueue(timer);
            }

            public void RemoveTimer(FunctionTimer timer)
            {
                toRemove.Enqueue(timer);
            }

            private void Update()
            {
                while (toAdd.Count > 0)
                {
                    FunctionTimer timer = toAdd.Dequeue();
                    if (!timers.Contains(timer)) timers.Add(timer);
                }

                while (toRemove.Count > 0)
                {
                    FunctionTimer timer = toRemove.Dequeue();
                    if (timers.Contains(timer)) timers.Remove(timer);
                }

                toAdd.Clear();

                toRemove.Clear();

                foreach (FunctionTimer timer in timers)
                {
                    timer.Update();
                }
            }
        }

        Action action;
        float time;
        bool isDestroyed;

        private FunctionTimer(Action action, float afterTime)
        {
            if (FunctionTimerManager.instance == null)
            {
                FunctionTimerManager.instance = new GameObject("FunctionTimerManager", typeof(FunctionTimerManager)).GetComponent<FunctionTimerManager>();
            }

            this.action = action;
            time = afterTime;
            isDestroyed = false;

            FunctionTimerManager.instance.AddTimer(this);
        }

        /// <summary>
        /// Runs the specified action <paramref name="action"/> after time <paramref name="time"/>.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="time"></param>
        public static void Run(Action action, float time)
        {
            _ = new FunctionTimer(action, time);
        }

        public void Update()
        {
            if (!isDestroyed)
            {
                time -= Time.deltaTime;
                if (time <= 0)
                {
                    action?.Invoke();
                    Dispose();
                }
            }
        }

        public void Dispose()
        {
            //Dispose(true);
            isDestroyed = true;
            FunctionTimerManager.instance.RemoveTimer(this);

            GC.SuppressFinalize(this);
        }

        //protected virtual void Dispose(bool disposing)
        //{
        //    if (!isDestroyed)
        //    {
        //        if (!disposing)
        //        {
        //
        //        }
        //
        //        isDestroyed = true;
        //    }
        //}
    }
}
