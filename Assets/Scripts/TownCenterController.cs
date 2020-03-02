using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace Forgeage
{
	public class TownCenterController : BuildingController
	{
        private static bool runningResearchedP1;
        private static bool runningResearchedP2;
        private static bool calendarResearchedP1;
        private static bool calendarResearchedP2;

        public static bool RunningResearchedP1 {
            get {
                return runningResearchedP1;
            } set {
                runningResearchedP1 = value;
                UpdateResearches(Spawner.Instance.Entities.player1entities, value, Spawner.Instance.Entities.IcRunning);
            }
        }
        public static bool RunningResearchedP2 {
            get {
                return runningResearchedP2;
            }
            set {
                runningResearchedP2 = value;
                UpdateResearches(Spawner.Instance.Entities.player2entities, value, Spawner.Instance.Entities.IcRunning);
            }
        }
        public static bool CalendarResearchedP1 {
            get {
                return calendarResearchedP1;
            }
            set {
                calendarResearchedP1 = value;
                UpdateResearches(Spawner.Instance.Entities.player1entities, value, Spawner.Instance.Entities.IcCalendar);
            }
        }
        public static bool CalendarResearchedP2 {
            get {
                return calendarResearchedP2;
            }
            set {
                calendarResearchedP2 = value;
                UpdateResearches(Spawner.Instance.Entities.player2entities, value, Spawner.Instance.Entities.IcCalendar);
            }
        }

        public static void UpdateResearches(List<GameObject> entities, bool shouldRemove, Sprite texture)
        {
            if(shouldRemove)
            {
                foreach (var e in entities)
                {
                    if (e.GetComponent<TownCenterController>() != null)
                    {
                        e.GetComponent<TownCenterController>().Actions.Remove(
                            e.GetComponent<TownCenterController>().Actions.FirstOrDefault(act => ((Action)act).UITexture == texture));
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
            Actions.Add(new Action(Spawner.Instance.Entities.Villager.GetComponent<EntityController>().UITexture, CreateVillager));
            if(GetComponent<EntityController>().Affiliation == 1 && !RunningResearchedP1)
            {
                    Actions.Add(new Action(Spawner.Instance.Entities.IcRunning, QueueRunning));
            }
            else if (GetComponent<EntityController>().Affiliation == 2 && !RunningResearchedP2)
            {
                Actions.Add(new Action(Spawner.Instance.Entities.IcRunning, QueueRunning));
            }

            if (GetComponent<EntityController>().Affiliation == 1 && !CalendarResearchedP1)
            {
                Actions.Add(new Action(Spawner.Instance.Entities.IcCalendar, QueueCalendar));
            }
            else if (GetComponent<EntityController>().Affiliation == 2 && !CalendarResearchedP2)
            {
                 Actions.Add(new Action(Spawner.Instance.Entities.IcCalendar, QueueCalendar));
            }
        }

        void CreateVillager()
        {
            foreach (var go in ActionHandler.Instance.Selected)
            {
                if(go.GetComponent<TownCenterController>() != null)
                {
                    if(ResourcesStockpileController.Instance.AttemptTransaction(foodC: 50))
                    {
                        go.GetComponent<TownCenterController>().queue.Add(
                            new TimedAction(go.gameObject, new ManagedAction( 
                                go.gameObject, 
                                Spawner.Instance.Entities.Villager.GetComponent<EntityController>().UITexture, 
                                SpawnVillager), trainTime));
                    }
                }
            }
        }

        void QueueRunning()
        {
            if(GetComponent<EntityController>().Affiliation == 1)
            {
                if(!RunningResearchedP1)
                {
                    if(ResourcesStockpileController.Instance.AttemptTransaction(foodC: 150, woodC: 50))
                    {
                        RunningResearchedP1 = true;
                        queue.Add(
                            new TimedAction(gameObject, new ManagedAction(
                                gameObject,
                                Spawner.Instance.Entities.IcRunning,
                                ResearchRunning), researchTime));
                    }
                }
            }
            else
            {
                if (!RunningResearchedP2)
                {
                    if (ResourcesStockpileController.Instance.AttemptTransaction(foodC: 150, woodC: 50))
                    {
                        RunningResearchedP2 = true;
                        queue.Add(
                            new TimedAction(gameObject, new ManagedAction(
                                gameObject,
                                Spawner.Instance.Entities.IcRunning,
                                ResearchRunning), researchTime));
                    }
                }
            }
        }

        void QueueCalendar()
        {
            if (GetComponent<EntityController>().Affiliation == 1)
            {
                if (!CalendarResearchedP1)
                {
                    if (ResourcesStockpileController.Instance.AttemptTransaction(woodC: 100, goldC: 100))
                    {
                        CalendarResearchedP1 = true;
                        queue.Add(
                            new TimedAction(gameObject, new ManagedAction(
                                gameObject,
                                Spawner.Instance.Entities.IcCalendar,
                                ResearchCalendar), researchTime));
                    }
                }
            }
            else
            {
                if (!CalendarResearchedP2)
                {
                    if (ResourcesStockpileController.Instance.AttemptTransaction(woodC: 100, goldC: 100))
                    {
                        CalendarResearchedP2 = true;
                        queue.Add(
                            new TimedAction(gameObject, new ManagedAction(
                                gameObject,
                                Spawner.Instance.Entities.IcCalendar,
                                ResearchCalendar), researchTime));
                    }
                }
            }
        }

        void ResearchRunning(GameObject tc)
        {
            if (GetComponent<EntityController>().Affiliation == 1)
            {
                VillagerController.SpeedP1 *= 1.5f;
            }
            else
            {
                VillagerController.SpeedP2 *= 1.5f;
            }
        }

        void ResearchCalendar(GameObject tc)
        {
            if (GetComponent<EntityController>().Affiliation == 1)
            {
                VillagerController.GatheringDelayP1 /= 1.5f;
            }
            else
            {
                VillagerController.GatheringDelayP2 /= 1.5f;
            }
        }

        void SpawnVillager(GameObject tc)
        {
            Spawner.Instance.Entities.Spawn(Entities.Entity.Villager, tc.GetComponent<TownCenterController>().SpawnPos, GetComponent<EntityController>().Affiliation);
        }

        public override void UpdateBuildPoints()
        {
            transform.localScale = new Vector3(transform.localScale.x, 
                transform.localScale.y,
                Mathf.Max(startingScaleY / 10, startingScaleY * Mathf.Max(1, CurrentBuildPoints) / Mathf.Max(1, TotalBuildPoints)));
        }
    }
}
