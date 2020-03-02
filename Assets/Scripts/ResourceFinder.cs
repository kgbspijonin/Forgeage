using System;
using UnityEngine;

namespace Forgeage
{
	public class ResourceFinder : MonoBehaviour
	{
        public VillagerController Controller;

        public event EventHandler ResourceReached;
        public event EventHandler ResourceAbandoned;

        void OnTriggerEnter(Collider other)
        {
            if(other.gameObject == Controller.ResourceDestination)
            {
                ResourceReached.Invoke(this, EventArgs.Empty);
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.gameObject == Controller.ResourceDestination)
            {
                ResourceAbandoned.Invoke(this, EventArgs.Empty);
            }
        }

        public bool CheckForResource()
        {
            foreach (Collider c in Physics.OverlapSphere(transform.position, GetComponent<SphereCollider>().radius))
            {
                if (c.gameObject == Controller.ResourceDestination)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
