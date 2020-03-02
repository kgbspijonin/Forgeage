using System;
using UnityEngine;

namespace Forgeage
{
	public class CaptureController : MonoBehaviour
	{
        public CowController controller;

        void OnTriggerEnter(Collider other)
        {
            if(other.GetComponent<EntityController>() != null)
            {
                if (controller.GetComponent<EntityController>().Affiliation == 0)
                {
                    controller.GetComponent<EntityController>().Affiliation = other.GetComponent<EntityController>().Affiliation;
                }
                else
                {
                    CheckForEnemy();
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.GetComponent<EntityController>() != null)
            {
                if (controller.GetComponent<EntityController>().Affiliation == other.GetComponent<EntityController>().Affiliation)
                {
                    CheckForEnemy();
                }
            }
        }

        public bool CheckForAlly()
        {
            foreach (Collider c in Physics.OverlapSphere(transform.position, GetComponent<SphereCollider>().radius))
            {
                if (c.gameObject.GetComponent<EntityController>() != null && c.GetComponent<EntityController>().Affiliation == controller.GetComponent<EntityController>().Affiliation)
                {
                    return true;
                }
            }
            return false;
        }

        public bool CheckForEnemy()
        {
            foreach (Collider c in Physics.OverlapSphere(transform.position, GetComponent<SphereCollider>().radius))
            {
                if (c.gameObject.GetComponent<EntityController>() != null && c.GetComponent<EntityController>().Affiliation != controller.GetComponent<EntityController>().Affiliation)
                {
                    if(!CheckForAlly())
                    {
                        controller.GetComponent<EntityController>().Affiliation = c.GetComponent<EntityController>().Affiliation;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
