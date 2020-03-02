using Forgeage.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Forgeage
{
	public class AbilitiesGridController : MonoBehaviour
	{
        public ObservableCollection<Ability> abilities = new ObservableCollection<Ability>();

        public GameObject image;

        void Start()
        {
            ActionHandler.Instance.Selected.CollectionChanged += RefreshAbilities;
        }

        void RefreshAbilities(object sender, EventArgs e)
        {
            foreach (Transform t in transform)
            {
                Destroy(t.gameObject);
            }
            if (ActionHandler.Instance.Selected.Count > 0)
            {
                if (ActionHandler.Instance.Selected.FirstOrDefault(hero => hero.GetComponent<HeroController>() != null) != null)
                {
                    if (abilities != null)
                    {
                        abilities.CollectionChanged -= RefreshUIAbilities;
                    }
                    abilities = ActionHandler.Instance.Selected.FirstOrDefault(hero => hero.GetComponent<HeroController>() != null).GetComponent<HeroController>().abilities;
                    if (abilities != null)
                    {
                        abilities.CollectionChanged += RefreshUIAbilities;
                        RefreshUIAbilities(this, null);
                    }
                }
            }
        }

        void RefreshUIAbilities(object sender, EventArgs args)
        {
            foreach (Transform t in transform)
            {
                Destroy(t.gameObject);
            }
            foreach (Ability a in abilities)
            {
                GameObject created = Instantiate(image, transform);
                created.GetComponent<Image>().sprite = a.UITexture;
                created.GetComponent<AbilityController>().ability = a;
            }
        }
    }
}
