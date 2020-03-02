using System;
using UnityEngine;

namespace Forgeage
{
	public class SwingCollider : MonoBehaviour
	{
        public MilitaryController controller;

        public event EventHandler EnemyReached;
        public event EventHandler EnemyAbandoned;

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == controller.Enemy)
            {
                EnemyReached.Invoke(this, EventArgs.Empty);
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.gameObject == controller.Enemy)
            {
                EnemyAbandoned.Invoke(this, EventArgs.Empty);
            }
        }

        public bool CheckForEnemy()
        {
            foreach (Collider c in Physics.OverlapSphere(transform.position, GetComponent<SphereCollider>().radius)) {
                if(c.gameObject == controller.Enemy)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
