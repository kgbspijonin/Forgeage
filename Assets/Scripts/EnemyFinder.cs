using System;
using UnityEngine;

namespace Forgeage
{
	public class EnemyFinder : MonoBehaviour
	{
        public MilitaryController controller;
        public event EventHandler onEnemyFound;

        private void OnTriggerEnter(Collider other)
        {
            if(other.GetComponent<EntityController>() != null && 
                other.GetComponent<EntityController>().Affiliation != controller.GetComponent<EntityController>().Affiliation &&
                other.GetComponent<UnitController>() != null)
            {
                if(controller.Enemy == null)
                {
                    controller.Enemy = other.gameObject;
                }
            }
        }

        public void Scan()
        {
            foreach(var c in Physics.OverlapSphere(transform.position, GetComponent<SphereCollider>().radius))
            {
                if(c.GetComponent<EntityController>() != null && 
                    c.GetComponent<EntityController>().Affiliation != controller.GetComponent<EntityController>().Affiliation &&
                    c.GetComponent<UnitController>() != null)
                {
                    if (controller.Enemy == null)
                    {
                        controller.Enemy = c.gameObject;
                    }
                }
            }
        }
    }
}
