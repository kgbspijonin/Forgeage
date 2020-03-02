using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Forgeage
{
    public class ArcherController : MilitaryController
    {
        public int attack = 15;
        public float range = 30;
        public float fireDelay = 2;

        private static int attackP1 = 15;
        private static int attackP2 = 15;
        private static float rangeP1 = 30;
        private static float rangeP2 = 30;
        private static float fireDelayP1 = 2;
        private static float fireDelayP2 = 2;

        public static int FoodPriceP1 = 0;
        public static int WoodPriceP1 = 25;
        public static int GoldPriceP1 = 25;
        public static int FoodPriceP2 = 0;
        public static int WoodPriceP2 = 25;
        public static int GoldPriceP2 = 25;

        public static int AttackP1 {
            get {
                return attackP1;
            }
            set {
                attackP1 = value;
                UpdateAttack(Spawner.Instance.Entities.player1entities);
            }
        }

        public static int AttackP2 {
            get {
                return attackP2;
            }
            set {
                attackP2 = value;
                UpdateAttack(Spawner.Instance.Entities.player2entities);
            }
        }

        public static float RangeP1 {
            get {
                return rangeP1;
            }
            set {
                rangeP1 = value;
                UpdateRange(Spawner.Instance.Entities.player1entities);
            }
        }

        public static float RangeP2 {
            get {
                return rangeP2;
            }
            set {
                rangeP2 = value;
                UpdateRange(Spawner.Instance.Entities.player2entities);
            }
        }

        public static float FireDelayP1 {
            get {
                return fireDelayP1;
            }
            set {
                fireDelayP1 = value;
                UpdateFireDelay(Spawner.Instance.Entities.player1entities);
            }
        }

        public static float FireDelayP2 {
            get {
                return fireDelayP2;
            }
            set {
                fireDelayP2 = value;
                UpdateFireDelay(Spawner.Instance.Entities.player2entities);
            }
        }

        public static void UpdateAttack(List<GameObject> entities)
        {
            foreach (var go in entities)
            {
                if (go.GetComponent<ArcherController>() != null)
                {
                    go.GetComponent<ArcherController>().attack = go.GetComponent<EntityController>().Affiliation == 1 ? AttackP1 : AttackP2;
                }
            }
        }

        private static void UpdateRange(List<GameObject> entities)
        {
            foreach (var go in entities)
            {
                if (go.GetComponent<ArcherController>() != null)
                {
                    go.GetComponent<ArcherController>().range = go.GetComponent<EntityController>().Affiliation == 1 ? RangeP1 : RangeP2;
                    go.GetComponent<ArcherController>().swingCollider.GetComponent<SphereCollider>().radius = go.GetComponent<ArcherController>().range;
                }
            }
        }

        private static void UpdateFireDelay(List<GameObject> entities)
        {
            foreach (var go in entities)
            {
                if (go.GetComponent<ArcherController>() != null)
                {
                    go.GetComponent<ArcherController>().fireDelay = go.GetComponent<EntityController>().Affiliation == 1 ? FireDelayP1 : FireDelayP2;
                }
            }
        }

        public SwingCollider swingCollider;
        public GameObject Arrow;

        private Coroutine coroutine;

        new void Start()
        {
            swingCollider.EnemyReached += StartAttacking;
            swingCollider.EnemyAbandoned += StopAttacking;

            NewEnemySelected += ChangeTarget;

            GetComponent<ArcherController>().attack = GetComponent<EntityController>().Affiliation == 1 ? AttackP1 : AttackP2;
            GetComponent<ArcherController>().fireDelay = GetComponent<EntityController>().Affiliation == 1 ? FireDelayP1 : FireDelayP2;
            GetComponent<ArcherController>().range = GetComponent<EntityController>().Affiliation == 1 ? RangeP1 : RangeP2;
            GetComponent<ArcherController>().swingCollider.GetComponent<SphereCollider>().radius = GetComponent<ArcherController>().range;
        }

        public override void MoveTo(Vector3 point)
        {
            GetComponent<NavMeshAgent>().SetDestination(point);
            Enemy = null;
            if(coroutine != null)
            {
                StopAttacking(this, null);
            }
        }

        private void ChangeTarget(object sender, EventArgs args)
        {
            if(swingCollider.CheckForEnemy())
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
                var arrow = Instantiate(Arrow, transform.position + new Vector3(0, 1.5f, 1f), transform.rotation);
                var lookat = Enemy.transform.position;
                arrow.transform.LookAt(lookat);
                arrow.GetComponent<Rigidbody>().AddRelativeForce(0, 0, 75, ForceMode.Impulse);
                arrow.GetComponent<ArrowController>().Attack = attack;
                arrow.GetComponent<ArrowController>().Affiliation = GetComponent<EntityController>().Affiliation;
                Destroy(arrow, 10);
            }
        }
    }
}
