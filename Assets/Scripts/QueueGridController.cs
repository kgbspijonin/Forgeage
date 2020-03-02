using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Forgeage
{
	public class QueueGridController : MonoBehaviour
	{
        public ObservableCollection<TimedAction> queue = new ObservableCollection<TimedAction>();

        public GameObject Image;

		void Start()
		{
            ActionHandler.Instance.Selected.CollectionChanged += RefreshQueue;
        }

		void RefreshQueue(object sender, EventArgs args)
        {
            foreach (Transform t in transform)
            {
                Destroy(t.gameObject);
            }
            if (queue != null)
            {
                queue.CollectionChanged -= RefreshUIQueue;
            }
            queue = ActionHandler.Instance.Selected.FirstOrDefault(e => e.GetComponent<BuildingController>() != null)?.GetComponent<BuildingController>().queue ?? null;
            if(queue != null)
            {
                queue.CollectionChanged += RefreshUIQueue;
                RefreshUIQueue(this, null);
            }
        }

        void RefreshUIQueue(object sender, EventArgs args)
        {
            foreach (Transform t in transform)
            {
                Destroy(t.gameObject);
            }
            if(queue != null)
            {
                foreach (TimedAction t in queue)
                {
                    GameObject created = Instantiate(Image, transform);
                    created.GetComponent<Image>().sprite = ((ManagedAction)t.action).UITexture;
                    created.GetComponent<TimedActionController>().SetupActionUI(t);
                }
            }
        }

    }
}
