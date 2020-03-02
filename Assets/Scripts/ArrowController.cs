using UnityEngine;

namespace Forgeage
{
	public class ArrowController : MonoBehaviour
	{
        public bool shouldDestroy = true;
        public int Affiliation;
        public int Attack;

        private void OnTriggerEnter(Collider other)
        {
            if((other.GetComponent<Terrain>() || other.GetComponent<ResourceController>()) && shouldDestroy)
            {
                Destroy(gameObject, 0.1f);
            }
            if(other.GetComponent<EntityController>() && other.GetComponent<EntityController>().Affiliation != Affiliation)
            {
                other.GetComponent<EntityController>().CurrentHP -= Attack;
                if(shouldDestroy)
                {
                    Destroy(gameObject, 0.1f);
                }
            }
        }
    }
}
