using Forgeage.Extensions;
using System;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UI;

namespace Forgeage
{
	public class ActionsGridController : MonoBehaviour
	{
        public ObservableCollection<IExecutable> actions = new ObservableCollection<IExecutable>();

        public GameObject image;

        void Start()
        {
            ActionHandler.Instance.Selected.CollectionChanged += RefreshActions;
        }

        void RefreshActions(object sender, EventArgs e)
        {
            foreach (Transform t in transform)
            {
                Destroy(t.gameObject);
            }
            if(ActionHandler.Instance.Selected.Count > 0)
            {
                if(actions != null)
                {
                    actions.CollectionChanged -= RefreshUIActions;
                }
                actions = ActionHandler.Instance.Selected[0].GetComponent<Controller>().Actions;
                if (actions != null)
                {
                    actions.CollectionChanged += RefreshUIActions;
                    RefreshUIActions(this, null);
                }
            }
        }

        void RefreshUIActions(object sender, EventArgs args)
        {
            foreach (Transform t in transform)
            {
                Destroy(t.gameObject);
            }
            foreach (Action a in actions)
            {
                GameObject created = Instantiate(image, transform);
                created.GetComponent<Image>().sprite = a.UITexture;
                created.GetComponent<UIImageController>().entity = ActionHandler.Instance.Selected[0].gameObject;
                created.GetComponent<UIImageController>().action = a;
            }
        }
    }
}
