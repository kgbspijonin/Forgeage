using UnityEngine;

namespace Forgeage
{
	public class ReviveController : MonoBehaviour
	{
        public HeroController controller;

        private void OnTriggerEnter(Collider other)
        {
            if(controller.GetComponent<EntityController>().CurrentHP > controller.GetComponent<EntityController>().MaxHP / 4 && 
                controller.isDead &&
                other.GetComponent<EntityController>() != null &&
                other.GetComponent<EntityController>()?.Affiliation == controller.GetComponent<EntityController>()?.Affiliation)
            {
                controller.Revive();
            }
        }
    }
}
