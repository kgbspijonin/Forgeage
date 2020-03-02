using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Forgeage
{
	public class GalleyController : MilitaryController
	{
        public GameObject Torpedo;

        public static float fireDelay = 3f;
        public static int attack = 40;

        private static int hpP1 = 250;
        private static int hpP2 = 250;

        public static int HpP1 {
            get {
                return hpP1;
            }
            set {
                hpP1 = value;
                UpdateHP(Spawner.Instance.Entities.player1entities);
            }
        }

        public static int HpP2 {
            get {
                return hpP2;
            }
            set {
                hpP2 = value;
                UpdateHP(Spawner.Instance.Entities.player2entities);
            }
        }

        private static void UpdateHP(List<GameObject> entities)
        {
            foreach (var go in entities)
            {
                if (go.GetComponent<GalleyController>() != null)
                {
                    go.GetComponent<EntityController>().MaxHP = go.GetComponent<EntityController>().Affiliation == 1 ? HpP1 : HpP2;
                    go.GetComponent<EntityController>().CurrentHP = go.GetComponent<EntityController>().Affiliation == 1 ? HpP1 : HpP2;
                }
            }
        }

        public SwingCollider swingCollider;

        private Coroutine coroutine;

        new void Start()
        {
            swingCollider.EnemyReached += StartAttacking;
            swingCollider.EnemyAbandoned += StopAttacking;

            NewEnemySelected += ChangeTarget;

            GetComponent<EntityController>().MaxHP = GetComponent<EntityController>().Affiliation == 1 ? HpP1 : HpP2;
            GetComponent<EntityController>().CurrentHP = GetComponent<EntityController>().Affiliation == 1 ? HpP1 : HpP2;
        }

        public override void MoveTo(Vector3 point)
        {
            GetComponent<NavMeshAgent>().SetDestination(point);
            Enemy = null;
            if (coroutine != null)
            {
                StopAttacking(this, null);
            }
        }

        private void ChangeTarget(object sender, EventArgs args)
        {
            if (swingCollider.CheckForEnemy())
            {
                StartAttacking(this, null);
            }
        }

        public override void GoAttack(GameObject target)
        {
            base.GoAttack(target);
            if (target.GetComponent<UnitController>() != null)
            {
                GetComponent<NavMeshAgent>().SetDestination(target.transform.position);
            }
            else
            {
                GetComponent<NavMeshAgent>().SetDestination(target.GetComponent<BoxCollider>().ClosestPoint(transform.position));
            }
            Enemy = target;
        }

        private void StartAttacking(object sender, EventArgs args)
        {
            GetComponent<NavMeshAgent>().ResetPath();
            coroutine = StartCoroutine(Attack());

        }

        private void StopAttacking(object sender, EventArgs args)
        {
            StopCoroutine(coroutine);
        }

        public override IEnumerator Attack()
        {
            while (true)
            {
                transform.LookAt(Enemy.transform);
                yield return new WaitForSeconds(fireDelay);
                if (Enemy == null)
                {
                    yield break;
                }
                transform.LookAt(Enemy.transform);
                var arrow = Instantiate(Torpedo, transform.position + new Vector3(0, 10f, 15f), transform.rotation);
                arrow.GetComponent<Rigidbody>().AddRelativeForce(0, 0, 75, ForceMode.Impulse);
                arrow.GetComponent<ArrowController>().Attack = attack;
                arrow.GetComponent<ArrowController>().Affiliation = GetComponent<EntityController>().Affiliation;
                arrow.GetComponent<ArrowController>().shouldDestroy = GetComponent<EntityController>().Affiliation == 1 ? !DockController.HeatingResearchedP1 : !DockController.HeatingResearchedP2;
                Destroy(arrow, 10);
            }
        }

    }
}
