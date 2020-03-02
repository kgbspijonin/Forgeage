using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Forgeage
{
	public class ArcheryRangeController : BuildingController
	{
        private static bool compositeResearchedP1;
        private static bool compositeResearchedP2;
        private static bool craftsmanshipResearchedP1;
        private static bool craftsmanshipResearchedP2;

        public static bool CompositeResearchedP1 {
            get {
                return compositeResearchedP1;
            }
            set {
                compositeResearchedP1 = value;
                UpdateResearches(Spawner.Instance.Entities.player1entities, value, Spawner.Instance.Entities.IcComposite);
            }
        }
        public static bool CompositeResearchedP2 {
            get {
                return compositeResearchedP2;
            }
            set {
                compositeResearchedP2 = value;
                UpdateResearches(Spawner.Instance.Entities.player2entities, value, Spawner.Instance.Entities.IcComposite);
            }
        }
        public static bool CraftsmanshipResearchedP1 {
            get {
                return craftsmanshipResearchedP1;
            }
            set {
                craftsmanshipResearchedP1 = value;
                UpdateResearches(Spawner.Instance.Entities.player1entities, value, Spawner.Instance.Entities.IcCraftsmanship);
            }
        }
        public static bool CraftsmanshipResearchedP2 {
            get {
                return craftsmanshipResearchedP2;
            }
            set {
                craftsmanshipResearchedP2 = value;
                UpdateResearches(Spawner.Instance.Entities.player2entities, value, Spawner.Instance.Entities.IcCraftsmanship);
            }
        }

        public static void UpdateResearches(List<GameObject> entities, bool shouldRemove, Sprite texture)
        {
            if (shouldRemove)
            {
                foreach (var e in entities)
                {
                    if (e.GetComponent<ArcheryRangeController>() != null)
                    {
                        e.GetComponent<ArcheryRangeController>().Actions.Remove(
                            e.GetComponent<ArcheryRangeController>().Actions.FirstOrDefault(act => ((Action)act).UITexture == texture));
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
            Actions.Add(new Action(Spawner.Instance.Entities.Archer.GetComponent<EntityController>().UITexture, CreateArcher));
            if (GetComponent<EntityController>().Affiliation == 1 && !CompositeResearchedP1)
            {
                Actions.Add(new Action(Spawner.Instance.Entities.IcComposite, QueueComposite));
            }
            else if (GetComponent<EntityController>().Affiliation == 2 && !CompositeResearchedP2)
            {
                Actions.Add(new Action(Spawner.Instance.Entities.IcComposite, QueueComposite));
            }

            if (GetComponent<EntityController>().Affiliation == 1 && !CraftsmanshipResearchedP1)
            {
                Actions.Add(new Action(Spawner.Instance.Entities.IcCraftsmanship, QueueCraftsmanship));
            }
            else if (GetComponent<EntityController>().Affiliation == 2 && !CraftsmanshipResearchedP2)
            {
                Actions.Add(new Action(Spawner.Instance.Entities.IcCraftsmanship, QueueCraftsmanship));
            }
        }

        private void QueueComposite()
        {
            if (GetComponent<EntityController>().Affiliation == 1)
            {
                if (!CompositeResearchedP1)
                {
                    if (ResourcesStockpileController.Instance.AttemptTransaction(woodC: 125, goldC: 100))
                    {
                        CompositeResearchedP1 = true;
                        queue.Add(
                            new TimedAction(gameObject, new ManagedAction(
                                gameObject,
                                Spawner.Instance.Entities.IcComposite,
                                ResearchComposite), researchTime));
                    }
                }
            }
            else
            {
                if (!CompositeResearchedP2)
                {
                    if (ResourcesStockpileController.Instance.AttemptTransaction(woodC: 125, goldC: 75))
                    {
                        CompositeResearchedP2 = true;
                        queue.Add(
                            new TimedAction(gameObject, new ManagedAction(
                                gameObject,
                                Spawner.Instance.Entities.IcComposite,
                                ResearchComposite), researchTime));
                    }
                }
            }
        }

        private void ResearchComposite(GameObject manager)
        {
            if (GetComponent<EntityController>().Affiliation == 1)
            {
                ArcherController.FireDelayP1 /= 1.5f;
            }
            else
            {
                ArcherController.FireDelayP2 /= 1.5f;
            }
        }

        private void QueueCraftsmanship()
        {
            if (GetComponent<EntityController>().Affiliation == 1)
            {
                if (!CraftsmanshipResearchedP1)
                {
                    if (ResourcesStockpileController.Instance.AttemptTransaction(foodC: 125, goldC: 100))
                    {
                        CraftsmanshipResearchedP1 = true;
                        queue.Add(
                            new TimedAction(gameObject, new ManagedAction(
                                gameObject,
                                Spawner.Instance.Entities.IcCraftsmanship,
                                ResearchCraftsmanship), researchTime));
                    }
                }
            }
            else
            {
                if (!CraftsmanshipResearchedP2)
                {
                    if (ResourcesStockpileController.Instance.AttemptTransaction(foodC: 125, goldC: 100))
                    {
                        CraftsmanshipResearchedP2 = true;
                        queue.Add(
                            new TimedAction(gameObject, new ManagedAction(
                                gameObject,
                                Spawner.Instance.Entities.IcCraftsmanship,
                                ResearchCraftsmanship), researchTime));
                    }
                }
            }
        }

        private void ResearchCraftsmanship(GameObject manager)
        {
            if (GetComponent<EntityController>().Affiliation == 1)
            {
                ArcherController.RangeP1 *= 1.25f;
                ArcherController.AttackP1 = Mathf.FloorToInt(ArcherController.AttackP1 * 1.25f);
            }
            else
            {
                ArcherController.RangeP2 *= 1.25f;
                ArcherController.AttackP2 = Mathf.FloorToInt(ArcherController.AttackP2 * 1.25f);
            }
        }

        void CreateArcher()
        {
            foreach (var go in ActionHandler.Instance.Selected)
            {
                if (go.GetComponent<ArcheryRangeController>() != null)
                {
                    if (ResourcesStockpileController.Instance.AttemptTransaction(
                        foodC: ArcherController.FoodPriceP1,
                        woodC: ArcherController.WoodPriceP1,
                        goldC: ArcherController.GoldPriceP1))
                    {
                        go.GetComponent<ArcheryRangeController>().queue.Add(
                            new TimedAction(go.gameObject, new ManagedAction(
                                go.gameObject,
                                Spawner.Instance.Entities.Archer.GetComponent<EntityController>().UITexture,
                                SpawnArcher), trainTime));
                    }
                }
            }
        }

        void SpawnArcher(GameObject archeryrange)
        {
            Spawner.Instance.Entities.Spawn(Entities.Entity.Archer, GetComponent<ArcheryRangeController>().SpawnPos, GetComponent<EntityController>().Affiliation);
        }

        public override void UpdateBuildPoints()
        {
            transform.localScale = new Vector3(transform.localScale.x,
                transform.localScale.y,
                Mathf.Max(startingScaleY / 10, startingScaleY * Mathf.Max(1, CurrentBuildPoints) / Mathf.Max(1, TotalBuildPoints)));
        }
    }
}
