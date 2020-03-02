using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Forgeage
{
	public class MilitaryController : UnitController
	{
        public GameObject enemy;

        public event EventHandler NewEnemySelected;

        public GameObject Enemy {
            get { return enemy; }
            set {
                enemy = value;
                NewEnemySelected?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Start()
        {
            
        }

        public virtual void GoAttack(GameObject target)
        {
            enemy = target;
        }

        public virtual IEnumerator Attack()
        {
            return null;
        }
    }
}
