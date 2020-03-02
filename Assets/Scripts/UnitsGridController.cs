using System;
using UnityEngine;
using UnityEngine.UI;

namespace Forgeage
{
	public class UnitsGridController : MonoBehaviour
	{
        public GameObject image;

		void Start()
		{
            ActionHandler.Instance.Selected.CollectionChanged += RefreshUnits;
		}

        void RefreshUnits(object sender, EventArgs e)
        {
            foreach(Transform t in transform)
            {
                Destroy(t.gameObject);
            }
            foreach(EntityController uc in ActionHandler.Instance.Selected)
            {
                GameObject created = Instantiate(image, transform);
                created.GetComponent<Image>().sprite = uc.UITexture;
                created.GetComponent<UIImageController>().entity = uc.gameObject;
            }
        }
	}
}
