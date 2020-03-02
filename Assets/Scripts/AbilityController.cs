using System;
using UnityEngine;
using UnityEngine.UI;

namespace Forgeage
{
	public class AbilityController : MonoBehaviour
	{
        public Ability ability;
        public GameObject ProgressForeground;
        public float MaxForegroundOffset;

        void Start()
        {
            ability.controller = this;
        }

        public void SetupActionUI(Ability action)
        {
            this.ability = action;
            ability.TimerUpdated += UpdateUI;
        }

        public void UpdateUI(object sender, EventArgs args)
        {
            if (ProgressForeground != null)
            {
                if(!ability.Charge)
                {
                    ProgressForeground.GetComponent<Image>().enabled = true;
                    ProgressForeground.transform.localPosition = new Vector3(
                        -(1 - ability.CurrentTime / ability.TotalTime) * MaxForegroundOffset,
                        0,
                        0);
                    ProgressForeground.transform.localScale = new Vector3(
                        ability.CurrentTime / ability.TotalTime,
                        1,
                        1);
                }
                else
                {
                    ProgressForeground.GetComponent<Image>().enabled = false;
                }
            }

        }

        public void Click()
        {
            if(ability.Charge)
            {
                ability.Execute();
                ability.Charge = false;
                UpdateUI(this, null);
            } 
        }
    }
}
