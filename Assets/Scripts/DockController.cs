using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Forgeage
{
	public class DockController : BuildingController
	{
        public Vector3 ColliderBounds;

        private static bool platingResearchedP1;
        private static bool platingResearchedP2;
        private static bool heatingResearchedP1;
        private static bool heatingResearchedP2;

        public static bool PlatingResearchedP1 {
            get {
                return platingResearchedP1;
            }
            set {
                platingResearchedP1 = value;
                UpdateResearches(Spawner.Instance.Entities.player1entities, value, Spawner.Instance.Entities.IcPlating);
            }
        }
        public static bool PlatingResearchedP2 {
            get {
                return platingResearchedP2;
            }
            set {
                platingResearchedP2 = value;
                UpdateResearches(Spawner.Instance.Entities.player2entities, value, Spawner.Instance.Entities.IcPlating);
            }
        }
        public static bool HeatingResearchedP1 {
            get {
                return heatingResearchedP1;
            }
            set {
                heatingResearchedP1 = value;
                UpdateResearches(Spawner.Instance.Entities.player1entities, value, Spawner.Instance.Entities.IcHeating);
            }
        }
        public static bool HeatingResearchedP2 {
            get {
                return heatingResearchedP2;
            }
            set {
                heatingResearchedP2 = value;
                UpdateResearches(Spawner.Instance.Entities.player2entities, value, Spawner.Instance.Entities.IcHeating);
            }
        }

        public static void UpdateResearches(List<GameObject> entities, bool shouldRemove, Sprite texture)
        {
            if (shouldRemove)
            {
                foreach (var e in entities)
                {
                    if (e.GetComponent<DockController>() != null)
                    {
                        e.GetComponent<DockController>().Actions.Remove(
                            e.GetComponent<DockController>().Actions.FirstOrDefault(act => ((Action)act).UITexture == texture));
                    }
                }
            }
        }

        void Start()
        {
            queue.CollectionChanged += HandleQueue;
            FindSpawnPos();
        }

        public override void OnCompleted()
        {
            Actions.Add(new Action(Spawner.Instance.Entities.Galley.GetComponent<EntityController>().UITexture, CreateGalley));
            if (GetComponent<EntityController>().Affiliation == 1 && !PlatingResearchedP1)
            {
                Actions.Add(new Action(Spawner.Instance.Entities.IcPlating, QueuePlating));
            }
            else if (GetComponent<EntityController>().Affiliation == 2 && !PlatingResearchedP2)
            {
                Actions.Add(new Action(Spawner.Instance.Entities.IcPlating, QueuePlating));
            }

            if (GetComponent<EntityController>().Affiliation == 1 && !HeatingResearchedP1)
            {
                Actions.Add(new Action(Spawner.Instance.Entities.IcHeating, QueueHeating));
            }
            else if (GetComponent<EntityController>().Affiliation == 2 && !HeatingResearchedP2)
            {
                Actions.Add(new Action(Spawner.Instance.Entities.IcHeating, QueueHeating));
            }
        }

        void CreateGalley()
        {
            foreach (var go in ActionHandler.Instance.Selected)
            {
                if (go.GetComponent<DockController>() != null)
                {
                    if (ResourcesStockpileController.Instance.AttemptTransaction(woodC: 75, goldC: 25))
                    {
                        go.GetComponent<DockController>().queue.Add(
                            new TimedAction(go.gameObject, new ManagedAction(
                                go.gameObject,
                                Spawner.Instance.Entities.Galley.GetComponent<EntityController>().UITexture,
                                SpawnGalley), trainTime));
                    }
                }
            }
        }

        void QueuePlating()
        {
            if (GetComponent<EntityController>().Affiliation == 1)
            {
                if (!PlatingResearchedP1)
                {
                    if (ResourcesStockpileController.Instance.AttemptTransaction(woodC: 125, goldC: 100))
                    {
                        PlatingResearchedP1 = true;
                        queue.Add(
                            new TimedAction(gameObject, new ManagedAction(
                                gameObject,
                                Spawner.Instance.Entities.IcPlating,
                                ResearchPlating), researchTime));
                    }
                }
            }
            else
            {
                if (!PlatingResearchedP2)
                {
                    if (ResourcesStockpileController.Instance.AttemptTransaction(woodC: 125, goldC: 100))
                    {
                        PlatingResearchedP2 = true;
                        queue.Add(
                            new TimedAction(gameObject, new ManagedAction(
                                gameObject,
                                Spawner.Instance.Entities.IcPlating,
                                ResearchPlating), researchTime));
                    }
                }
            }
        }

        void QueueHeating()
        {
            if (GetComponent<EntityController>().Affiliation == 1)
            {
                if (!HeatingResearchedP1)
                {
                    if (ResourcesStockpileController.Instance.AttemptTransaction(foodC: 175, goldC: 50))
                    {
                        HeatingResearchedP1 = true;
                        queue.Add(
                            new TimedAction(gameObject, new ManagedAction(
                                gameObject,
                                Spawner.Instance.Entities.IcHeating,
                                ResearchHeating), researchTime));
                    }
                }
            }
            else
            {
                if (!HeatingResearchedP2)
                {
                    if (ResourcesStockpileController.Instance.AttemptTransaction(foodC: 175, goldC: 50))
                    {
                        HeatingResearchedP1 = true;
                        queue.Add(
                            new TimedAction(gameObject, new ManagedAction(
                                gameObject,
                                Spawner.Instance.Entities.IcHeating,
                                ResearchHeating), researchTime));
                    }
                }
            }
        }

        void ResearchPlating(GameObject dock)
        {
            if (GetComponent<EntityController>().Affiliation == 1)
            {
                GalleyController.HpP1 = Mathf.RoundToInt(GalleyController.HpP1 * 1.5f);
            }
            else
            {
                GalleyController.HpP2 = Mathf.RoundToInt(GalleyController.HpP2 * 1.5f);
            }
        }

        void ResearchHeating(GameObject dock)
        {
            if (GetComponent<EntityController>().Affiliation == 1)
            {
                heatingResearchedP1 = true;
            }
            else
            {
                heatingResearchedP1 = true;
            }
        }

        void SpawnGalley(GameObject dock)
        {
            Spawner.Instance.Entities.Spawn(Entities.Entity.Galley, dock.GetComponent<DockController>().SpawnPos, GetComponent<EntityController>().Affiliation);
        }


        public override void UpdateBuildPoints()
        {
            GetComponent<BoxCollider>().size = new Vector3(ColliderBounds.x,
                ColliderBounds.y * (Mathf.Max(1, TotalBuildPoints) / Mathf.Max(10, CurrentBuildPoints)),
                ColliderBounds.z);
            transform.localScale = new Vector3(transform.localScale.x, Mathf.Max(startingScaleY / 10, startingScaleY * Mathf.Max(1, CurrentBuildPoints) / Mathf.Max(1, TotalBuildPoints)), transform.localScale.z);
        }

        void FindSpawnPos()
        {
            if(Physics.OverlapSphere(transform.position + new Vector3(0, 0, -5), 2, ActionController.terrainLayerMask).Length == 0)
            {
                SpawnPos = transform.position + new Vector3(0, 0, -5);
            }
            else if (Physics.OverlapSphere(transform.position + new Vector3(0, 0, 5), 2, ActionController.terrainLayerMask).Length == 0)
            {
                SpawnPos = transform.position + new Vector3(0, 0, 5);
            }
            else if (Physics.OverlapSphere(transform.position + new Vector3(-5, 0, 0), 2, ActionController.terrainLayerMask).Length == 0)
            {
                SpawnPos = transform.position + new Vector3(-5, 0, 0);
            }
            else if (Physics.OverlapSphere(transform.position + new Vector3(5, 0, 0), 2, ActionController.terrainLayerMask).Length == 0)
            {
                SpawnPos = transform.position + new Vector3(5, 0, 0);
            }
            else
            {
                SpawnPos = transform.position;
            }
        }
    }
}
