using Forgeage.Extensions;
using System;
using System.Collections;
using UnityEngine;

namespace Forgeage
{
    public class TimedAction
    {
        public GameObject parent;
        public IExecutable action;
        private float totalTime;
        private float currentTime;
        public Coroutine coroutine;

        public event EventHandler TimerUpdated;

        public float Progress {
            get {
                return CurrentTime / TotalTime;
            }
        }

        public float CurrentTime {
            get {
                return currentTime;
            }
            set {
                currentTime = value;
                TimerUpdated?.Invoke(this, null);
                if (currentTime >= TotalTime)
                {
                    currentTime = totalTime;
                    action.Execute();
                    parent.GetComponent<BuildingController>().StopCoroutine(coroutine);
                    parent.GetComponent<BuildingController>().queue.Remove(this);
                }
            }
        }

        public float TotalTime {
            get {
                return totalTime;
            }
            set {
                totalTime = value;
            }
        }

		public TimedAction(GameObject parent, IExecutable action, float timer, float currentProgress = 0)
        {
            this.parent = parent;
            this.action = action;
            totalTime = timer;
            currentTime = currentProgress;
        }

        public IEnumerator UpdateProgress()
        {
            while(true)
            {
                yield return new WaitForSeconds(0.25f);
                CurrentTime += 0.25f;
            }
        }

        public void DoNothing(object sender, EventArgs args)
        {

        }
    }
}
