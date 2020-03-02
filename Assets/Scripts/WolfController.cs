using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Forgeage
{
    public class WolfController : MilitaryController
    {
        public int attack = 30;

        public SwingCollider swingCollider;
        public EnemyFinder enemyFinder;

        public Coroutine coroutine;
        public Coroutine routeCoroutine;

        new void Start()
        {
            swingCollider.EnemyReached += StartAttacking;
            swingCollider.EnemyAbandoned += StopAttacking;

            NewEnemySelected += FindTarget;
        }

        private void FindTarget(object sender, EventArgs e)
        {
            if(Enemy == null)
            {
                enemyFinder.Scan();
            }
            else
            {
                GoAttack(enemy);
            }
        }

        public override void GoAttack(GameObject target)
        {
            base.GoAttack(target);
            if (target.GetComponent<UnitController>() != null)
            {
                routeCoroutine = StartCoroutine(GoTowardsEnemy());
            }
            else
            {
                Enemy = null;
            }
        }

        private IEnumerator GoTowardsEnemy()
        {
            while(true)
            {
                GetComponent<NavMeshAgent>().SetDestination(Enemy.transform.position);
                yield return new WaitForSeconds(0.25f);
            }
            
        }

        private void StopAttacking(object sender, EventArgs e)
        {
            StopCoroutine(coroutine);
        }

        private void StartAttacking(object sender, EventArgs e)
        {
            coroutine = StartCoroutine(Attack());
        }

        public override IEnumerator Attack()
        {
            while (true)
            {
                yield return new WaitForSeconds(1);
                if (Enemy.GetComponent<EntityController>() != null)
                {
                    Enemy.GetComponent<EntityController>().CurrentHP -= attack;
                }
            }
        }


    }
}
