using System;
using System.Collections;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.AI;

namespace Forgeage
{
	public class HeroController : MilitaryController
	{
        public ObservableCollection<Ability> abilities = new ObservableCollection<Ability>();

        public float actionDelay = 1f;
        public int attack = 30;
        public float abilityDelay = 3f;
        public float healRange = 10f;
        public bool isDead = false;

        private EntityController entityController;
        public SwingCollider swingCollider;
        public GameObject Arrow;
        public GameObject BigArrow;
        public ReviveController reviveController;

        private Coroutine attackCoroutine;
        private Coroutine regenerateCoroutine;
        

        new void Start()
        {
            entityController = GetComponent<EntityController>();
            swingCollider.EnemyReached += StartAttacking;
            swingCollider.EnemyAbandoned += StopAttacking;

            NewEnemySelected += ChangeTarget;
            regenerateCoroutine = StartCoroutine(RegenerateHP());

            AddAbilities();
        }

        public void Heal(GameObject hero)
        {
            foreach(Collider c in Physics.OverlapSphere(transform.position, healRange, ActionController.buildingsLayerMask | ActionController.unitsLayerMask))
            {
                if(c.GetComponent<EntityController>() != null && c.GetComponent<EntityController>().Affiliation == GetComponent<EntityController>().Affiliation)
                {
                    c.GetComponent<EntityController>().CurrentHP += 150;
                }
            }
        }

        public void FireBigArrow(GameObject hero)
        {
            var torpedo = Instantiate(BigArrow, transform.position + new Vector3(0, 1f, 0), Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(-15, 0, 0)));
            torpedo.GetComponent<Rigidbody>().AddRelativeForce(0, 0, 30, ForceMode.Impulse);
            torpedo.GetComponent<ArrowController>().Attack = attack * 5;
            torpedo.GetComponent<ArrowController>().Affiliation = GetComponent<EntityController>().Affiliation;
            torpedo.GetComponent<ArrowController>().shouldDestroy = false;
            Destroy(torpedo, 10);
        }

        public IEnumerator RegenerateHP()
        {
            while(true)
            {
                yield return new WaitForSeconds(actionDelay);
                entityController.CurrentHP += 3;
            }
        }

        public override void MoveTo(Vector3 point)
        {
            GetComponent<NavMeshAgent>().SetDestination(point);
            Enemy = null;
            if (attackCoroutine != null)
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
            attackCoroutine = StartCoroutine(Attack());
        }

        private void StopAttacking(object sender, EventArgs args)
        {
            StopCoroutine(attackCoroutine);
        }

        public override IEnumerator Attack()
        {
            while (true)
            {
                transform.LookAt(Enemy.transform);
                yield return new WaitForSeconds(actionDelay);
                if (Enemy == null)
                {
                    yield break;
                }
                transform.LookAt(Enemy.transform);
                var arrow = Instantiate(Arrow, transform.position + new Vector3(0, 1.5f, 1f), Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(-5, 0, 0)));
                var lookat = Enemy.transform.position;
                arrow.transform.LookAt(lookat);
                arrow.GetComponent<Rigidbody>().AddRelativeForce(0, 0, 75, ForceMode.Impulse);
                arrow.GetComponent<ArrowController>().Attack = attack;
                arrow.GetComponent<ArrowController>().Affiliation = GetComponent<EntityController>().Affiliation;
                Destroy(arrow, 10);
            }
        }

        public override void Die()
        {
            GetComponent<CapsuleCollider>().enabled = false;
            GetComponent<NavMeshAgent>().enabled = false;
            isDead = true;
            ClearAbilities();
        }

        public void Revive()
        {
            GetComponent<CapsuleCollider>().enabled = true;
            GetComponent<NavMeshAgent>().enabled = true;
            isDead = false;
            AddAbilities();
        }

        private void AddAbilities()
        {
            abilities.Add(new Ability(Spawner.Instance.Entities.IcHeal, gameObject, Heal, abilityDelay));
            abilities.Add(new Ability(Spawner.Instance.Entities.IcBigArrow, gameObject, FireBigArrow, abilityDelay));
        }

        private void ClearAbilities()
        {
            abilities.Clear();
        }
    }
}
