using UnityEngine;

namespace Forgeage
{
	public class FarmController : BuildingController
	{
        void Start()
        {
            if (IsCompleted)
            {
                IsCompleted = true;
            }
        }

        public override void OnCompleted()
        {
            gameObject.layer = LayerMask.NameToLayer("Natural Resources");
        }

        public override void UpdateBuildPoints()
        {
            transform.localScale = new Vector3(transform.localScale.x, Mathf.Max(startingScaleY / 10, startingScaleY * Mathf.Max(1, CurrentBuildPoints) / Mathf.Max(1, TotalBuildPoints)), transform.localScale.z);
        }
    }
}
