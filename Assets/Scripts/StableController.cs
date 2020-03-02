using UnityEngine;

namespace Forgeage
{
	public class StableController : BuildingController
	{
        public static int knightFoodPrice = 50;
        public static int knightWoodPrice = 0;
        public static int knightGoldPrice = 25;

        void Start()
        {
            SpawnPos = transform.position + new Vector3(0, 0, -10);
            Actions.Add(new Action(Spawner.Instance.Entities.Archer.GetComponent<EntityController>().UITexture, CreateKnight));
        }

        void CreateKnight()
        {
            foreach (var go in ActionHandler.Instance.Selected)
            {
                if (go.GetComponent<StableController>() != null)
                {
                    if (ResourcesStockpileController.Instance.AttemptTransaction(woodC: knightWoodPrice, foodC: knightFoodPrice, goldC: knightGoldPrice))
                    {
                        Spawner.Instance.Entities.Spawn(Entities.Entity.Swordsman, go.GetComponent<StableController>().SpawnPos, GetComponent<EntityController>().Affiliation);
                    }
                }
            }
        }

        public override void UpdateBuildPoints()
        {
            transform.localScale = new Vector3(transform.localScale.x,
                transform.localScale.y,
                Mathf.Max(startingScaleY / 10, startingScaleY * Mathf.Max(1, CurrentBuildPoints) / Mathf.Max(1, TotalBuildPoints)));
        }
    }
}
