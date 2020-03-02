using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Forgeage
{
	public class CowController : UnitController
	{
        Coroutine decay;

        private void Start()
        {
            Actions.Add(new Action(Spawner.Instance.Entities.IcKillCow, Suicide));
        }

        public void Suicide()
        {
            Actions.Remove(Actions.FirstOrDefault(a => ((Action)a).UITexture == Spawner.Instance.Entities.IcKillCow));
            Destroy(GetComponent<NavMeshAgent>());
            Destroy(GetComponent<Rigidbody>());
            gameObject.layer = LayerMask.NameToLayer("Natural Resources");
            transform.Find("Canvas").gameObject.SetActive(false);
            transform.Find("Capture Collider").gameObject.SetActive(false);
            transform.localScale = new Vector3(1, 0.25f, 1);
            decay = StartCoroutine(Decay());
        }

        public IEnumerator Decay()
        {
            while(true)
            {
                yield return new WaitForSeconds(1);
                GetComponent<ResourceController>().GatherResource(null);
            }
        }
    }
}
