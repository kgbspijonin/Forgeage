using Forgeage.Extensions;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Forgeage
{
	public class Ability : IExecutable
	{
        public GameObject parent;
        public AbilityDelegate ability;
        public float TotalTime { get; set; }
        private float currentTime;
        public Coroutine coroutine;
        private bool charge = true;
        public Sprite UITexture;
        public AbilityController controller;

        public bool Charge {
            get {
                return charge;
            } set {
                charge = value;
                if(charge)
                {
                    parent.GetComponent<Controller>().StopCoroutine(coroutine);
                }
                if(!charge)
                {
                    coroutine = parent.GetComponent<Controller>().StartCoroutine(UpdateProgress());
                }
                controller?.UpdateUI(this, null);
            }
        }

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
                controller?.UpdateUI(this, null);
                if (currentTime >= TotalTime)
                {
                    currentTime = 0;
                    Charge = true;
                }
            }
        }

        public Ability(Sprite uiTexture, GameObject parent, AbilityDelegate ability, float timer, float currentProgress = 0)
        {
            this.parent = parent;
            this.ability = ability;
            this.UITexture = uiTexture;
            TotalTime = timer;
            currentTime = currentProgress;
        }

        public IEnumerator UpdateProgress()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.25f);
                if(!Charge)
                {
                    CurrentTime += 0.25f;
                }
            }
        }

        public void DoNothing(object sender, EventArgs args)
        {

        }

        public void Execute()
        {
            ability?.Invoke(parent);
        }
    }
}
