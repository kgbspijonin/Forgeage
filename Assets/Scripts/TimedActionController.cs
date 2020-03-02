using System;
using UnityEngine;
using UnityEngine.UI;

namespace Forgeage
{
	public class TimedActionController : MonoBehaviour
	{
        public TimedAction action;
        public GameObject ProgressForeground;
        public float MaxForegroundOffset;

        public void SetupActionUI(TimedAction action)
        {
            this.action = action;
            action.TimerUpdated += UpdateUI;
        }

        void UpdateUI(object sender, EventArgs args) {
            if(ProgressForeground != null)
            {
                ProgressForeground.GetComponent<Image>().enabled = true;
                ProgressForeground.transform.localPosition = new Vector3(
                    -(1 - action.CurrentTime / action.TotalTime) * MaxForegroundOffset,
                    0,
                    0);
                ProgressForeground.transform.localScale = new Vector3(
                    action.CurrentTime / action.TotalTime,
                    1,
                    1);
            }
            
        }
	}
}
