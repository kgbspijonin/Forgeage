using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace Forgeage
{
	public class ResourcesStockpileController : Singleton<ResourcesStockpileController>
	{
        public Text FoodCount;
        public Text WoodCount;
        public Text GoldCount;

        private int food;
        private int wood;
        private int gold;

        public int Food {
            get { return food; }
            set { food = value >= 0 ? value : 0; FoodUpdated.Invoke(this, EventArgs.Empty); }
        }
        public int Wood {
            get { return wood; }
            set { wood = value >= 0 ? value : 0; WoodUpdated.Invoke(this, EventArgs.Empty); }
        }
        public int Gold {
            get { return gold; }
            set { gold = value >= 0 ? value : 0; GoldUpdated.Invoke(this, EventArgs.Empty); }
        }

        public event EventHandler FoodUpdated;
		public event EventHandler WoodUpdated;
		public event EventHandler GoldUpdated;

        private void Awake()
        {
            FoodUpdated += UpdateFoodCount;
            WoodUpdated += UpdateWoodCount;
            GoldUpdated += UpdateGoldCount;

            Food = 2500;
            Wood = 2500;
            Gold = 1000;
        }

        public bool CheckForWood(int count)
        {
            return wood >= count;
        }

        public bool CheckForFood(int count)
        {
            return food >= count;
        }

        public bool CheckForGold(int count)
        {
            return gold >= count;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool AttemptTransaction(int woodC = 0, int foodC = 0, int goldC = 0)
        {
            if(CheckForWood(woodC) && CheckForFood(foodC) && CheckForGold(goldC))
            {
                Wood -= woodC;
                Food -= foodC;
                Gold -= goldC;
                return true;
            }
            return false;
        }

        void UpdateFoodCount(object sender, EventArgs args)
        {
            FoodCount.text = Food.ToString();
        }

        void UpdateWoodCount(object sender, EventArgs args)
        {
            WoodCount.text = Wood.ToString();
        }

        void UpdateGoldCount(object sender, EventArgs args)
        {
            GoldCount.text = Gold.ToString();
        }
    }
}
