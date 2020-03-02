using UnityEngine;

namespace Forgeage
{
	public class Spawner : Singleton<Spawner>
	{
        public Entities Entities;

        void Awake()
        {
            Entities = GameObject.Find("Spawner").GetComponent<Entities>();
            Entities.player1parent = GameObject.Find("Player 1 Entities");
            Entities.player2parent = GameObject.Find("Player 2 Entities");
            Entities.gaiaparent = GameObject.Find("Gaia Parent");
        }
    }
}
