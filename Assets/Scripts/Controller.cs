using Forgeage.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.AI;

namespace Forgeage
{
	public class Controller : MonoBehaviour
	{
        public ObservableCollection<IExecutable> Actions = new ObservableCollection<IExecutable>();

        public virtual void MoveTo(Vector3 point)
        {
            GetComponent<NavMeshAgent>().SetDestination(point);
        }

        void Start()
        {
            GetComponent<EntityController>().Controller = this;
        }

        public virtual void Die()
        {
            Destroy(gameObject);
        }
    }
}
