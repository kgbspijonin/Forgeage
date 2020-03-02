using Forgeage.Extensions;
using UnityEngine;

namespace Forgeage
{
	public class ManagedAction : IExecutable
	{
        public ManagedAction(GameObject manager, Sprite UITexture, ManagedActionDelegate action)
        {
            this.manager = manager;
            this.UITexture = UITexture;
            this.action = action;
        }

        public GameObject manager;
        public Sprite UITexture;
        public ManagedActionDelegate action;

        public void Execute()
        {
            action.Invoke(manager);
        }
    }
}
