using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Forgeage
{
	public class SwordsmanController : MilitaryController
    {
        private static float speedP1 = 5f;
        private static float speedP2 = 5f;
        private static int attackP1 = 25;
        private static int attackP2 = 25;
        private static int hpP1 = 100;
        private static int hpP2 = 100;

        public static int FoodPriceP1 = 40;
        public static int WoodPriceP1 = 0;
        public static int GoldPriceP1 = 10;
        public static int FoodPriceP2 = 40;
        public static int WoodPriceP2 = 0;
        public static int GoldPriceP2 = 10;

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

        public static float SpeedP1 {
            get {
                return speedP1;
            }
            set {
                speedP1 = value;
                UpdateSpeed(Spawner.Instance.Entities.player1entities);
            }
        }

        public static float SpeedP2 {
            get {
                return speedP2;
            }
            set {
                speedP2 = value;
                UpdateSpeed(Spawner.Instance.Entities.player2entities);
            }
        }

        public static void UpdateAttack(List<GameObject> entities)
        {
            foreach (var go in entities)
            {
                if (go.GetComponent<SwordsmanController>() != null)
                {
                    go.GetComponent<SwordsmanController>().attack = go.GetComponent<EntityController>().Affiliation == 1 ? AttackP1 : AttackP2;
                }
            }
        }

        private static void UpdateHP(List<GameObject> entities)
        {
            foreach (var go in entities)
            {
                if (go.GetComponent<SwordsmanController>() != null)
                {
                    go.GetComponent<EntityController>().MaxHP = go.GetComponent<EntityController>().Affiliation == 1 ? HpP1 : HpP2;
                    go.GetComponent<EntityController>().CurrentHP = go.GetComponent<EntityController>().Affiliation == 1 ? HpP1 : HpP2;
                }
            }
        }

        private static void UpdateSpeed(List<GameObject> entities)
        {
            foreach (var go in entities)
            {
                if (go.GetComponent<SwordsmanController>() != null)
                {
                    go.GetComponent<NavMeshAgent>().speed = go.GetComponent<EntityController>().Affiliation == 1 ? SpeedP1 : SpeedP2;
                }
            }
        }

        public int attack;

        public SwingCollider swingCollider;

        private Coroutine coroutine;

        new void Start()
        {
            swingCollider.EnemyReached += StartAttacking;
            swingCollider.EnemyAbandoned += StopAttacking;

            GetComponent<EntityController>().MaxHP = GetComponent<EntityController>().Affiliation == 1 ? HpP1 : HpP2;
            GetComponent<EntityController>().CurrentHP = GetComponent<EntityController>().Affiliation == 1 ? HpP1 : HpP2;

            GetComponent<NavMeshAgent>().speed = GetComponent<EntityController>().Affiliation == 1 ? SpeedP1 : SpeedP2;
            attack = GetComponent<EntityController>().Affiliation == 1 ? AttackP1 : AttackP2;

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
            coroutine = StartCoroutine(Attack());
        }

        private void StopAttacking(object sender, EventArgs args)
        {
            StopCoroutine(coroutine);
        }

        public override IEnumerator Attack()
        {
            while(true)
            {
                yield return new WaitForSeconds(1);
                if (Enemy.GetComponent<EntityController>() != null)
                {
                    Enemy.GetComponent<EntityController>().CurrentHP -= GetComponent<EntityController>().Affiliation == 1 ? AttackP1 : AttackP2;
                }
            }
        }
    }
}
