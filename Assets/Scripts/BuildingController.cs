using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Forgeage
{
	public class BuildingController : Controller
	{
        public Vector3 SpawnPos;

        protected static float trainTime = 1;
        protected static float researchTime = 3;

        public int TotalBuildPoints = 100;

        private int currentBuildPoints;

        public float startingScaleY;

        public ObservableCollection<TimedAction> queue = new ObservableCollection<TimedAction>();

        private bool isCompleted;
        protected bool completedCalled = false;

        public bool IsCompleted {
            get {
                return isCompleted;
            }
            set {
                isCompleted = value;
                currentBuildPoints = TotalBuildPoints;
                if(isCompleted && !completedCalled)
                {
                    completedCalled = true;
                    OnCompleted();
                }
            }
        }

        
        public int CurrentBuildPoints {
            get {
                return currentBuildPoints;
            }
            set {
                currentBuildPoints = value;
                if (currentBuildPoints >= TotalBuildPoints)
                {
                    IsCompleted = true;
                    currentBuildPoints = TotalBuildPoints;
                }
                GetComponent<EntityController>().CurrentHP = ((float)CurrentBuildPoints / TotalBuildPoints) * GetComponent<EntityController>().MaxHP;
                UpdateBuildPoints();
            }
        }

        void Start()
        {
            startingScaleY = transform.localScale.y;
            queue.CollectionChanged += HandleQueue;
            if (IsCompleted)
            {
                IsCompleted = true;
            }
        }

        public virtual void OnCompleted()
        {

        }

        public void Build(VillagerController builder)
        {
            CurrentBuildPoints += 5;
        }

        public virtual void UpdateBuildPoints()
        {
            transform.localScale = new Vector3(transform.localScale.x, Mathf.Max(startingScaleY / 10, startingScaleY * Mathf.Max(1, CurrentBuildPoints) / Mathf.Max(1, TotalBuildPoints)), transform.localScale.z);
        }

        protected virtual void HandleQueue(object sender, EventArgs args)
        {
            if(queue.Count > 0 && queue[0].coroutine == null)
            {
                queue[0].coroutine = StartCoroutine(queue[0].UpdateProgress());
            }
        }
	}
}
