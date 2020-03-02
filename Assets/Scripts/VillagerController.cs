using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Forgeage
{
	public class VillagerController : UnitController
    {
        public ResourceFinder ResourceFinder;

        private GameObject resourceDestination;
        public event EventHandler NewResourceSelected;

        private Coroutine coroutine;

        public GameObject ResourceDestination {
            get { return resourceDestination; }
            set {
                resourceDestination = value;
                NewResourceSelected?.Invoke(this, EventArgs.Empty);
            }
        }

        private float gatheringDelay = 1;

        private static float speedP1 = 4f;
        private static float speedP2 = 4f;
        private static float gatheringDelayP1 = 1f;
        private static float gatheringDelayP2 = 1f;

        public static float SpeedP1 {
            get {
                return speedP1;
            } set {
                speedP1 = value;
                UpdateSpeed(Spawner.Instance.Entities.player1entities, speedP1);
            }
        }
        public static float SpeedP2 {
            get {
                return speedP2;
            } set {
                speedP2 = value;
                UpdateSpeed(Spawner.Instance.Entities.player2entities, speedP2);
            }
        }

        public static float GatheringDelayP1 {
            get {
                return gatheringDelayP1;
            }
            set {
                gatheringDelayP1 = value;
                UpdateGatheringSpeed(Spawner.Instance.Entities.player1entities, gatheringDelayP1);

            }
        }
        public static float GatheringDelayP2 {
            get {
                return gatheringDelayP2;
            }
            set {
                gatheringDelayP2 = value;
                UpdateGatheringSpeed(Spawner.Instance.Entities.player2entities, gatheringDelayP2);
            }
        }

        public static void UpdateGatheringSpeed(List<GameObject> entities, float value)
        {
            foreach (var go in entities)
            {
                if (go.GetComponent<VillagerController>() != null)
                {
                    go.GetComponent<VillagerController>().gatheringDelay = value;
                }
            }
        }

        public static void UpdateSpeed(List<GameObject> entities, float value)
        {
            foreach (var go in entities)
            {
                if (go.GetComponent<VillagerController>() != null)
                {
                    go.GetComponent<NavMeshAgent>().speed = value;
                }
            }
        }

        public void Start()
        {
            ResourceFinder.ResourceReached += StartWorking;
            ResourceFinder.ResourceAbandoned += StopWorking;

            NewResourceSelected += AttemptGathering;

            Actions.Add(new Action(Spawner.Instance.Entities.TownCenter.GetComponent<EntityController>().UITexture, BuildTownCenter));
            Actions.Add(new Action(Spawner.Instance.Entities.Barracks.GetComponent<EntityController>().UITexture, BuildBarracks));
            Actions.Add(new Action(Spawner.Instance.Entities.ArcheryRange.GetComponent<EntityController>().UITexture, BuildArcheryRange));
            Actions.Add(new Action(Spawner.Instance.Entities.Farm.GetComponent<EntityController>().UITexture, BuildFarm));
            Actions.Add(new Action(Spawner.Instance.Entities.Dock.GetComponent<EntityController>().UITexture, BuildDock));

            GetComponent<NavMeshAgent>().speed = GetComponent<EntityController>().Affiliation == 1 ? SpeedP1 : SpeedP2;
            gatheringDelay = GetComponent<EntityController>().Affiliation == 1 ? GatheringDelayP1 : GatheringDelayP2;
        }

        void AttemptGathering(object sender, EventArgs args)
        {
            if(ResourceFinder.CheckForResource())
            {
                if(coroutine != null)
                {
                    StopCoroutine(coroutine);
                }
                coroutine = StartCoroutine(Work());
            }
        }

        void BuildTownCenter()
        {
            if(ResourcesStockpileController.Instance.AttemptTransaction(woodC: 250))
            {
                ActionController.Instance.InitBuilding(Entities.Entity.TownCenter);
            }
        }

        void BuildBarracks()
        {
            if(ResourcesStockpileController.Instance.AttemptTransaction(woodC: 100))
            {
                ActionController.Instance.InitBuilding(Entities.Entity.Barracks);
            }
        }

        void BuildArcheryRange()
        {
            if (ResourcesStockpileController.Instance.AttemptTransaction(woodC: 100))
            {
                ActionController.Instance.InitBuilding(Entities.Entity.ArcheryRange);
            }
        }

        void BuildFarm()
        {
            if (ResourcesStockpileController.Instance.AttemptTransaction(woodC: 25))
            {
                ActionController.Instance.InitBuilding(Entities.Entity.Farm);
            }
        }

        void BuildDock()
        {
            if (ResourcesStockpileController.Instance.AttemptTransaction(woodC: 100))
            {
                ActionController.Instance.InitBuilding(Entities.Entity.Dock);
            }
        }

        public override void MoveTo(Vector3 point)
        {
            base.MoveTo(point);
            ResourceDestination = null;
            if(coroutine != null)
            {
                StopCoroutine(coroutine);
            }
            coroutine = null;
        }

        public void GoWork(GameObject res)
        {
            if (res.GetComponent<ResourceController>() != null)
            {
                GetComponent<NavMeshAgent>().SetDestination(res.transform.position);
            }
            else
            {
                GetComponent<NavMeshAgent>().SetDestination(res.GetComponent<BoxCollider>().ClosestPoint(transform.position));
            }
            ResourceDestination = res;
        }

        public void StartWorking(object obj, EventArgs args)
        {
            coroutine = StartCoroutine(Work());
        }

        public void StopWorking(object obj, EventArgs args)
        {
            StopCoroutine(coroutine);
        }

        public IEnumerator Work()
        {
            while(true)
            {
                yield return new WaitForSeconds(gatheringDelay);
                if(resourceDestination != null)
                {
                    if(resourceDestination.GetComponent<CowController>() != null)
                    {
                        ResourcesStockpileController.Instance.Food += resourceDestination.GetComponent<ResourceController>().GatherResource(this);
                    }
                    else if (resourceDestination.GetComponent<FarmController>() != null && ResourceDestination.GetComponent<BuildingController>().IsCompleted)
                    {
                        ResourcesStockpileController.Instance.Food += resourceDestination.GetComponent<ResourceController>().GatherResource(this);
                    }
                    else if (ResourceDestination.GetComponent<BuildingController>() != null && !ResourceDestination.GetComponent<BuildingController>().IsCompleted)
                    {
                        ResourceDestination.GetComponent<BuildingController>().Build(this);
                    }
                    else if (ResourceDestination.GetComponent<TreeController>() != null)
                    {
                        ResourcesStockpileController.Instance.Wood += resourceDestination.GetComponent<ResourceController>().GatherResource(this);
                    }
                    else if (ResourceDestination.GetComponent<GoldController>() != null)
                    {
                        ResourcesStockpileController.Instance.Gold += resourceDestination.GetComponent<ResourceController>().GatherResource(this);
                    }
                }
                else
                {
                    StopCoroutine(coroutine);
                    yield break;
                }
            }
        }
	}
}
