using UnityEngine;

namespace Forgeage
{
	public class UIImageController : MonoBehaviour
	{
        public GameObject entity;
        public Action action;

		public void Click()
        {
            action?.Execute();
        }
	}
}
