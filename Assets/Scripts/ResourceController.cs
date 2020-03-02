using UnityEngine;

namespace Forgeage
{
	public class ResourceController : MonoBehaviour
	{
        protected int resourceCount = 200;

        public int GatherResource(VillagerController chopper)
        {
            resourceCount--;
            if (resourceCount < 1)
            {
                Destroy(gameObject);
            }
            return 1;
        }
    }
}
