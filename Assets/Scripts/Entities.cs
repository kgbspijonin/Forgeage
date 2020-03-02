using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;

namespace Forgeage
{
	public class Entities : MonoBehaviourPunCallbacks
	{
        public enum Entity
        {
            TownCenter,
            Farm,

            Barracks,
            ArcheryRange,
            Dock,

            Villager,
            Swordsman,
            Archer,
            Hero,
            Galley,

            Cow,
            Wolf,

            Tree_A,
            Tree_B,
            Gold_A,
            Gold_B,

            Arrow,
            Torpedo
        }

        public GameObject Terrain;
        public GameObject Camera;

        public GameObject TownCenter;
        public GameObject Farm;

        public GameObject Barracks;
        public GameObject ArcheryRange;
        public GameObject Dock;

        public GameObject Villager;
        public GameObject Swordsman;
        public GameObject Archer;
        public GameObject Hero;
        public GameObject Galley;

        public GameObject Wolf;
        public GameObject Cow;

        public GameObject Tree_A;
        public GameObject Tree_B;
        public GameObject Gold_A;
        public GameObject Gold_B;

        public GameObject Arrow;
        public GameObject Torpedo;

        public Sprite IcRunning;
        public Sprite IcCalendar;
        public Sprite IcKnighthood;
        public Sprite IcArmor;
        public Sprite IcComposite;
        public Sprite IcCraftsmanship;
        public Sprite IcPlating;
        public Sprite IcHeating;
        public Sprite IcHeal;
        public Sprite IcBigArrow;
        public Sprite IcKillCow;

        public List<GameObject> gaiaeintities;
        public GameObject gaiaparent;

        public List<GameObject> player1entities;
        public GameObject player1parent;

        public List<GameObject> player2entities;
        public GameObject player2parent;

        public Dictionary<Entity, GameObject> Spawnables = new Dictionary<Entity, GameObject>();

        void Awake()
        {
            Spawnables.Add(Entity.TownCenter, TownCenter);
            Spawnables.Add(Entity.Villager, Villager);
            Spawnables.Add(Entity.Barracks, Barracks);
            Spawnables.Add(Entity.ArcheryRange, ArcheryRange);
            Spawnables.Add(Entity.Swordsman, Swordsman);
            Spawnables.Add(Entity.Archer, Archer);
            Spawnables.Add(Entity.Farm, Farm);
            Spawnables.Add(Entity.Dock, Dock);
            Spawnables.Add(Entity.Galley, Galley);
            Spawnables.Add(Entity.Hero, Hero);
            Spawnables.Add(Entity.Cow, Cow);
            Spawnables.Add(Entity.Wolf, Wolf);
            Spawnables.Add(Entity.Tree_A, Tree_A);
            Spawnables.Add(Entity.Tree_B, Tree_B);
            Spawnables.Add(Entity.Gold_A, Gold_A);
            Spawnables.Add(Entity.Gold_B, Gold_A);
            Spawnables.Add(Entity.Arrow, Arrow);
            Spawnables.Add(Entity.Torpedo, Torpedo);
        }

        public GameObject Spawn(Entity e, Vector3 position, int player = 0, bool isCompleted = false, Quaternion rotation = new Quaternion())
        {
            bool isBuilding = false;
            GameObject prefab = Spawnables[e];
            if (prefab.GetComponent<BuildingController>() != null)
            {
                isBuilding = true;
            }
            var obj = PhotonNetwork.Instantiate(prefab.transform.name, position, rotation);
            if (player != -1)
            {
                obj.GetComponent<EntityController>().Affiliation = player;
            }
            if (isBuilding)
            {
                obj.GetComponent<BuildingController>().startingScaleY = prefab.transform.localScale.y;
                if (!isCompleted)
                {
                    obj.GetComponent<BuildingController>().CurrentBuildPoints = 0;
                }
                else
                {
                    obj.GetComponent<BuildingController>().CurrentBuildPoints = obj.GetComponent<BuildingController>().TotalBuildPoints;
                }
                obj.transform.rotation = prefab.transform.rotation;
            }
            if (player == 1)
            {
                obj.transform.parent = player1parent.transform;
                player1entities.Add(obj);
            }
            if (player == 2)
            {
                obj.transform.parent = player2parent.transform;
                player2entities.Add(obj);
            }
            if (obj.GetComponent<EntityController>() != null)
            {
                obj.GetComponent<EntityController>().InitFinished();
            }
            return obj;
        }
	}
}
