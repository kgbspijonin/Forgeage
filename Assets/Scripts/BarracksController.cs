using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Forgeage
{
	public class BarracksController : BuildingController
	{
        private static bool knighthoodResearchedP1;
        private static bool knighthoodResearchedP2;
        private static bool armorResearchedP1;
        private static bool armorResearchedP2;

        public static bool KnighthoodResearchedP1 {
            get {
                return knighthoodResearchedP1;
            }
            set {
                knighthoodResearchedP1 = value;
                UpdateResearches(Spawner.Instance.Entities.player1entities, value, Spawner.Instance.Entities.IcKnighthood);
            }
        }
        public static bool KnighthoodResearchedP2 {
            get {
                return knighthoodResearchedP2;
            }
            set {
                knighthoodResearchedP2 = value;
                UpdateResearches(Spawner.Instance.Entities.player2entities, value, Spawner.Instance.Entities.IcKnighthood);
            }
        }
        public static bool ArmorResearchedP1 {
            get {
                return armorResearchedP1;
            }
            set {
                armorResearchedP1 = value;
                UpdateResearches(Spawner.Instance.Entities.player1entities, value, Spawner.Instance.Entities.IcArmor);
            }
        }
        public static bool ArmorResearchedP2 {
            get {
                return armorResearchedP2;
            }
            set {
                armorResearchedP2 = value;
                UpdateResearches(Spawner.Instance.Entities.player2entities, value, Spawner.Instance.Entities.IcArmor);
            }
        }

        public static void UpdateResearches(List<GameObject> entities, bool shouldRemove, Sprite texture)
        {
            if (shouldRemove)
            {
                foreach (var e in entities)
                {
                    if (e.GetComponent<BarracksController>() != null)
                    {
                        e.GetComponent<BarracksController>().Actions.Remove(
                            e.GetComponent<BarracksController>().Actions.FirstOrDefault(act => ((Action)act).UITexture == texture));
                    }
                }
            }
        }

        void Start()
        {
            queue.CollectionChanged += HandleQueue;
            SpawnPos = transform.position + new Vector3(0, 0, -10);
        }

        public override void OnCompleted()
        {
            Actions.Add(new Action(Spawner.Instance.Entities.Swordsman.GetComponent<EntityController>().UITexture, CreateSwordsman));
            if (GetComponent<EntityController>().Affiliation == 1 && !KnighthoodResearchedP1)
            {
                Actions.Add(new Action(Spawner.Instance.Entities.IcKnighthood, QueueKnighthood));
            }
            else if (GetComponent<EntityController>().Affiliation == 2 && !KnighthoodResearchedP2)
            {
                Actions.Add(new Action(Spawner.Instance.Entities.IcKnighthood, QueueKnighthood));
            }

            if (GetComponent<EntityController>().Affiliation == 1 && !ArmorResearchedP1)
            {
                Actions.Add(new Action(Spawner.Instance.Entities.IcArmor, QueueArmor));
            }
            else if (GetComponent<EntityController>().Affiliation == 2 && !ArmorResearchedP2)
            {
                Actions.Add(new Action(Spawner.Instance.Entities.IcArmor, QueueArmor));
            }
        }

        private void QueueKnighthood()
        {
            if (GetComponent<EntityController>().Affiliation == 1)
            {
                if (!KnighthoodResearchedP1)
                {
                    if (ResourcesStockpileController.Instance.AttemptTransaction(foodC: 150, goldC: 75))
                    {
                        KnighthoodResearchedP1 = true;
                        queue.Add(
                            new TimedAction(gameObject, new ManagedAction(
                                gameObject,
                                Spawner.Instance.Entities.IcKnighthood,
                                ResearchKnighthood), researchTime));
                    }
                }
            }
            else
            {
                if (!KnighthoodResearchedP2)
                {
                    if (ResourcesStockpileController.Instance.AttemptTransaction(foodC: 150, goldC: 75))
                    {
                        KnighthoodResearchedP2 = true;
                        queue.Add(
                            new TimedAction(gameObject, new ManagedAction(
                                gameObject,
                                Spawner.Instance.Entities.IcKnighthood,
                                ResearchKnighthood), researchTime));
                    }
                }
            }
        }

        private void ResearchKnighthood(GameObject manager)
        {
            if (GetComponent<EntityController>().Affiliation == 1)
            {

                SwordsmanController.SpeedP1 *= 1.25f;
                SwordsmanController.AttackP1 = Mathf.RoundToInt(SwordsmanController.AttackP1 * 1.25f);
            }
            else
            {
                SwordsmanController.SpeedP2 *= 1.25f;
                SwordsmanController.AttackP2 = Mathf.RoundToInt(SwordsmanController.AttackP2 * 1.25f);
            }
        }

        private void QueueArmor()
        {
            if (GetComponent<EntityController>().Affiliation == 1)
            {
                if (!ArmorResearchedP1)
                {
                    if (ResourcesStockpileController.Instance.AttemptTransaction(woodC: 150, goldC: 75))
                    {
                        ArmorResearchedP1 = true;
                        queue.Add(
                            new TimedAction(gameObject, new ManagedAction(
                                gameObject,
                                Spawner.Instance.Entities.IcArmor,
                                ResearchArmor), researchTime));
                    }
                }
            }
            else
            {
                if (!ArmorResearchedP2)
                {
                    if (ResourcesStockpileController.Instance.AttemptTransaction(woodC: 150, goldC: 75))
                    {
                        ArmorResearchedP2 = true;
                        queue.Add(
                            new TimedAction(gameObject, new ManagedAction(
                                gameObject,
                                Spawner.Instance.Entities.IcArmor,
                                ResearchArmor), researchTime));
                    }
                }
            }
        }

        private void ResearchArmor(GameObject manager)
        {
            if (GetComponent<EntityController>().Affiliation == 1)
            {
                SwordsmanController.HpP1 = Mathf.RoundToInt(SwordsmanController.HpP1 * 1.5f);
            }
            else
            {
                SwordsmanController.HpP2 = Mathf.RoundToInt(SwordsmanController.HpP2 * 1.5f);
            }
        }

        void CreateSwordsman()
        {
            foreach (var go in ActionHandler.Instance.Selected)
            {
                if (go.GetComponent<BarracksController>() != null)
                {
                    if (ResourcesStockpileController.Instance.AttemptTransaction(
                        foodC: SwordsmanController.FoodPriceP1,
                        woodC: SwordsmanController.WoodPriceP1,
                        goldC: SwordsmanController.GoldPriceP1))
                    {
                        go.GetComponent<BarracksController>().queue.Add(
                            new TimedAction(go.gameObject, new ManagedAction(
                                go.gameObject,
                                Spawner.Instance.Entities.Swordsman.GetComponent<EntityController>().UITexture,
                                SpawnSwordsman), trainTime));
                    }
                }
            }
        }

        private void SpawnSwordsman(GameObject rax)
        {
            Spawner.Instance.Entities.Spawn(Entities.Entity.Swordsman, rax.GetComponent<BarracksController>().SpawnPos, GetComponent<EntityController>().Affiliation);
        }

        public override void UpdateBuildPoints()
        {
            transform.localScale = new Vector3(transform.localScale.x,
                transform.localScale.y,
                Mathf.Max(startingScaleY / 10, startingScaleY * Mathf.Max(1, CurrentBuildPoints) / Mathf.Max(1, TotalBuildPoints)));
        }
    }
}
